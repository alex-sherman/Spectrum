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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Screens;

namespace Spectrum.Framework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LoadableType : System.Attribute { }

    public class LoadHelper
    {
        public static void RegisterTypes(Plugin plugin)
        {
            TypeHelper.RegisterType(typeof(ElementStyle), null);
            foreach (Type type in plugin.GetLoadableTypes())
            {
                Serialization.RegisterType(type);
                var typeData = TypeHelper.RegisterType(type, plugin);
                var accessor = TypeHelper.Model.GetTypeAccessor(type);
                foreach (var member in accessor.Members.Values.Where(m => m.Info.IsStatic))
                {
                    var preload = member.Info.GetAttribute<PreloadedContentAttribute>();
                    if (preload == null) continue;
                    object content = ContentHelper.LoadType(preload.Type ?? member.Type, preload.Path);
                    member.SetValue(null, content);
                }
            }
        }
        public static void LoadPrefabs(bool refreshCache = false)
        {
            foreach (var plugin in SpectrumGame.Game.Plugins.Values)
                LoadPrefabs(plugin, refreshCache);
        }
        public static void LoadPrefabs(Plugin plugin, bool refreshCache = false)
        {
            try
            {
                foreach (var prefab in ContentHelper.LoadAll<InitData>().Where(p => p?.Name != null))
                    InitData.Register(prefab.Name, prefab);
            }
            catch (DirectoryNotFoundException) { }
            catch (FileNotFoundException) { }
        }
        public static void ReloadPrefabs(EntityManager manager)
        {
            LoadPrefabs(true);
            foreach (var prefab in InitData.Prefabs)
            {
                foreach (var entity in manager.FindByPrefab(prefab.Key))
                {
                    entity.InitData = prefab.Value;
                }
            }
            foreach (var entity in manager)
                entity.Reload();
        }

        public static Assembly SpectrumAssembly => Assembly.GetExecutingAssembly();

        public static void LoadTypes(string LocalDir = null)
        {
            SpectrumGame.Game.Plugins["Main"] = Plugin.CreatePlugin("Main", ContentHelper.Single, Assembly.GetEntryAssembly());
            SpectrumGame.Game.Plugins["Spectrum"] = Plugin.CreatePlugin("Spectrum", ContentHelper.Single, SpectrumAssembly);

            foreach (var pluginName in Plugin.GetPluginNames())
                SpectrumGame.Game.Plugins[pluginName] = Plugin.CreatePlugin(pluginName);

            //Load all types before calling OnLoad
            foreach (var plugin in SpectrumGame.Game.Plugins.Values)
                RegisterTypes(plugin);

            foreach (var plugin in SpectrumGame.Game.Plugins.Values)
                LoadPrefabs(plugin);

            foreach (var plugin in SpectrumGame.Game.Plugins.Values)
                plugin.OnLoad();
        }
    }

}
