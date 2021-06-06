using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using ProtoBuf.Meta;
using Replicate;
using Replicate.MetaData;
using Replicate.Serialization;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Network.Surrogates;
using Spectrum.Framework.Physics.Collision.Shapes;
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
        public static readonly BinaryGraphSerializer BinarySerializer = new BinaryGraphSerializer(TypeHelper.Model);
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
            RegisterType(typeof(GameObject));
            RegisterType(typeof(Point));
            RegisterType(typeof(byte));
            RegisterType(typeof(Guid));
            RegisterType(typeof(Matrix));
            RegisterType(typeof(JToken));
            RegisterType(typeof(NetID));
            RegisterType(typeof(InitData));
            RegisterType(typeof(BoxShape));
            //RegisterType(typeof(List<>));
            //RegisterType(typeof(Dictionary<>));
            if (Model.IsDefined(typeof(Primitive)))
                return;
            TypeHelper.Model.Add(typeof(Vector2)).AddMember("X").AddMember("Y");
            TypeHelper.Model.Add(typeof(Vector3)).AddMember("X").AddMember("Y").AddMember("Z");
            TypeHelper.Model.Add(typeof(Rectangle)).AddMember("X").AddMember("Y").AddMember("Width").AddMember("Height");
            TypeHelper.Model.Add(typeof(MemoryStream)).SetSurrogate(new Surrogate(typeof(StreamSurrogate)));
            Model.Add(typeof(Vector3), true);
            Model[typeof(Vector3)].Add("X").Add("Y").Add("Z");
            Model.Add(typeof(Point3), true);
            Model[typeof(Point3)].Add("X").Add("Y").Add("Z");
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
            Model.Add(typeof(SpecModel), false).SetSurrogate(typeof(ModelSurrogate));
            Model.Add(typeof(Shape), true);
            Model[typeof(Shape)].AddSubType(1, typeof(BoxShape));
            Model[typeof(Shape)].AddSubType(2, typeof(ListMultishape));
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
        public static T Copy<T>(T obj) where T : class, new()
        {
            if (obj == null) return null;
            var accessor = TypeHelper.Model.GetTypeAccessor(obj.GetType());
            return TypeUtil.CopyToRaw(obj, accessor, null, accessor) as T;
        }
    }
}
