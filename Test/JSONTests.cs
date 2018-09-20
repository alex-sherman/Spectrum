using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework;
using Spectrum.Framework.Content;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Physics.Collision.Shapes;

namespace SpectrumTest
{
    [TestClass]
    public class JSONTests
    {
        [TestInitialize()]
        public void Initialize()
        {
            var plugin = Plugin.CreatePlugin("Main", null, LoadHelper.SpectrumAssembly);
            LoadHelper.RegisterTypes(plugin);
        }
        [TestMethod]
        public void TestReadVector3()
        {
            var vector = JConvert.Deserialize<Vector3>("[0, 1, 2]");
            Assert.AreEqual(new Vector3(0, 1, 2), vector);
        }
        [TestMethod]
        public void TestReadNullableVector3()
        {
            var vector = JConvert.Deserialize<Vector3?>("[0, 1, 2]");
            Assert.AreEqual(new Vector3(0, 1, 2), vector);
        }
        [TestMethod]
        public void TestReadNullableVector3Null()
        {
            var vector = JConvert.Deserialize<Vector3?>("null");
            Assert.AreEqual(null, vector);
        }
        [TestMethod]
        public void TestWriteVector3()
        {
            var vectorString = JConvert.Serialize(new Vector3(0, 3, 2));
            Assert.AreEqual("[0.0,3.0,2.0]", vectorString);
        }
        [TestMethod]
        public void TestReadMatrix()
        {
            var matrix = JConvert.Deserialize<Matrix>("{'rotation': [0, 90, 0]}");
            Assert.AreEqual(Matrix.CreateFromYawPitchRoll((float)Math.PI / 2, 0, 0), matrix);
        }
        [TestMethod]
        public void TestReadWriteMatrix()
        {
            var matrix = JConvert.Deserialize<Matrix>(JConvert.Serialize(Matrix.CreateFromYawPitchRoll((float)Math.PI / 2, 0, 0)));
            Assert.AreEqual(Matrix.CreateFromYawPitchRoll((float)Math.PI / 2, 0, 0), matrix);
        }
        [TestMethod]
        public void TestReadBoxShape()
        {
            var shape = JConvert.Deserialize<Shape>("{'type': 'box', 'size': [1, 2, 3]}");
            Assert.IsInstanceOfType(shape, typeof(BoxShape));
            var boxShape = shape as BoxShape;
            Assert.AreEqual(new Vector3(1, 2, 3), boxShape.Size);
        }
        [TestMethod]
        public void TestWriteBoxShape()
        {
            var boxString = JConvert.Serialize(new BoxShape(Vector3.One, Vector3.One / 2));
            Assert.AreEqual("{\r\n  \"type\": \"box\",\r\n  \"size\":[1.0,1.0,1.0],\r\n  \"position\":[0.5,0.5,0.5]\r\n}", boxString);
        }
        [TestMethod]
        public void TestReadListShape()
        {
            var shape = JConvert.Deserialize<Shape>("{'type': 'list', 'shapes': [{'type': 'box', 'size': [1, 2, 3]}]}");
            Assert.IsInstanceOfType(shape, typeof(ListMultishape));
            var listShape = shape as ListMultishape;
            Assert.IsInstanceOfType(listShape.Shapes[0], typeof(BoxShape));
            var boxShape = listShape.Shapes[0] as BoxShape;
            Assert.AreEqual(new Vector3(1, 2, 3), boxShape.Size);
        }
        [TestMethod]
        public void TestWriteListShape()
        {
            var shape = JConvert.Deserialize<Shape>("{'type': 'list', 'shapes': [{'type': 'box', 'size': [1, 2, 3]}]}");
            Assert.IsInstanceOfType(shape, typeof(ListMultishape));
            var shapeString = JConvert.Serialize(shape);
            Assert.AreEqual("{\r\n  \"type\": \"list\",\r\n  \"shapes\": [\r\n    {\r\n      \"type\": \"box\",\r\n      \"size\":[1.0,2.0,3.0],\r\n      \"position\":[0.0,0.0,0.0]\r\n    }\r\n  ]\r\n}", shapeString);
        }
        [TestMethod]
        public void TestWriteInitData()
        {
            var initString = JConvert.Serialize(new InitData<GameObject>("faff").Set("herp", Matrix.CreateTranslation(Vector3.One)));
            Assert.AreEqual("{\r\n  \"@Name\": null,\r\n  \"@TypeName\": \"GameObject\",\r\n  \"herp\":[1.0,0.0,0.0,0.0,0.0,1.0,0.0,0.0,0.0,0.0,1.0,0.0,1.0,1.0,1.0,1.0]\r\n}", initString);
        }
    }
}
