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
using Spectrum.Framework.Network;

namespace Spectrum.Framework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LoadableType : System.Attribute { }

    public class LoadHelper
    {
        public static void LoadPlugin(Plugin plugin)
        {
            foreach (Type type in plugin.GetLoadableTypes())
            {
                Serialization.RegisterType(type);
                var typeData = TypeHelper.RegisterType(type, plugin);

                foreach (var member in typeData.members.Values.Where(mem => mem.PreloadedContent != null))
                {
                    var preload = member.PreloadedContent;
                    object content = ContentHelper.LoadType(preload.Type ?? member.MemberType, preload.Path);
                    if (member.IsStatic)
                        member.SetValue(null, content);
                }
            }
        }
        public static void LoadTypes(string LocalDir = null)
        {
            SpectrumGame.Game.Plugins["Main"] = Plugin.CreatePlugin("Main", ContentHelper.Single, Assembly.GetEntryAssembly());
            SpectrumGame.Game.Plugins["Spectrum"] = Plugin.CreatePlugin("Spectrum", ContentHelper.Single, Assembly.GetExecutingAssembly());

            foreach (var pluginName in Plugin.GetPluginNames())
            {
                SpectrumGame.Game.Plugins[pluginName] = Plugin.CreatePlugin(pluginName);
            }

            //Load all types before calling OnLoad
            foreach (var plugin in SpectrumGame.Game.Plugins.Values)
            {
                LoadPlugin(plugin);
            }

            foreach (var plugin in SpectrumGame.Game.Plugins.Values)
            {
                plugin.OnLoad();
            }
        }
    }

}
