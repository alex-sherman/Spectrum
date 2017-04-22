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
    class MemberInfo
    {
        private FieldInfo field;
        private PropertyInfo property;
        public MemberInfo(FieldInfo field)
        {
            this.field = field;
        }
        public MemberInfo(PropertyInfo property)
        {
            this.property = property;
        }
        public Type MemberType { get { return property?.PropertyType ?? field?.FieldType; } }
        public void SetValue(object obj, object value) { property?.SetValue(obj, value); field?.SetValue(obj, value); }
        public object GetValue(object obj) { return property?.GetValue(obj) ?? field?.GetValue(obj); }
    }
    public class TypeData
    {
        private Dictionary<string, MemberInfo> members = new Dictionary<string, MemberInfo>();
        public List<string> ReplicatedMemebers = new List<string>();
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
            try
            {
                object output = cinfo.Invoke(args);

                foreach (FieldInfo field in output.GetType().GetFields())
                {
                    foreach (object attribute in field.GetCustomAttributes(true).ToList())
                    {
                        PreloadedContentAttribute preload = attribute as PreloadedContentAttribute;
                        if (preload != null)
                        {
                            field.SetValue(output, ContentHelper.LoadType(field.FieldType, preload.Path));
                        }
                    }
                }
                return output;
            }
            catch
            {
                throw new Exception("An error occured constructing the entity");
            }
        }
        public TypeData(Type type)
        {
            Type = type;
            foreach (var field in type.GetFields())
            {
                members[field.Name] = new MemberInfo(field);
                if (field.GetCustomAttributes().Where(attr => attr is ReplicateAttribute).Any())
                    ReplicatedMemebers.Add(field.Name);
            }
            foreach (var property in type.GetProperties())
            {
                members[property.Name] = new MemberInfo(property);
                if (property.GetCustomAttributes().Where(attr => attr is ReplicateAttribute).Any())
                    ReplicatedMemebers.Add(property.Name);
            }
        }
        public void Set(object obj, string name, object value)
        {
            if (members.ContainsKey(name))
            {
                MemberInfo info = members[name];
                if (value == null || info.MemberType.IsAssignableFrom(value.GetType()))
                    members[name].SetValue(obj, value);
                else if (info.MemberType.IsSubclassOf(typeof(Enum)) && value is int)
                    members[name].SetValue(obj, Enum.ToObject(info.MemberType, (int)value));
                else if (ContentHelper.ContentParsers.ContainsKey(info.MemberType) && value is string)
                {
                    members[name].SetValue(obj, ContentHelper.LoadType(info.MemberType, value as string));
                }
                else
                {
                    // Try assigning a prefab or InitData
                    if (value is string && Prefab.Prefabs.ContainsKey(value as string))
                        value = Prefab.Prefabs[value as string];
                    if (value is InitData)
                    {
                        InitData initData = value as InitData;
                        // Maybe the target field is type InitData
                        if (info.MemberType.IsAssignableFrom(value.GetType()))
                            members[name].SetValue(obj, value);
                        // Construct an object to fill the field if we can
                        else if (info.MemberType.IsAssignableFrom(initData.TypeData.Type))
                            members[name].SetValue(obj, initData.Construct());
                    }
                }

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
        public Type MemberType(string name)
        {
            return members.ContainsKey(name) ? members[name].MemberType : null;
        }
    }
}
