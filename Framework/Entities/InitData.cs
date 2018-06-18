﻿using Microsoft.Xna.Framework;
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
        public Primitive[] Args;
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
        public Primitive[] args = new Primitive[0];
        public Dictionary<string, Primitive> fields = new Dictionary<string, Primitive>();
        public List<FunctionCall> FunctionCalls = new List<FunctionCall>();
        internal InitData() { }
        public InitData(string type, params object[] args)
        {
            TypeName = type;
            this.args = args.Select(obj => new Primitive(obj)).ToArray();
        }
        [ProtoIgnore]
        public TypeData TypeData { get { return TypeHelper.Types.GetData(TypeName); } }
        public object Construct()
        {
            if (TypeData == null)
            {
                DebugPrinter.print(string.Format("Failed to construct {0}", TypeName));
                return null;
            }
            object output = TypeData.Instantiate(args.Select(prim => prim.Object).ToArray());
            foreach (var field in fields)
            {
                TypeData.Set(output, field.Key, field.Value.Object);
            }
            TypeData.Set(output, "TypeName", TypeName);
            foreach (var call in FunctionCalls)
            {
                TypeData.Call(output, call.Name, call.Args.Select((prim) => prim.Object).ToArray());
            }
            if (output is IReplicatable)
            {
                var rep = (output as IReplicatable);
                rep.ReplicationData = new ReplicationData(this.Clone(), rep);
            }
            return output;
        }
        public T Construct<T>() where T : class
        {
            return Construct() as T;
        }
        public virtual InitData Set(string name, object value)
        {
            if (value is Enum)
                value = (int)value;
            fields[name] = new Primitive(value);
            return this;
        }
        public virtual InitData Call(string name, params object[] args)
        {
            FunctionCalls.Add(new FunctionCall() { Name = name, Args = args.Select((arg) => new Primitive(arg)).ToArray() });
            return this;
        }
        public InitData Clone()
        {
            InitData output = new InitData();
            output.TypeName = TypeName;
            output.args = args.Select(prim => new Primitive(prim.Object)).ToArray();
            output.fields = fields.ToDictionary(kvp => kvp.Key, kvp => new Primitive(kvp.Value.Object));
            output.FunctionCalls = FunctionCalls.ToList();
            return output;
        }
        public ImmultableInitData ToImmutable()
        {
            ImmultableInitData output = new ImmultableInitData();
            output.TypeName = TypeName;
            output.args = args;
            output.fields = new Dictionary<string, Primitive>(fields);
            output.FunctionCalls = FunctionCalls.ToList();
            return output;
        }
    }
    public class InitData<T> : InitData
    {
        public InitData(params object[] args) : base(typeof(T).Name, args) { }
        new public virtual InitData<T> Set(string name, object value)
        {
            base.Set(name, value);
            return this;
        }
    }
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [LoadableType]
    public class ImmultableInitData : InitData
    {
        internal ImmultableInitData() { }
        public ImmultableInitData(string type, params object[] args) : base(type, args) { }
        public override InitData Set(string name, object value)
        {
            InitData output = Clone();
            return output.Set(name, value);
        }
    }
}
