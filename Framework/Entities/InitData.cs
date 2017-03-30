﻿using Microsoft.Xna.Framework;
using ProtoBuf;
using Spectrum.Framework.Network;
using Spectrum.Framework.Network.Surrogates;
using Spectrum.Framework.Physics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Spectrum.Framework.Entities
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class InitData
    {
        public string type;
        public Primitive[] args = new Primitive[0];
        public Dictionary<string, Primitive> fields = new Dictionary<string, Primitive>();
        internal InitData() { }
        public InitData(string type, params object[] args)
        {
            this.type = type;
            this.args = args.Select(obj => new Primitive(obj)).ToArray();
        }
        [ProtoIgnore]
        public TypeData TypeData { get { return TypeHelper.Types.GetData(type); } }
        public object Construct()
        {
            object output = TypeData.Instantiate(args.Select(prim => prim.Object).ToArray());
            foreach (var field in fields)
            {
                TypeData.Set(output, field.Key, field.Value.Object);
            }
            return output;
        }
        public virtual InitData Set(string name, object value)
        {
            if (value is Enum)
                value = (int)value;
            fields[name] = new Primitive(value);
            return this;
        }
        public InitData Clone()
        {
            InitData output = new InitData();
            output.type = type.ToString();
            output.args = args.Select(prim => new Primitive(prim.Object)).ToArray();
            output.fields = fields.ToDictionary(kvp => kvp.Key, kvp => new Primitive(kvp.Value.Object));
            return output;
        }
        public ImmultableInitData ToImmutable()
        {
            ImmultableInitData output = new ImmultableInitData();
            output.args = args;
            output.type = type;
            output.fields = new Dictionary<string, Primitive>(fields);
            return output;
        }
    }
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ImmultableInitData : InitData
    {
        internal ImmultableInitData() { }
        public ImmultableInitData(string type, params object[] args) : base(type, args) { }
        public override InitData Set(string name, object value)
        {
            InitData output = new InitData();
            output.args = args;
            output.type = type;
            output.fields = new Dictionary<string, Primitive>(fields);
            return output.Set(name, value);
        }
    }
}