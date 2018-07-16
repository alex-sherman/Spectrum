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
            Entity entity = (new InitData<Entity>().SetDict("Test", "herp")).Construct();
            Assert.IsNotNull(entity);
            Assert.AreEqual(entity.Data["Test"], "herp");
        }
    }
}
