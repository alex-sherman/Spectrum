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
                    #region PreloadContent
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
                #endregion

            }
        }
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
            foreach (var plugin in SpectrumGame.Game.Plugins.Values)
            {
                plugin.Initialize();
            }

            LoadHelper.LoadTypes(Assembly.GetEntryAssembly().GetTypes());
            LoadHelper.LoadTypes(Assembly.GetExecutingAssembly().GetTypes());
            //TypeHelper.Helper["StatModifier"] = typeof(StatModifier);
            //TypeHelper.Helper["Player"] = typeof(Player);
            //TypeHelper.Helper["Water"] = typeof(Water);
            return TypeHelper.Types;
        }
    }

}
