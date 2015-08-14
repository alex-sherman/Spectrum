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
    public class LoadableType : System.Attribute
    { }

    public class LoadHelper
    {
        public static void LoadTypes(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                if (type.GetCustomAttributes(true).Any((object attribute) => attribute is LoadableType))
                {
                    TypeHelper.Types[type.Name] = type;

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
        }
        public static void LoadTypes(string LocalDir = null)
        {
            foreach (var pluginName in Plugin.GetPluginNames())
            {
                SpectrumGame.Game.Plugins[pluginName] = Plugin.CreatePlugin(pluginName);
            }

            //Load all types before calling OnLoad
            foreach (var plugin in SpectrumGame.Game.Plugins.Values)
            {
                LoadHelper.LoadTypes(plugin.GetTypes());
            }

            LoadHelper.LoadTypes(Assembly.GetEntryAssembly().GetTypes());
            LoadHelper.LoadTypes(Assembly.GetExecutingAssembly().GetTypes());

            foreach (var plugin in SpectrumGame.Game.Plugins.Values)
            {
                plugin.OnLoad();
            }
        }
    }

}
