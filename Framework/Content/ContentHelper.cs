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

namespace Spectrum.Framework.Content
{
    public class ContentHelper
    {
        private static ContentHelper single;
        public List<string> Directories = new List<string>() { "Content" };
        public static ContentHelper Single { get { if (single == null) { single = new ContentHelper(SpectrumGame.Game.Content); } return single; } }
        static Texture2D blank;
        public static Texture2D Blank { get { if(blank == null) blank = Load<Texture2D>("blank"); return blank; } }
        static Texture2D missing;
        public static Texture2D Missing { get { if(missing == null) missing = Load<Texture2D>("missing"); return missing; } }
        public static Dictionary<Type, IContentParser> ContentParsers = new Dictionary<Type, IContentParser>()
            {
                {typeof(Effect), new EffectParser()},
                {typeof(SpecModel), new ModelParser()},
                {typeof(AnimationData), new AnimationParser()},
                {typeof(ImageAsset), new ImageAssetParser()},
                {typeof(Texture2D), new Texture2DParser()},
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
            name = name.Replace('/', '\\');
            if (usePrefix && name.Contains('@'))
            {
                string[] split = name.Split('@');
                Plugin plugin;
                if (SpectrumGame.Game.Plugins.TryGetValue(split[0], out plugin))
                    return plugin.Content.LoadRelative<T>(split[1], true);
            }
            return (T)Single.LoadRelative<T>(name, usePrefix);
        }

        public static object LoadType(Type type, string path)
        {
            MethodInfo load = typeof(ContentHelper).GetMethod("Load", new Type[] { typeof(string), typeof(bool) });
            load = load.MakeGenericMethod(type);
            return load.Invoke(null, new object[] { path, true });
        }

        public T LoadRelative<T>(string path, bool usePrefix) where T : class
        {
            Type t = typeof(T);
            if (ContentParsers.ContainsKey(t))
            {
                IContentParser parser = ContentParsers[t];
                if (!usePrefix)
                    return (T)parser.Load(path);
                foreach (var directory in Directories)
                {
                    T output = (T)parser.Load(Path.Combine(directory, parser.Prefix, path));
                    if (output != null)
                        return output;
                }
            }
            DebugPrinter.print(string.Format("File not found {0}", path));
            return null;
        }
    }
}
