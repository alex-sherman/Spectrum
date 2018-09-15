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

namespace Spectrum.Framework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LoadableType : System.Attribute { }

    public class LoadHelper
    {
        public static void RegisterTypes(Plugin plugin)
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
        public static void LoadPrefabs(bool refreshCache = false)
        {
            foreach (var plugin in SpectrumGame.Game.Plugins.Values)
                LoadPrefabs(plugin, refreshCache);
        }
        public static void LoadPrefabs(Plugin plugin, bool refreshCache = false)
        {
            try
            {
                var path = Path.Combine(plugin.Content.Directories[0], "InitData");
                var files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
                foreach (String filename in files)
                {
                    InitData prefab = plugin.Content.LoadRelative<InitData>(filename.Substring(path.Length + 1), true, refreshCache);
                    if (prefab?.Name != null)
                        InitData.Register(prefab.Name, prefab);
                }
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
                    // Clear render properties so removing fields like Texture will work
                    if (entity is GameObject gameObject)
                    {
                        gameObject.Material = null;
                    }
                    prefab.Value.Apply(entity);
                }
            }
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
