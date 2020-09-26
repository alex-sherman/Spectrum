using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Audio;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Graphics.Animation;
using Spectrum.Framework.Screens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Framework.Content
{
    public class ContentHelper
    {
        private static ContentHelper single;
        public List<string> Directories = new List<string>() { "Content" };
        public static ContentHelper Single { get { if (single == null) { single = new ContentHelper(SpectrumGame.Game?.Content); } return single; } }
        static Texture2D blank;
        public static Texture2D Blank { get { if (blank == null) blank = Load<Texture2D>("blank"); return blank; } }
        static Texture2D missing;
        public static Texture2D Missing { get { if (missing == null) missing = Load<Texture2D>("missing"); return missing; } }
        public static Dictionary<Type, IContentParser> ContentParsers = new Dictionary<Type, IContentParser>()
            {
                {typeof(Effect), new EffectParser()},
                {typeof(SpectrumEffect), new SpectrumEffectParser()},
                {typeof(SpecModel), new ModelParser()},
                {typeof(AnimationData), new AnimationParser()},
                {typeof(ImageAsset), new ImageAssetParser()},
                {typeof(Texture2D), new Texture2DParser()},
                {typeof(Component), new ScriptParser()},
                {typeof(ScriptAsset), new ScriptParser()},
                {typeof(float[,]), new HeightmapParser()},
                {typeof(SoundEffect), new SoundParser()},
                {typeof(InitData), new InitDataParser()},
                // TODO: Could be totally removed if fonts can be imported some other way...
                {typeof(SpriteFont), new MGCParser<SpriteFont>("Fonts", "")},

            };
        public ContentManager Content { get; private set; }
        public ContentHelper(ContentManager content)
        {
            Content = content;
        }

        public static void ClearCache()
        {
            foreach (var parser in ContentParsers.Values)
                parser.Clear();
        }

        public static T Load<T>(string name, bool usePrefix = true) where T : class
        {
            if (name == null) return null;
            if (usePrefix && name.Contains('@'))
            {
                string[] split = name.Split('@');
                Plugin plugin;
                if (SpectrumGame.Game.Plugins.TryGetValue(split[0], out plugin))
                    return plugin.Content.LoadRelative<T>(name, true);
                else throw new FileNotFoundException(@"No plugin found named {split[0]}");
            }
            return (T)Single.LoadRelative<T>(name, usePrefix);
        }

        public static IEnumerable<string> FindAll<T>(string glob = "*", bool recursive = true) where T : class
        {
            Type t = typeof(T);
            Dictionary<string, string> locations = new Dictionary<string, string>();
            var output = new List<string>();
            foreach (var plugin in SpectrumGame.Game.Plugins.Values)
            {
                var content = plugin.Content;
                if (ContentParsers.TryGetValue(t, out var parser))
                {
                    foreach (var directory in content.Directories)
                    {
                        foreach (var result in parser.FindAll(Path.Combine(directory, parser.Prefix), glob, recursive))
                        {
                            if (content == Single)
                                locations[result] = null;
                            else if (!locations.ContainsKey(result))
                                locations[result] = plugin.Name;
                        }
                    }
                }
            }
            return locations.Select(kvp => (kvp.Value == null ? "" : (kvp.Value + "@")) + kvp.Key);
        }

        public static IEnumerable<T> LoadAll<T>(string glob = "*", bool recursive = true) where T : class
            => FindAll<T>(glob, recursive).Select(s => Load<T>(s));

        public static object LoadType(Type type, string path)
        {
            MethodInfo load = typeof(ContentHelper).GetMethod("Load", new Type[] { typeof(string), typeof(bool) });
            load = load.MakeGenericMethod(type);
            return load.Invoke(null, new object[] { path, true });
        }

        private HashSet<(Type, string)> failedLoads = new HashSet<(Type, string)>();
        public T LoadRelative<T>(string name, bool usePrefix, bool refreshCache = false) where T : class
        {
            if (Content == null) return null;
            Type t = typeof(T);
            var path = new Regex(@"(.*@)?(.+)").Match(name).Groups[2].Value.Replace('/', '\\');
            if (!refreshCache && failedLoads.Contains((t, path))) return null;
            var root = Content.RootDirectory;
            if (ContentParsers.TryGetValue(t, out var parser))
            {
                if (!usePrefix)
                    return (T)parser.Load(path, name, refreshCache);
                foreach (var directory in Directories)
                {
                    try
                    {
                        var loaded = parser.Load(Path.Combine(root, directory, parser.Prefix, path), name, refreshCache);
                        if (loaded != null)
                        {
                            if (InitData.TryCast(typeof(T), loaded, out var output))
                                return (T)output;
                            DebugPrinter.Print($"Failed to cast {path} to {typeof(T).Name}");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        DebugPrinter.Print($"Failed to load {name}: {e.Message}");
                        break;
                    }
                }
                DebugPrinter.Print(string.Format("File not found {0}", name));
            }
            failedLoads.Add((t, path));
            return null;
        }
    }
}
