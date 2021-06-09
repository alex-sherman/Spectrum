using System;
using Spectrum.Framework.Physics.Collision.Shapes;
using Spectrum.Framework.Physics.Collision;
using Spectrum.Framework.Entities;
using Spectrum;
using Spectrum.Framework.Network;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Network.Surrogates;
using NUnit.Framework;
using Replicate.Serialization;
using Spectrum.Framework;

namespace SpectrumTest
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void TestJSONPrimitiveCopy()
        {
            Serialization.InitSurrogates();
            var output = Serialization.Copy(new Primitive(JToken.Parse("{\"herp\": 3}")));
        }
        [Test]
        public void TestInitDataTCopyable()
        {
            Serialization.InitSurrogates();
            TypeHelper.Model.LoadTypes(typeof(GameObject).Assembly);
            var initData = new InitData<GameObject>(() => new GameObject() { Position = new Vector3(1, 2, 3) });
            var stream = Serialization.BinarySerializer.Serialize(initData);
            stream.Position = 0;
            var result = Serialization.BinarySerializer.Deserialize<InitData<GameObject>>(stream);
        }
        [Test]
        public void TestInitDataCopyable()
        {
            Serialization.InitSurrogates();
            TypeHelper.Model.LoadTypes(typeof(GameObject).Assembly);
            var initData = new InitData<GameObject>(() => new GameObject() { Position = new Vector3(1, 2, 3) });
            var stream = Serialization.BinarySerializer.Serialize((InitData)initData);
            stream.Position = 0;
            var result = Serialization.BinarySerializer.Deserialize<InitData>(stream);
        }
    }
}
