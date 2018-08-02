using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
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
        // TODO: These numbers aren't useful at the moment
        [TestMethod]
        public void RayCastBugFix1()
        {
            var cs = new CollisionSystemPersistentSAP();
            GameObject test = new GameObject();
            var p = new Vector3(-4.915751f, 4.986178f, -4.940529f);
            var d = new Vector3(0.8097232f,- 0.1328402f, 0.5715783f);
            test.Shape = new BoxShape(new Vector3(160.0045f, 0.526213f, 160.0041f), new Vector3(80.00226f, 0.2631065f, 80.00204f));
            test.PhysicsUpdate(0);
            var result = cs.Raycast(test, p, d, out Vector3 normal, out float fraction);
            Assert.IsTrue(result);
        }
    }
}
