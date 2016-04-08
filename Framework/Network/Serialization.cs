using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using ProtoBuf.Meta;
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
        public static RuntimeTypeModel Model = RuntimeTypeModel.Default;
        public static void InitSurrogates()
        {
            Model.AutoCompile = false;
            RegisterType(typeof(bool));
            RegisterType(typeof(int));
            RegisterType(typeof(float));
            //RegisterType(typeof(float[,]));
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
            if (Model.IsDefined(typeof(Primitive)))
                return;
            Model.Add(typeof(Vector3), true);
            Model[typeof(Vector3)].Add("X").Add("Y").Add("Z");
            MetaType matrix = Model.Add(typeof(Matrix), true);
            for (int r = 1; r <= 4; r++)
            {
                for (int c = 1; c <= 4; c++)
                {
                    matrix.Add(string.Format("M{0}{1}", r, c));
                }
            }
            Model.Add(typeof(MemoryStream), false).SetSurrogate(typeof(StreamSurrogate));
            Model.Add(typeof(Primitive), false).SetSurrogate(typeof(PrimitiveSurrogate));
            //Model.Add(typeof(float[,]), false).SetSurrogate(typeof(FloatArraySurrogate));
        }
        public static void RegisterType(Type type)
        {
            if (type.IsSubclassOf(typeof(Entity)))
            {
                int subTypeCount = Model[typeof(Entity)].GetSubtypes().Count();
                Model[typeof(Entity)].AddSubType(subTypeCount + 1, type);
                Type surrogateType = typeof(EntitySurrogate<>);
                surrogateType = surrogateType.MakeGenericType(type);
                Model.Add(type, false).SetSurrogate(surrogateType);
            }
            PrimitiveSurrogate.RegisterType(type);
        }
        public static T Copy<T>(T obj) where T : class
        {
            if (obj == null) return null;
            MemoryStream stream = new MemoryStream();
            Serializer.NonGeneric.Serialize(stream, obj);
            stream.Position = 0;
            return Serializer.Deserialize<T>(stream);
        }
    }
}
