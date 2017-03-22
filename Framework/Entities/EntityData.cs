using Microsoft.Xna.Framework;
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
    public class EntityData
    {
        public string type;
        public Primitive[] args = new Primitive[0];
        public Dictionary<string, Primitive> fields = new Dictionary<string, Primitive>();
        internal EntityData() { }
        public EntityData(string type, params object[] args)
        {
            this.type = type;
            this.args = args.Select(obj => new Primitive(obj)).ToArray();
        }
        public virtual EntityData Set(string name, object value)
        {
            fields[name] = new Primitive(value);
            return this;
        }
        public EntityData Clone()
        {
            return Serialization.Copy(this);
        }
        public ImmutableEntityData ToImmutable()
        {
            ImmutableEntityData output = new ImmutableEntityData();
            output.args = args;
            output.type = type;
            output.fields = new Dictionary<string, Primitive>(fields);
            return output;
        }
    }
    public class ImmutableEntityData : EntityData
    {
        internal ImmutableEntityData() { }
        public ImmutableEntityData(string type, params object[] args) : base(type, args) { }
        public override EntityData Set(string name, object value)
        {
            EntityData output = new EntityData();
            output.args = args;
            output.type = type;
            output.fields = new Dictionary<string, Primitive>(fields);
            return output.Set(name, value);
        }
    }
}
