using NUnit.Framework;
using Spectrum.Framework;
using Spectrum.Framework.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumTest
{
    [TestFixture]
    public class ElementTest
    {
        [Test]
        public void DefaultHorizontalLayout()
        {
            var root = new Element();
            var ele1 = root.AddElement(new Element() { Width = 10, Height = 20 });
            var ele2 = root.AddElement(new Element() { Width = 20, Height = 10 });
            root.ClearMeasure();
            root.Measure(100, 100);
            root.Layout(new Rectangle(0, 0, 100, 100));
            Assert.AreEqual(ele1.Rect.Left, 0);
            Assert.AreEqual(ele1.Rect.Right, 10);
            Assert.AreEqual(ele1.Rect.Top, 0);
            Assert.AreEqual(ele1.Rect.Bottom, 20);
            Assert.AreEqual(ele2.Rect.Left, 10);
            Assert.AreEqual(ele2.Rect.Right, 30);
            Assert.AreEqual(ele2.Rect.Top, 0);
            Assert.AreEqual(ele2.Rect.Bottom, 10);
        }
        [Test]
        public void CenterHorizontally()
        {
            var root = new Element { Width = 100, Height = 100 };
            var ele1 = root.AddElement(new Element() { Margin = new RectOffset() { Left = new ElementSize(-5, 0.5) }, Width = 10, Height = 20 });
            root.ClearMeasure();
            root.Measure(100, 100);
            root.Layout(new Rectangle(0, 0, 100, 100));
            Assert.AreEqual(ele1.Rect.Left, 45);
            Assert.AreEqual(ele1.Rect.Right, 55);
            Assert.AreEqual(ele1.Rect.Top, 0);
            Assert.AreEqual(ele1.Rect.Bottom, 20);
        }
        [Test]
        public void GridLayoutRelativeChildren()
        {
            var root = new GridLayout(4) { Width = 100, Height = 100 };
            var ele0 = root.AddElement(new Element() { Width = 1.0, Height = 1.0 });
            var ele1 = root.AddElement(new Element() { Width = 1.0, Height = 1.0 });
            var ele2 = root.AddElement(new Element() { Width = 1.0, Height = 1.0 });
            var ele3 = root.AddElement(new Element() { Width = 1.0, Height = 1.0 });
            var ele4 = root.AddElement(new Element() { Width = 1.0, Height = 1.0 });
            root.ClearMeasure();
            root.Measure(100, 100);
            root.Layout(new Rectangle(0, 0, 100, 100));
            Assert.AreEqual(ele0.Rect.Left, 0);
            Assert.AreEqual(ele0.Rect.Right, 25);
            Assert.AreEqual(ele0.Rect.Top, 0);
            Assert.AreEqual(ele0.Rect.Bottom, 25);
            Assert.AreEqual(ele1.Rect.Left, 25);
            Assert.AreEqual(ele1.Rect.Right, 50);
            Assert.AreEqual(ele1.Rect.Top, 0);
            Assert.AreEqual(ele1.Rect.Bottom, 25);
            Assert.AreEqual(ele4.Rect.Left, 0);
            Assert.AreEqual(ele4.Rect.Right, 25);
            Assert.AreEqual(ele4.Rect.Top, 25);
            Assert.AreEqual(ele4.Rect.Bottom, 50);
        }
    }
}
