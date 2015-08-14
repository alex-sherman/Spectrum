﻿using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Spectrum.Framework.Content
{
    public class Plugin
    {
        public const string BasePath = "Plugins";
        public ContentHelper Content { get; private set; }
        private string path;
        public Assembly assembly { get; protected set; }
        public Plugin()
        {
            //Monogame's content manager doesn't work with absolute paths
            //try
            //{
            //    files = Directory.GetFiles(Path.Combine(this.path, "Locations"), "*.loc", SearchOption.TopDirectoryOnly);
            //    foreach (String filename in files)
            //    {
            //        using (FileStream file = new FileStream(Path.Combine(this.path, "Locations", filename), FileMode.Open))
            //        {
            //            try
            //            {
            //                //LocationPlacer.locations.Add(Location.FromFile(file, SpectrumGame.Game.MP));
            //            }
            //            catch (SerializationException)
            //            {
            //                continue;
            //            }
            //        }
            //    }
            //}
            //catch (DirectoryNotFoundException) { }
            //catch (FileNotFoundException) { }
        }

        public virtual void OnLoad()
        {

        }

        public static List<string> GetPluginNames()
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
            string[] possiblePaths = Directory.GetDirectories(BasePath);
            List<string> pluginNames = new List<string>();


            foreach (string pluginPath in possiblePaths)
            {
                string pluginName = Path.GetFileName(pluginPath);
                if (Directory.GetFiles(pluginPath, pluginName + ".dll", SearchOption.TopDirectoryOnly).Length > 0)
                    pluginNames.Add(pluginName);
            }
            return pluginNames;
        }

        public static Plugin CreatePlugin(string plugin)
        {
            string pluginPath = Path.Combine(BasePath, plugin);
            string[] dlls = Directory.GetFiles(pluginPath, plugin + ".dll", SearchOption.TopDirectoryOnly);
            if (dlls.Length < 1)
                throw new FileNotFoundException("Invalid plugin, does not contain a DLL of the same name as the plugin directory");
            Assembly assembly = Assembly.LoadFile(System.IO.Path.GetFullPath(dlls[0]));

            Plugin output = null;
            Type pluginType = assembly.GetType(plugin + "." + plugin);
            if (pluginType != null)
            {
                if (!pluginType.IsSubclassOf(typeof(Plugin)))
                    throw new TypeLoadException("Plugin types must be a subclass of Plugin!");
                output = (Plugin)pluginType.GetConstructor(new Type[] { }).Invoke(new object[] { });
            }
            else
                output = new Plugin();
            output.assembly = assembly;
            output.Content = new ContentHelper(new ContentManager(SpectrumGame.Game.Services, Path.Combine(pluginPath, "Content")));
            return output;
        }

        public virtual List<Type> GetTypes()
        {
            return assembly.GetTypes().ToList();
        }
    }
}
