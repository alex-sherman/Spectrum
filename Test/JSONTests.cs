using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Physics.Collision.Shapes;

namespace SpectrumTest
{
    [TestClass]
    public class JSONTests
    {
        [TestMethod]
        public void TestReadVector3()
        {
            var vector = JConvert.Read<Vector3>("[0, 1, 2]");
            Assert.AreEqual(new Vector3(0, 1, 2), vector);
        }
        [TestMethod]
        public void TestReadNullableVector3()
        {
            var vector = JConvert.Read<Vector3?>("[0, 1, 2]");
            Assert.AreEqual(new Vector3(0, 1, 2), vector);
        }
        [TestMethod]
        public void TestReadNullableVector3Null()
        {
            var vector = JConvert.Read<Vector3?>("null");
            Assert.AreEqual(null, vector);
        }
        [TestMethod]
        public void TestWriteVector3()
        {
            var vectorString = JConvert.Write(new Vector3(0, 3, 2));
            Assert.AreEqual("[0.0,3.0,2.0]", vectorString);
        }
        [TestMethod]
        public void TestReadMatrix()
        {
            var matrix = JConvert.Read<Matrix>("{'rotation': [0, 90, 0]}");
            Assert.AreEqual(Matrix.CreateFromYawPitchRoll((float)Math.PI / 2, 0, 0), matrix);
        }
        [TestMethod]
        public void TestReadWriteMatrix()
        {
            var matrix = JConvert.Read<Matrix>(JConvert.Write(Matrix.CreateFromYawPitchRoll((float)Math.PI / 2, 0, 0)));
            Assert.AreEqual(Matrix.CreateFromYawPitchRoll((float)Math.PI / 2, 0, 0), matrix);
        }
        [TestMethod]
        public void TestReadBoxShape()
        {
            var shape = JConvert.Read<Shape>("{'type': 'box', 'size': [1, 2, 3]}");
            Assert.IsInstanceOfType(shape, typeof(BoxShape));
            var boxShape = shape as BoxShape;
            Assert.AreEqual(new Vector3(1, 2, 3), boxShape.Size);
        }
        [TestMethod]
        public void TestReadListShape()
        {
            var shape = JConvert.Read<Shape>("{'type': 'list', 'shapes': [{'type': 'box', 'size': [1, 2, 3]}]}");
            Assert.IsInstanceOfType(shape, typeof(ListMultishape));
            var listShape = shape as ListMultishape;
            Assert.IsInstanceOfType(listShape.Shapes[0], typeof(BoxShape));
            var boxShape = listShape.Shapes[0] as BoxShape;
            Assert.AreEqual(new Vector3(1, 2, 3), boxShape.Size);
        }
    }
}
