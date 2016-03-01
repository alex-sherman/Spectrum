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
    [AttributeUsage(AttributeTargets.Class)]
    public class LoadableType : System.Attribute { }

    public class LoadHelper
    {
        public static void LoadTypes(Plugin plugin)
        {
            foreach (Type type in plugin.GetLoadableTypes())
            {
                TypeHelper.RegisterType(type, plugin);

                foreach (object attribute in type.GetCustomAttributes(true).ToList())
                {
                    PreloadedContentAttribute preload = attribute as PreloadedContentAttribute;
                    if (preload != null)
                    {
                        ContentHelper.LoadType(preload.Type, preload.Path);
                    }
                }
                foreach (FieldInfo field in type.GetFields())
                {
                    foreach (object attribute in field.GetCustomAttributes(true).ToList())
                    {
                        PreloadedContentAttribute preload = attribute as PreloadedContentAttribute;
                        if (preload != null)
                        {
                            ContentHelper.LoadType(field.FieldType, preload.Path);
                        }
                    }
                }
            }
        }
        public static void LoadTypes(string LocalDir = null)
        {
            SpectrumGame.Game.Plugins["Main"] = Plugin.CreatePlugin("Main", ContentHelper.Single, Assembly.GetEntryAssembly());

            foreach (var pluginName in Plugin.GetPluginNames())
            {
                SpectrumGame.Game.Plugins[pluginName] = Plugin.CreatePlugin(pluginName);
            }

            //Load all types before calling OnLoad
            foreach (var plugin in SpectrumGame.Game.Plugins.Values)
            {
                LoadHelper.LoadTypes(plugin);
            }

            foreach (var plugin in SpectrumGame.Game.Plugins.Values)
            {
                plugin.OnLoad();
            }
        }
    }

}
