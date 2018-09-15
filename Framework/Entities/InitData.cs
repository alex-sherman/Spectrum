﻿using Microsoft.Xna.Framework;
using Newtonsoft.Json;
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
        public bool Immutable { get; protected set; }
        public string Name;
        public string TypeName {
            get => TypeData.Type.Name;
            set => TypeData = TypeHelper.Types.GetData(value);
        }
        public Primitive[] Args = new Primitive[0];
        /// <summary>
        /// Once stored, all fields are set via reference. This may lead
        /// to strange side effects if GameObjects mutate field values that were
        /// set via InitData. Instead be sure to copy any values that must be mutated.
        /// </summary>
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
            Args = args.Select(obj => new Primitive(obj)).ToArray();
        }
        [ProtoIgnore]
        [JsonIgnore]
        public TypeData TypeData;
        public object Construct()
        {
            if (TypeData == null)
            {
                DebugPrinter.Print($"Failed to construct {TypeName}");
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
                try
                {
                    TypeData.Set(target, field.Key, field.Value.Object);
                }
                catch(Exception e)
                {
                    DebugPrinter.Print($"Failed to set field {field.Key} in {TypeName}\n{e}");
                }
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
            if (Immutable)
                return Clone().SetDict(key, value, dictField);
            Data[dictField][key] = new Primitive(value);
            return this;
        }
        public virtual InitData Set(string name, object value)
        {
            if (Immutable)
                return Clone().Set(name, value);
            Fields[name] = new Primitive(value);
            return this;
        }
        public virtual InitData Unset(string name)
        {
            if (Immutable)
                return Clone().Unset(name);
            Fields.Remove(name);
            return this;
        }
        public virtual InitData Call(string name, params object[] args)
        {
            if (Immutable)
                return Clone().Call(name, args);
            FunctionCalls.Add(new FunctionCall(name, false, args));
            return this;
        }
        public virtual InitData CallOnce(string name, params object[] args)
        {
            if (Immutable)
                return Clone().CallOnce(name, args);
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
        public InitData ToImmutable()
        {
            Immutable = true;
            return this;
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
        new public virtual InitData<T> Unset(string name)
        {
            base.Unset(name);
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
        new public virtual InitData<T> ToImmutable()
        {
            base.ToImmutable();
            return this;
        }
        public new T Construct() => base.Construct() as T;
        public InitData ToNonGeneric() => base.Clone();
    }
}
