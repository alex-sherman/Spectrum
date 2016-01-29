using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spectrum.Framework.Physics.Collision.Shapes;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Physics.Collision;
using Spectrum.Framework.Entities;
using Spectrum;

namespace SpectrumTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //new SpectrumGame(null);
            GameObject go1 = new GameObject();
            go1.Shape = new TerrainShape(new float[2, 2] { { -1, 1 }, { -1, 1 } }, 2);
            GameObject go2 = new GameObject();
            go2.Shape = new BoxShape(Vector3.One);
            go2.Position = new Vector3(0.5f, 0.4f, 0.5f);
            CollisionSystemPersistentSAP system = new CollisionSystemPersistentSAP();
            Vector3 point, normal;
            float penetration;
            system.GetContact(go1, go2, out point, out normal, out penetration);
            Assert.IsTrue(Vector3.Dot(normal, new Vector3(0, -0.7f, 0.7f)) > 0.8f);
        }
    }
}
