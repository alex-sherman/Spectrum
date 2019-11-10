using Microsoft.Xna.Framework;
using NUnit.Framework;
using Spectrum.Framework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumTest
{
    [TestFixture]
    public class MathExtensionTests
    {
        [Test]
        public void RetangleFitCropX()
        {
            var fitted = new Rectangle(0, 0, 50, 50).FitTo(new Rectangle(0, 0, 200, 100), true, false);
            Assert.AreEqual(new Rectangle(0, 0, 200, 200), fitted);
        }
        [Test]
        public void RetangleFitCropY()
        {
            var fitted = new Rectangle(0, 0, 50, 50).FitTo(new Rectangle(0, 0, 100, 200), true, false);
            Assert.AreEqual(new Rectangle(0, 0, 200, 200), fitted);
        }
        [Test]
        public void RetangleFitNoCropX()
        {
            var fitted = new Rectangle(0, 0, 50, 50).FitTo(new Rectangle(0, 0, 100, 200), false, false);
            Assert.AreEqual(new Rectangle(0, 0, 100, 100), fitted);
        }
        [Test]
        public void RetangleFitNoCropY()
        {
            var fitted = new Rectangle(0, 0, 50, 50).FitTo(new Rectangle(0, 0, 200, 100), false, false);
            Assert.AreEqual(new Rectangle(0, 0, 100, 100), fitted);
        }

        //Centered
        [Test]
        public void RetangleFitCropXCentered()
        {
            var fitted = new Rectangle(0, 0, 50, 50).FitTo(new Rectangle(0, 0, 200, 100), true);
            Assert.AreEqual(new Rectangle(0, -50, 200, 200), fitted);
        }
        [Test]
        public void RetangleFitCropYCentered()
        {
            var fitted = new Rectangle(0, 0, 50, 50).FitTo(new Rectangle(0, 0, 100, 200), true);
            Assert.AreEqual(new Rectangle(-50, 0, 200, 200), fitted);
        }
        [Test]
        public void RetangleFitNoCropXCentered()
        {
            var fitted = new Rectangle(0, 0, 50, 50).FitTo(new Rectangle(0, 0, 100, 200), false);
            Assert.AreEqual(new Rectangle(0, 50, 100, 100), fitted);
        }
        [Test]
        public void RetangleFitNoCropYCentered()
        {
            var fitted = new Rectangle(0, 0, 50, 50).FitTo(new Rectangle(0, 0, 200, 100), false);
            Assert.AreEqual(new Rectangle(50, 0, 100, 100), fitted);
        }
    }
}
