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
    public class LoadableTypeAttribute : Attribute
    {
        public string Name;
        public LoadableTypeAttribute(string name) { Name = name; }
        public LoadableTypeAttribute() { }
    }

    public class LoadHelper
    {
        public static Assembly SpectrumAssembly => Assembly.GetExecutingAssembly();
        public static Assembly MainAssembly { get; private set; }
        internal static void SetMainAssembly<T>()
        {
            MainAssembly = typeof(T).Assembly;
        }
        public static void RegisterTypes(Plugin plugin)
        {
            TypeHelper.RegisterType(typeof(ElementStyle), null);
            foreach (Type type in plugin.GetLoadableTypes())
            {
                Serialization.RegisterType(type);
                var typeData = TypeHelper.RegisterType(type, plugin);
                var accessor = TypeHelper.Model.GetTypeAccessor(type);
                var initData = new InitData(accessor);
                var preloadedMembers = accessor.Type.GetCustomAttributes<ClassContentAttribute>().Select(preload => (accessor.Members[preload.Member], preload));
                foreach ((var member, var preload) in preloadedMembers)
                {
                    initData.Set(member.Info.Name, preload.Path);
                }
                foreach (var member in accessor.Members.Values.Where(member => !member.Info.IsStatic && member.Info.GetAttribute<MemberContentAttribute>() != null))
                {
                    var preload = member.Info.GetAttribute<MemberContentAttribute>();
                    initData.Set(member.Info.Name, preload.Path);
                }
                var prefabName = type.GetCustomAttribute<LoadableTypeAttribute>()?.Name ?? type.Name;
                InitData.Register(prefabName, initData);
                foreach (var member in accessor.Members.Values.Where(m => m.Info.IsStatic))
                {
                    var preload = member.Info.GetAttribute<MemberContentAttribute>();
                    if (preload == null) continue;
                    object content = ContentHelper.LoadType(member.Type, preload.Path);
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
            foreach (var entity in manager.Entities.All)
                entity.Reload();
        }


        public static void LoadTypes(string LocalDir = null)
        {
            if (MainAssembly == null) MainAssembly = Assembly.GetEntryAssembly();
            SpectrumGame.Game.Plugins["Main"] = Plugin.CreatePlugin("Main", ContentHelper.Single, MainAssembly);
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
