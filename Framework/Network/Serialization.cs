using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Network.Surrogates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Network
{
    public class Serialization
    {
        public static SurrogateSelector SurrogateSelector = new SurrogateSelector();
        public static void InitSurrogates()
        {
            RegisterType(typeof(bool));
            RegisterType(typeof(int));
            RegisterType(typeof(float));
            RegisterType(typeof(float[,]));
            RegisterType(typeof(string));
            RegisterType(typeof(Vector3));
            RegisterType(typeof(Entity));
            RegisterType(typeof(Point));
            RegisterType(typeof(byte));
            RegisterType(typeof(Guid));
            RegisterType(typeof(Matrix));
            RegisterType(typeof(JToken));
            RegisterType(typeof(NetID));
            RegisterType(typeof(EntityData));
            //RegisterType(typeof(List<>));
            //RegisterType(typeof(Dictionary<>));
            if (ProtoBuf.Meta.RuntimeTypeModel.Default.IsDefined(typeof(Primitive)))
                return;
            ProtoBuf.Meta.RuntimeTypeModel.Default.Add(typeof(Vector3), true);
            ProtoBuf.Meta.RuntimeTypeModel.Default[typeof(Vector3)].Add("X").Add("Y").Add("Z");
            ProtoBuf.Meta.RuntimeTypeModel.Default.Add(typeof(MemoryStream), false).SetSurrogate(typeof(StreamSurrogate));
            ProtoBuf.Meta.RuntimeTypeModel.Default.Add(typeof(Primitive), false).SetSurrogate(typeof(PrimitiveSurrogate));
            ProtoBuf.Meta.RuntimeTypeModel.Default.Add(typeof(float[,]), false).SetSurrogate(typeof(FloatArraySurrogate));
        }
        public static void RegisterType(Type type)
        {
            PrimitiveSurrogate.RegisterType(type);
        }
        public static object Copy(object obj)
        {
            if (obj == null) return null;
            Type type = obj.GetType();
            MemoryStream stream = new MemoryStream();
            Serializer.NonGeneric.Serialize(stream, obj);
            stream.Position = 0;
            return Serializer.NonGeneric.Deserialize(type, stream);
        }
    }
}
