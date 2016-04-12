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
        private EntityData() { }
        public EntityData(string type, params object[] args)
        {
            this.type = type;
            this.args = args.Select(obj => new Primitive(obj)).ToArray();
        }
        public EntityData Set(string name, object value)
        {
            fields[name] = new Primitive(value);
            return this;
        }
        public EntityData Set(Dictionary<string, object> values)
        {
            foreach (var kvp in values)
            {
                Set(kvp.Key, kvp.Value);
            }
            return this;
        }
        public EntityData Clone()
        {
            return Serialization.Copy(this);
        }
    }
}
