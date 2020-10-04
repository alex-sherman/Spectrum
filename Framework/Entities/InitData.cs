using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using ProtoBuf;
using Replicate;
using Replicate.MetaData;
using Spectrum.Framework.Content;
using Spectrum.Framework.Network;
using Spectrum.Framework.Network.Surrogates;
using Spectrum.Framework.Physics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Spectrum.Framework.Entities
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public struct FunctionCall
    {
        public string Name;
        public bool CallOnce;
        public Primitive[] Args;

        public FunctionCall(string name, bool callOnce, object[] args)
        {
            Name = name;
            CallOnce = callOnce;
            Args = args.Select((arg) => new Primitive(arg)).ToArray();
        }
    }
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [LoadableType]
    public class InitData
    {
        #region Prefabs
        private static Dictionary<string, InitData> prefabs = new Dictionary<string, InitData>();
        public static IReadOnlyDictionary<string, InitData> Prefabs
        {
            get { return prefabs; }
        }
        public static InitData Get(string name)
        {
            if (prefabs.TryGetValue(name, out var initData))
            {
                return initData;
            }
            if (TypeHelper.Model.Types.TryGetValue(name, out var typeData))
            {
                var typeAccessor = TypeHelper.Model.GetTypeAccessor(typeData.Type);
                if (typeAccessor != null) return new InitData(typeAccessor);
            }

            throw new KeyNotFoundException($"No InitData found for \"{name}\"");
        }
        public static InitData<T> Get<T>(string name = null) where T : class
        {
            var initData = Get(name ?? typeof(T).Name);
            if (!typeof(T).IsAssignableFrom(initData.TypeData.Type)) throw new InvalidCastException($"{typeof(T).Name} != {initData.TypeName}");
            var output = new InitData<T>(initData.TypeData);
            initData.CopyFieldsTo(output);
            return output;
        }
        public static InitData Register(string name, InitData data)
        {
            var immutableData = prefabs[name] = data.ToImmutable();
            prefabs[name].Name = name;
            return immutableData;
        }
        public static InitData<T> Register<T>(string name, InitData<T> data) where T : class
        {
            var immutableData = data.ToImmutable();
            Register(name, (InitData)immutableData);
            return immutableData;
        }
        public static object Construct(string name)
        {
            if (!prefabs.ContainsKey(name)) return null;
            return prefabs[name].Construct();
        }
        #endregion
        public bool Immutable { get; protected set; }
        public string Name;
        public string Path { get; set; }
        public string FullPath { get; set; }
        public string TypeName => TypeData.Type.Name;
        public Primitive[] Args = new Primitive[0];
        public InitData SetArgs(params object[] args)
        {
            Args = args.Select(obj => new Primitive(obj)).ToArray();
            return this;
        }
        /// <summary>
        /// Once stored, all fields are set via reference. This may lead
        /// to strange side effects if GameObjects mutate field values that were
        /// set via InitData. Instead be sure to copy any values that must be mutated.
        /// </summary>
        public Dictionary<string, Primitive> Fields = new Dictionary<string, Primitive>();
        // TODO: This should be readonly
        public Dictionary<string, Primitive> Data = new Dictionary<string, Primitive>();
        public List<FunctionCall> FunctionCalls = new List<FunctionCall>();
        internal InitData(TypeAccessor typeData) { TypeData = typeData; }
        public InitData(string type, params object[] args)
        {
            TypeData = TypeHelper.Model.GetTypeAccessor(TypeHelper.Model.Types[type].Type);
            if (TypeData == null)
                throw new KeyNotFoundException($"Could not find type {type} in TypeData lookup");
            SetArgs(args);
        }
        [ProtoIgnore]
        [JsonIgnore]
        public TypeAccessor TypeData;
        public object Construct()
        {
            if (TypeData == null)
            {
                DebugPrinter.Print($"Failed to find type {TypeName}");
                return null;
            }
            object output = TypeData.Construct(Args.Select(prim => prim.Object).ToArray());
            Apply(output, true);
            return output;
        }
        public void Apply(object target, bool firstCall = false)
        {
            foreach (var field in Fields)
            {
                try
                {
                    MemberAccessor info = TypeData[field.Key];
                    if (Coerce(info.Type, field.Value.Object, out var coercedValue))
                        info.SetValue(target, coercedValue);
                    else
                        DebugPrinter.PrintOnce($"Failed to coerce {info.Type.Name}.{field.Key}");
                }
                catch (Exception e)
                {
                    DebugPrinter.Print($"Failed to set field {field.Key} in {TypeName}\n{e}");
                }
            }
            TypeData["TypeName"]?.SetValue(target, TypeName);
            foreach (var call in FunctionCalls)
            {
                if (!call.CallOnce || firstCall)
                    Call(target, call.Name, call.Args.Select((prim) => prim.Object).ToArray());
            }
            if (target is IReplicatable)
            {
                var rep = (target as IReplicatable);
                rep.ReplicationData = new ReplicationData(TypeData, rep);
                if (firstCall)
                    rep.InitData = Clone();
            }
        }
        private object Call(object obj, string name, params object[] args)
        {
            var method = TypeData.Methods[name];
            if (method == null)
            {
                DebugPrinter.PrintOnce($"Method not found {TypeData.Name}.{name}");
                return null;
            }
            var methodArgs = method.GetParameters();
            var coercedArgs = methodArgs.Select((param, i) =>
            {
                if (i < args.Length)
                {
                    if (!Coerce(param.ParameterType, args[i], out object coerced)) return null;
                    return coerced;
                }
                if (param.HasDefaultValue) return param.DefaultValue;
                return null;
            }).ToArray();
            try
            {
                return method.Invoke(obj, coercedArgs);
            }
            catch (Exception e)
            {
                DebugPrinter.Print($"Exception encountered calling {TypeData.Name}.{name}: {e}");
            }
            return null;
        }
        static Dictionary<Tuple<Type, Type>, MethodInfo> castMethodCache = new Dictionary<Tuple<Type, Type>, MethodInfo>();
        public static bool TryCast(Type type, object value, out object output)
        {
            output = value;
            Type from = value.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GetGenericArguments()[0];
            if (value == null || type.IsAssignableFrom(from))
                return true;
            try
            {
                // The next to checks might be super slow, consider caching or removing them
                // if it becomes a problem
                if (PrimitiveTypeMap.Contains(type))
                {
                    if (type.IsSubclassOf(typeof(Enum)) && value is int)
                        output = Enum.ToObject(type, (int)value);
                    else
                        output = Convert.ChangeType(value, type);
                    return true;
                }
                var key = new Tuple<Type, Type>(from, type);
                if (!castMethodCache.TryGetValue(key, out MethodInfo method))
                {
                    method = value.GetType().GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Union(type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                        .Where(m => m.Name == "op_Implicit" && m.GetParameters()[0].ParameterType == from && m.ReturnType == type).FirstOrDefault();
                    castMethodCache[key] = method;
                }
                if (method != null)
                {
                    output = method.Invoke(null, new object[] { value });
                    return true;
                }
            }
            catch (Exception e) { DebugPrinter.PrintOnce($"Failed to coerce {from} to {type}: {e}"); }
            return false;
        }
        public static bool Coerce(Type type, object value, out object output)
        {
            if (TryCast(type, value, out output))
                return true;
            // Try assigning a prefab or InitData
            if (value is string && Prefabs.ContainsKey(value as string))
                output = Prefabs[value as string];
            if (value is string && TypeHelper.Model.Types[value as string] != null)
                output = new InitData(value as string);
            // Maybe the target field is type InitData
            if (type.IsAssignableFrom(output.GetType()))
                return true;
            // Construct an object to fill the field if we can
            if (output is InitData initData && type.IsAssignableFrom(initData.TypeData.Type))
            {
                output = initData.Construct();
                return true;
            }
            // Try content last
            if (ContentHelper.ContentParsers.ContainsKey(type) && value is string)
            {
                output = ContentHelper.LoadType(type, value as string);
                if (output != null)
                    return true;
            }
            return false;
        }
        public virtual InitData SetData(string key, object value)
        {
            if (Immutable) return Clone().SetData(key, value);
            Data[key] = new Primitive(value);
            return this;
        }
        public virtual InitData Set(string name, object value)
        {
            if (TypeData[name] == null)
                DebugPrinter.PrintOnce($"No such field {TypeData.Name}.{name}");
            else
            {
                if (Immutable) return Clone().Set(name, value);
                Fields[name] = new Primitive(value);
            }
            return this;
        }
        public virtual InitData Unset(string name)
        {
            if (Immutable) return Clone().Unset(name);
            Fields.Remove(name);
            return this;
        }
        public virtual InitData Call(string name, params object[] args)
        {
            if (Immutable) return Clone().Call(name, args);
            FunctionCalls.Add(new FunctionCall(name, false, args));
            return this;
        }
        public virtual InitData CallOnce(string name, params object[] args)
        {
            if (Immutable) return Clone().CallOnce(name, args);
            FunctionCalls.Add(new FunctionCall(name, true, args));
            return this;
        }
        /// <summary>
        /// Specifically does not copy the immutable flag such that clones begin mutable
        /// </summary>
        /// <param name="other"></param>
        protected void CopyFieldsTo(InitData other)
        {
            other.Name = Name;
            other.Path = Path;
            other.FullPath = FullPath;
            other.Args = Args.Select(prim => new Primitive(prim.Object)).ToArray();
            other.Fields = Fields.ToDictionary(kvp => kvp.Key, kvp => new Primitive(kvp.Value.Object));
            other.Data = Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            other.FunctionCalls = FunctionCalls.ToList();
        }
        public InitData Clone()
        {
            InitData output = new InitData(TypeData);
            CopyFieldsTo(output);
            return output;
        }
        public InitData ToImmutable()
        {
            Immutable = true;
            return this;
        }
    }
    public class InitData<T> : InitData where T : class
    {
        internal InitData(TypeAccessor typeData) : base(typeData) { }
        public InitData(params object[] args) : base(typeof(T).Name, args) { }
        public InitData(Expression<Func<T>> exp) : base(typeof(T).Name)
        {
            NewExpression newExpression = null;
            if (exp.Body is MemberInitExpression init)
            {
                newExpression = init.NewExpression;
                Fields = init.Bindings.ToDictionary(b => b.Member.Name, b =>
                {
                    if (b is MemberAssignment assign)
                    {
                        try
                        {
                            if (assign.Expression is UnaryExpression convert && convert.NodeType == ExpressionType.Convert)
                                return new Primitive(convert.Operand.GetConstantValue());
                            return new Primitive(assign.Expression.GetConstantValue());
                        }
                        catch (ArgumentException e) { throw new ArgumentException($"Invalid member assignment for {b.Member.Name}", e); }
                    }
                    else if (b is MemberListBinding bind)
                    {
                        throw new NotImplementedException();
                        // TODO
                    }
                    throw new ArgumentException("All member bindings must be assignments");
                });
            }
            else if (exp.Body is NewExpression) newExpression = exp.Body as NewExpression;
            else throw new ArgumentException("Expression must be a member initializer");

            Args = newExpression.Arguments.Select(e => new Primitive(e.GetConstantValue())).ToArray();
        }
        new public InitData<T> Clone()
        {
            var output = new InitData<T>(TypeData);
            CopyFieldsTo(output);
            return output;
        }
        new public InitData<T> SetData(string key, object value)
        {
            if (Immutable) return Clone().SetData(key, value);
            base.SetData(key, value);
            return this;
        }
        new public InitData<T> Set(string name, object value)
        {
            if (Immutable) return Clone().Set(name, value);
            base.Set(name, value);
            return this;
        }
        public InitData<T> Set<U>(Expression<Func<T, U>> lambda, U value)
        {
            if (!(lambda.Body is MemberExpression member))
                throw new InvalidOperationException("Must be member expression");
            if (Immutable) return Clone().Set(member.Member.Name, value);
            return Set(member.Member.Name, value);
        }
        new public InitData<T> SetArgs(params object[] args)
        {
            if (Immutable) return Clone().SetArgs(args);
            base.SetArgs(args);
            return this;
        }
        new public InitData<T> Unset(string name)
        {
            if (Immutable) return Clone().Unset(name);
            base.Unset(name);
            return this;
        }
        new public InitData<T> Call(string name, params object[] args)
        {
            if (Immutable) return Clone().Call(name, args);
            base.Call(name, args);
            return this;
        }
        new public InitData<T> CallOnce(string name, params object[] args)
        {
            if (Immutable) return Clone().CallOnce(name, args);
            base.CallOnce(name, args);
            return this;
        }
        new public InitData<T> ToImmutable()
        {
            base.ToImmutable();
            return this;
        }
        public new T Construct() => base.Construct() as T;
        public InitData ToNonGeneric() => base.Clone();
    }
}
