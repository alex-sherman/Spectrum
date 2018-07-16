using Newtonsoft.Json.Linq;
using ProtoBuf;
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
        public Primitive(object obj = null)
        {
            if (obj is Enum)
                obj = (int)obj;
            Object = obj;
        }
    }
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    class PrimitiveSurrogate
    {
        private static Dictionary<int, Type> TypeMap = new Dictionary<int, Type>();
        private static int LastTypeID = 1;
        public int type;
        public byte[] buffer;

        static BinaryFormatter binarySerializer = new BinaryFormatter();

        private object Object
        {
            get
            {
                if (type == 0) return null;
                MemoryStream stream = new MemoryStream(buffer);
                var objType = TypeMap[type];
                if (objType == typeof(JToken))
                    return (JToken)(JSONSurrogate)Serializer.NonGeneric.Deserialize(typeof(JSONSurrogate), stream);
                return Serializer.NonGeneric.Deserialize(TypeMap[type], stream);
            }
        }
        public static implicit operator PrimitiveSurrogate(Primitive obj)
        {
            if (obj == null) return null;
            if (obj.Object != null)
            {
                int type = GetID(obj.Object.GetType());
                MemoryStream buffer = new MemoryStream();
                Serialize(buffer, obj.Object);
                return new PrimitiveSurrogate() { type = type, buffer = buffer.ToArray() };
            }
            return new PrimitiveSurrogate() { type = 0 };
        }
        public static implicit operator Primitive(PrimitiveSurrogate obj)
        {
            if (obj == null) return null;
            return new Primitive(obj.Object);
        }
        public static int GetID(Type type)
        {
            return TypeMap.Where(typeMap => type == typeMap.Value || type.IsSubclassOf(typeMap.Value))
                .Select(typeMap => (int?)typeMap.Key).FirstOrDefault() ?? -1;
        }
        public static MemoryStream Serialize(MemoryStream stream, object obj)
        {
            if (obj is JToken)
                obj = (JSONSurrogate)(JToken)obj;
            Serializer.NonGeneric.Serialize(stream, obj);
            return stream;
        }
        public static void RegisterType(Type type)
        {
            TypeMap[LastTypeID] = type;
            LastTypeID++;
        }
    }
}
