using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Spectrum.Framework;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Physics.Collision;
using Spectrum.Framework.Physics.Collision.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumTest
{
    [TestClass]
    public class PhysicsTests
    {
        [TestMethod]
        public void RayCastBugFix1()
        {
            var cs = new CollisionSystemPersistentSAP();
            GameObject test = new GameObject();
            Ray ray = JConvert.Deserialize<Ray>("{\r\n  \"Direction\":[0.529075,-0.291127771,0.7970723],\r\n  \"Position\":[-1.4831599,12.3906965,-1.33272672]\r\n}");
            test.Shape = JConvert.Deserialize<Shape>("{\r\n  \"type\": \"box\",\r\n  \"size\": [\r\n    640.018066,\r\n    0.526213,\r\n    640.016357\r\n  ],\r\n  \"position\": [\r\n    320.009033,\r\n    0.2631065,\r\n    320.008179\r\n  ]\r\n}");
            test.PhysicsUpdate(0);
            var result = cs.Raycast(test, ray.Position, ray.Direction, out Vector3 normal, out float fraction);
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void RayCastBugFix2()
        {
            var cs = new CollisionSystemPersistentSAP();
            GameObject test = new GameObject();
            Ray ray = JConvert.Deserialize<Ray>("{\r\n  \"Direction\":[0.689626336,-0.1335963,0.7117356],\r\n  \"Position\":[46.7059631,62.460495,-20.704731]\r\n}");
            test.Shape = JConvert.Deserialize<Shape>("{\r\n  \"type\": \"box\",\r\n  \"size\": [\r\n    640.018066,\r\n    0.526213,\r\n    640.016357\r\n  ],\r\n  \"position\": [\r\n    320.009033,\r\n    0.2631065,\r\n    320.008179\r\n  ]\r\n}");
            test.PhysicsUpdate(0);
            var result = cs.Raycast(test, ray.Position, ray.Direction, out Vector3 normal, out float fraction);
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void TerrainCollision()
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
