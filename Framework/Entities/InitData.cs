using Microsoft.Xna.Framework;
using ProtoBuf;
using Spectrum.Framework.Network;
using Spectrum.Framework.Network.Surrogates;
using Spectrum.Framework.Physics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        public static void Register(string name, InitData data)
        {
            prefabs[name] = data.ToImmutable();
            prefabs[name].Name = name;
        }
        public static object Construct(string name)
        {
            if (!prefabs.ContainsKey(name)) return null;
            return prefabs[name].Construct();
        }
        #endregion
        public string Name;
        public string TypeName;
        public Primitive[] Args = new Primitive[0];
        public Dictionary<string, Primitive> Fields = new Dictionary<string, Primitive>();
        public DefaultDict<string, Dictionary<string, Primitive>> Data
            = new DefaultDict<string, Dictionary<string, Primitive>>(() => new Dictionary<string, Primitive>(), true);
        public List<FunctionCall> FunctionCalls = new List<FunctionCall>();
        internal InitData(TypeData typeData) { TypeData = typeData; }
        public InitData(string type, params object[] args)
        {
            TypeData = TypeHelper.Types.GetData(type);
            if (TypeData == null)
                throw new KeyNotFoundException($"Could not find type {type} in TypeData lookup");
            TypeName = type;
            Args = args.Select(obj => new Primitive(obj)).ToArray();
        }
        [ProtoIgnore]
        public readonly TypeData TypeData;
        public object Construct()
        {
            if (TypeData == null)
            {
                DebugPrinter.print($"Failed to construct {TypeName}");
                return null;
            }
            object output = TypeData.Instantiate(Args.Select(prim => prim.Object).ToArray());
            Apply(output, true);
            return output;
        }
        public void Apply(object target, bool firstCall = false)
        {
            foreach (var field in Fields)
            {
                TypeData.Set(target, field.Key, field.Value.Object);
            }
            foreach (var dict in Data)
            {
                if (!(TypeData.Get(target, dict.Key) is IDictionary<string, object> targetDict))
                {
                    DebugPrinter.PrintOnce($"Failed to find data dictionary {dict.Key} in {TypeName}");
                    continue;
                }
                foreach (var kvp in dict.Value)
                {
                    targetDict[kvp.Key] = kvp.Value.Object;
                }
            }
            TypeData.Set(target, "TypeName", TypeName);
            foreach (var call in FunctionCalls)
            {
                if (!call.CallOnce || firstCall)
                    TypeData.Call(target, call.Name, call.Args.Select((prim) => prim.Object).ToArray());
            }
            if (target is IReplicatable)
            {
                var rep = (target as IReplicatable);
                rep.ReplicationData = new ReplicationData(TypeData, rep);
                if (firstCall)
                    rep.InitData = Clone();
            }
        }
        public virtual InitData SetDict(string key, object value, string dictField = "Data")
        {
            Data[dictField][key] = new Primitive(value);
            return this;
        }
        public virtual InitData Set(string name, object value)
        {
            Fields[name] = new Primitive(value);
            return this;
        }
        public virtual InitData Call(string name, params object[] args)
        {
            FunctionCalls.Add(new FunctionCall(name, false, args));
            return this;
        }
        public virtual InitData CallOnce(string name, params object[] args)
        {
            FunctionCalls.Add(new FunctionCall(name, true, args));
            return this;
        }
        protected void CopyFieldsTo(InitData other)
        {
            other.Name = Name;
            other.TypeName = TypeName;
            other.Args = Args.Select(prim => new Primitive(prim.Object)).ToArray();
            other.Fields = Fields.ToDictionary(kvp => kvp.Key, kvp => new Primitive(kvp.Value.Object));
            other.Data = Data.Copy();
            other.FunctionCalls = FunctionCalls.ToList();
        }
        public InitData Clone()
        {
            InitData output = new InitData(TypeData);
            CopyFieldsTo(output);
            return output;
        }
        public ImmultableInitData ToImmutable()
        {
            ImmultableInitData output = new ImmultableInitData(TypeData);
            output.Name = Name;
            output.TypeName = TypeName;
            output.Args = Args;
            output.Fields = new Dictionary<string, Primitive>(Fields);
            output.FunctionCalls = FunctionCalls.ToList();
            return output;
        }
    }
    public class InitData<T> : InitData where T : class
    {
        internal InitData(TypeData typeData) : base(typeData) { }
        public InitData(params object[] args) : base(typeof(T).Name, args) { }
        new public InitData<T> Clone()
        {
            var output = new InitData<T>(TypeData);
            CopyFieldsTo(output);
            return output;
        }
        new public InitData<T> SetDict(string key, object value, string dictField = "Data")
        {
            base.SetDict(key, value, dictField);
            return this;
        }
        new public virtual InitData<T> Set(string name, object value)
        {
            base.Set(name, value);
            return this;
        }
        new public virtual InitData<T> Call(string name, params object[] args)
        {
            base.Call(name, args);
            return this;
        }
        new public virtual InitData<T> CallOnce(string name, params object[] args)
        {
            base.CallOnce(name, args);
            return this;
        }
        public new T Construct() => base.Construct() as T;
    }
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [LoadableType]
    public class ImmultableInitData : InitData
    {
        internal ImmultableInitData(TypeData typeData) : base(typeData) { }
        public ImmultableInitData(string type, params object[] args) : base(type, args) { }
        public override InitData Set(string name, object value)
            => Clone().Set(name, value);
        public override InitData Call(string name, params object[] args)
            => Clone().Call(name, args);
        public override InitData CallOnce(string name, params object[] args)
            => Clone().CallOnce(name, args);
    }
}
