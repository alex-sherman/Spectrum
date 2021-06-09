using Newtonsoft.Json.Linq;
using ProtoBuf;
using Replicate;
using Replicate.MetaData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Network.Surrogates
{
    public class Primitive
    {
        public object Object;
        public Primitive() { }
        public Primitive(object obj)
        {
            if (obj is Enum)
                obj = (int)obj;
            Object = obj;
        }
    }
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [ReplicateType]
    public class PrimitiveSurrogate
    {
        public TypeId type;
        public byte[] buffer;

        private object Object
        {
            get
            {
                if (type.Id.IsEmpty) return null;
                MemoryStream stream = new MemoryStream(buffer);
                Type objType = TypeHelper.Model.GetType(type);
                if (objType == typeof(JToken))
                    return (JToken)(JSONSurrogate)Serializer.NonGeneric.Deserialize(typeof(JSONSurrogate), stream);
                return Serialization.BinarySerializer.Deserialize(objType, stream);
            }
        }
        public static implicit operator PrimitiveSurrogate(Primitive obj)
        {
            if (obj == null) return null;
            if (obj.Object != null)
            {
                TypeId type = TypeHelper.Model.GetId(obj.Object.GetType());
                MemoryStream buffer = new MemoryStream();
                Serialize(buffer, obj.Object);
                return new PrimitiveSurrogate() { type = type, buffer = buffer.ToArray() };
            }
            return new PrimitiveSurrogate();
        }
        public static implicit operator Primitive(PrimitiveSurrogate obj)
        {
            if (obj == null) return null;
            return new Primitive(obj.Object);
        }
        public static MemoryStream Serialize(MemoryStream stream, object obj)
        {
            if (obj is JToken)
                obj = (JSONSurrogate)(JToken)obj;
            Serialization.BinarySerializer.Serialize(obj.GetType(), obj, stream);
            return stream;
        }
    }
}
