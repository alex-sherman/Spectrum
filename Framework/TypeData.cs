using IronPython.Runtime.Types;
using Spectrum.Framework.Content;
using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public class MemberInfo
    {

        private FieldInfo field;
        private PropertyInfo property;
        public MemberInfo(FieldInfo field)
        {
            this.field = field;
            PreloadedContent = field.GetCustomAttributes().FirstOrDefault(attr => attr is PreloadedContentAttribute) as PreloadedContentAttribute;
        }
        public MemberInfo(PropertyInfo property)
        {
            this.property = property;
            PreloadedContent = property.GetCustomAttributes().FirstOrDefault(attr => attr is PreloadedContentAttribute) as PreloadedContentAttribute;
        }
        public Type MemberType { get { return property?.PropertyType ?? field?.FieldType; } }
        public void SetValue(object obj, object value) { property?.SetValue(obj, value); field?.SetValue(obj, value); }
        public object GetValue(object obj) { return property?.GetValue(obj) ?? field?.GetValue(obj); }
        public PreloadedContentAttribute PreloadedContent = null;
        public bool IsStatic = false;
    }
    public class TypeData
    {
        public Dictionary<string, MemberInfo> members = new Dictionary<string, MemberInfo>();
        public Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        public List<string> ReplicatedMemebers = new List<string>();
        public List<string> ReplicatedMethods = new List<string>();
        public Type Type { get; private set; }
        private IronPythonTypeWrapper pythonWrapper;
        public TypeData(IronPythonTypeWrapper type)
            : this(type.Type)
        {
            pythonWrapper = type;
        }
        public object Instantiate(params object[] args)
        {
            if (pythonWrapper != null)
                return pythonWrapper.Activator();

            if (args == null) args = new object[0];
            Type[] types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                types[i] = args[i].GetType();
            }
            ConstructorInfo cinfo = Type.GetConstructor(types);
            if (cinfo == null)
            {
                throw new InvalidOperationException("Unable to construct an entity with the given parameters");
            }
#if !DEBUG
            try
            {
#endif
                object output = cinfo.Invoke(args);

                foreach (MemberInfo member in members.Values.Where(member => !member.IsStatic && member.PreloadedContent != null))
                {
                    member.SetValue(output, ContentHelper.LoadType(member.MemberType, member.PreloadedContent.Path));
                }
                return output;
#if !DEBUG
            }
            catch
            {
                throw new Exception("An error occured constructing the entity");
            }
#endif
        }
        public TypeData(Type type)
        {
            Type = type;
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var info = members[field.Name] = new MemberInfo(field);
                if (field.GetCustomAttributes().Where(attr => attr is ReplicateAttribute).Any())
                    ReplicatedMemebers.Add(field.Name);
            }
            foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public))
                members[field.Name] = new MemberInfo(field) { IsStatic = true };
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var info = members[property.Name] = new MemberInfo(property);
                if (property.GetCustomAttributes().Where(attr => attr is ReplicateAttribute).Any())
                    ReplicatedMemebers.Add(property.Name);
            }
            foreach (var property in type.GetProperties(BindingFlags.Static | BindingFlags.Public))
                members[property.Name] = new MemberInfo(property) { IsStatic = true };
            //TODO: Check for overloaded methods
            foreach (var method in type.GetMethods())
            {
                if (!method.IsStatic && method.IsPublic)
                    methods[method.Name] = method;
                if (method.GetCustomAttributes(true).ToList().Any(x => x is ReplicateAttribute))
                {
                    ReplicatedMethods.Add(method.Name);
                }
            }
        }
        private bool Coerce(Type type, object value, out object output)
        {
            output = value;
            if (value == null || type.IsAssignableFrom(value.GetType()))
                return true;
            else if (type.IsSubclassOf(typeof(Enum)) && value is int)
            {
                output = Enum.ToObject(type, (int)value);
                return true;
            }
            else if (ContentHelper.ContentParsers.ContainsKey(type) && value is string)
            {
                output = ContentHelper.LoadType(type, value as string);
                return true;
            }
            else
            {
                // Try assigning a prefab or InitData
                if (value is string && Prefab.Prefabs.ContainsKey(value as string))
                    output = Prefab.Prefabs[value as string];
                if (value is string && TypeHelper.Types[value as string] != null)
                    output = new InitData(value as string);
                // Maybe the target field is type InitData
                if (type.IsAssignableFrom(output.GetType()))
                    return true;

                if (output is InitData)
                {
                    InitData initData = output as InitData;
                    // Construct an object to fill the field if we can
                    if (type.IsAssignableFrom(initData.TypeData.Type))
                    {
                        output = initData.Construct();
                        return true;
                    }
                }
            }
            return false;
        }
        public void Set(object obj, string name, object value)
        {
            if (members.ContainsKey(name))
            {
                MemberInfo info = members[name];
                object coercedValue;
                if (Coerce(info.MemberType, value, out coercedValue))
                    members[name].SetValue(obj, coercedValue);

            }
        }
        public object Get(object obj, string name)
        {
            if (members.ContainsKey(name))
            {
                return members[name].GetValue(obj);
            }
            return null;
        }
        public object Call(object obj, string name, params object[] args)
        {
            if (methods.ContainsKey(name))
            {
                var method = methods[name];
                var methodArgs = method.GetParameters();
                if (args.Length != methodArgs.Length) { return null; }
                var coercedArgs = new List<object>();
                foreach (var argPair in methodArgs.Zip(args, (a, b) => new Tuple<Type, object>(a.ParameterType, b)))
                {
                    object coerced;
                    if (!Coerce(argPair.Item1, argPair.Item2, out coerced))
                        return null;
                    coercedArgs.Add(coerced);
                }
                return methods[name].Invoke(obj, coercedArgs.ToArray());
            }
            return null;
        }
        public Type MemberType(string name)
        {
            return members.ContainsKey(name) ? members[name].MemberType : null;
        }
    }
}
