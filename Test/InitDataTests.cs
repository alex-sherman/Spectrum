using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spectrum.Framework;
using Spectrum.Framework.Content;
using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumTest
{
    [LoadableType]
    public class ClassWithArgument
    {
        public string Field;
        public int Property { get; set; }
        public float Derp { get; private set; }
        public ClassWithArgument(float derp)
        {
            Derp = derp;
        }
    }
    [TestClass]
    public class InitDataTests
    {
        [TestInitialize()]
        public void Initialize()
        {
            var plugin = Plugin.CreatePlugin("Main", null, LoadHelper.SpectrumAssembly);
            LoadHelper.RegisterTypes(plugin);
        }
        [TestMethod]
        public void InitDataUpdateOnSet()
        {
            var idata = new InitData<Entity>();
            Assert.AreEqual(idata, idata.Set("Position", null));
        }
        [TestMethod]
        public void InitDataCopyOnSet()
        {
            InitData idata = new InitData<Entity>().ToImmutable();
            Assert.AreNotEqual(idata, idata.Set("Position", null));
        }
        [TestMethod]
        public void SetEntityDataValid()
        {
            Entity entity = new InitData<Entity>().SetDict("Test", "herp").Construct();
            Assert.IsNotNull(entity);
            Assert.AreEqual(entity.Data["Test"], "herp");
        }
        [TestMethod]
        public void FunctionalInspection()
        {
            TypeHelper.RegisterType(typeof(ClassWithArgument), null);
            var obj = new InitData<ClassWithArgument>(() => new ClassWithArgument(1.5f) { Field = "Derp", Property = 3 }).Construct();
            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.Derp, 1.5f);
            Assert.AreEqual(obj.Field, "Derp");
            Assert.AreEqual(obj.Property, 3);
        }
    }
}
