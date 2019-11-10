using System;
using Spectrum.Framework.Physics.Collision.Shapes;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Physics.Collision;
using Spectrum.Framework.Entities;
using Spectrum;
using Spectrum.Framework.Network;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Network.Surrogates;
using NUnit.Framework;

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
    }
}
