using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Spectrum.Framework.Content;
using Spectrum.Framework.Graphics;

namespace Spectrum.Framework
{
    public class LoadHelper
    {
        public static TypeHelper LoadTypes(string LocalDir = null)
        {
            string path = "Plugins";
            if (LocalDir != null)
            {
                path = System.IO.Path.Combine(LocalDir, "Plugins");
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string[] plugins = Directory.GetDirectories(path);
            foreach (string pluginPath in plugins)
            {
                string pluginName = Path.GetFileName(pluginPath);
                SpectrumGame.Game.Plugins[pluginName] = new Plugin(pluginName, pluginPath);
            }
            //TypeHelper.Helper["StatModifier"] = typeof(StatModifier);
            //TypeHelper.Helper["Player"] = typeof(Player);
            TypeHelper.Helper["Water"] = typeof(Water);
            return TypeHelper.Helper;
        }
    }
}
