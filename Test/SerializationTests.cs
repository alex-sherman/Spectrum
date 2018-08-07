using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spectrum.Framework.Physics.Collision.Shapes;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Physics.Collision;
using Spectrum.Framework.Entities;
using Spectrum;
using Spectrum.Framework.Network;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Network.Surrogates;

namespace SpectrumTest
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void TestJSONPrimitiveCopy()
        {
            Serialization.InitSurrogates();
            var output = Serialization.Copy(new Primitive(JToken.Parse("{\"herp\": 3}")));
        }
    }
}
