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
        public static readonly BinarySerializer BinarySerializer = new BinarySerializer(TypeHelper.Model);
        private static bool Initialized = false;
        public static void InitSurrogates()
        {
            if (Initialized)
                return;
            Initialized = true;
            TypeHelper.Model.Add(typeof(Vector2)).AddMember("X").AddMember("Y");
            TypeHelper.Model.Add(typeof(Vector3)).AddMember("X").AddMember("Y").AddMember("Z");
            TypeHelper.Model.Add(typeof(Rectangle)).AddMember("X").AddMember("Y").AddMember("Width").AddMember("Height");
            TypeHelper.Model.Add(typeof(MemoryStream)).SetSurrogate(new Surrogate(typeof(StreamSurrogate)));
            TypeHelper.Model.Add(typeof(Primitive)).SetSurrogate(typeof(PrimitiveSurrogate));
            TypeHelper.Model.Add(typeof(SpecModel)).SetSurrogate(typeof(ModelSurrogate));
            TypeHelper.Model.Add(typeof(Shape));
        }
        public static T Copy<T>(T obj) where T : class, new()
        {
            if (obj == null) return null;
            var accessor = TypeHelper.Model.GetTypeAccessor(obj.GetType());
            return TypeUtil.CopyToRaw(obj, accessor, null, accessor) as T;
        }
    }
}
