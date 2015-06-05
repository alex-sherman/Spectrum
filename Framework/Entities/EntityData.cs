using Microsoft.Xna.Framework;
using Spectrum.Framework.Network;
using Spectrum.Framework.Physics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Entities
{
    public class EntityData : ISerializable
    {
        public string type;
        public Guid guid;
        public NetID owner;
        public object[] args;
        public Vector3 position;
        private EntityData() { }
        public EntityData(NetMessage stream)
        {
            byte[] guidBuffer = new byte[16];
            guid = stream.ReadGuid();
            owner = stream.ReadNetID();
            position = stream.ReadVector();
            type = stream.ReadString();
            args = stream.ReadPrimitiveArray();
        }
        public EntityData(string type, Guid guid, NetID owner, Vector3 position, object[] args)
        {
            this.type = type;
            this.guid = guid;
            this.owner = owner;
            this.position = position;
            this.args = args;
        }
        public EntityData(Entity e)
        {
            this.type = e.GetType().Name;
            this.guid = e.ID;
            this.owner = e.OwnerGuid;
            if (e is GameObject)
            {
                this.position = (e as GameObject).Position;
            }
            this.args = e.creationArgs;
        }
        public void WriteTo(NetMessage output)
        {
            //Serializer.ConvertToStream(args).WriteTo(output);
            output.Write(guid);
            output.Write(owner);
            output.Write(position);
            output.Write(type);
            output.WritePrimitiveArray(args);
        }

        public ISerializable Copy()
        {
            NetMessage temp = new NetMessage();
            this.WriteTo(temp);
            return new EntityData(temp);
        }
    }
}
