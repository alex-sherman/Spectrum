using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Network.Surrogates
{
    public class Primitive
    {
        public object Object;
        public Primitive(object obj = null) { Object = obj; }
    }
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    class PrimitiveSurrogate
    {
        private static Dictionary<int, Type> TypeMap = new Dictionary<int, Type>();
        private static int LastTypeID = 0;
        public int type;
        public byte[] buffer;

        private object Object
        {
            get
            {
                MemoryStream stream = new MemoryStream(buffer);
                return Serializer.NonGeneric.Deserialize(TypeMap[type], stream);
            }
        }
        public static implicit operator PrimitiveSurrogate(Primitive obj)
        {
            if (obj == null) return null;
            int type = GetID(obj.Object.GetType());
            byte[] buffer = GetBytes(obj.Object);
            return new PrimitiveSurrogate() { type = type, buffer = buffer };
        }
        public static implicit operator Primitive(PrimitiveSurrogate obj)
        {
            if (obj == null) return null;
            return new Primitive(obj.Object);
        }
        public static int GetID(Type type)
        {
            return TypeMap.Where(typeMap => type == typeMap.Value || type.IsSubclassOf(typeMap.Value))
                .Select(typeMap => typeMap.Key).First();
        }
        public static byte[] GetBytes(object obj)
        {
            MemoryStream stream = new MemoryStream();
            Serializer.NonGeneric.Serialize(stream, obj);
            return stream.ToArray();
        }
        public static void RegisterType(Type type)
        {
            TypeMap[LastTypeID] = type;
            LastTypeID++;
        }
    }
}
