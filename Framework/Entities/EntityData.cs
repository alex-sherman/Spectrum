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
        public EntityData(string type, Primitive[] args = null, Dictionary<string, Primitive> fields = null)
        {
            this.type = type;
            this.args = args ?? new Primitive[0];
            this.fields = fields ?? new Dictionary<string, Primitive>();
        }
        public EntityData(Entity e)
        {
            fields = new Dictionary<string, Primitive>();
            this.type = e.GetType().Name;
            this.fields["ID"] = new Primitive(e.ID);
            this.fields["OwnerGuid"] = new Primitive(e.OwnerGuid);
            if (e is GameObject)
            {
                this.fields["position"] = new Primitive((e as GameObject).Position);
            }
            this.args = e.creationArgs.Select(obj => new Primitive() { Object = obj }).ToArray();
        }
        public EntityData Set(string name, object value)
        {
            fields[name] = new Primitive(value);
            return this;
        }
    }
}
