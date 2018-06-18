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
    public class ContentLoadException : Exception
    {

    }

    public class ContentHelper
    {
        private static ContentHelper single;
        public static ContentHelper Single { get { if (single == null) { single = new ContentHelper(SpectrumGame.Game.Content); } return single; } }
        public static Texture2D Blank { get { return ContentHelper.Load<Texture2D>("blank"); } }
        public static Texture2D Missing { get { return ContentHelper.Load<Texture2D>("missing"); } }
        public static Dictionary<Type, ICachedContentParser> ContentParsers = new Dictionary<Type, ICachedContentParser>()
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
            };
        public ContentManager Content { get; private set; }
        public ContentHelper(ContentManager content)
        {
            Content = content;
        }

        public static T Load<T>(string name, bool usePrefix) where T : class
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

        public static T Load<T>(string path) where T : class
        {
            return Load<T>(path, true);
        }

        public static object LoadType(Type type, string path)
        {
            MethodInfo load = typeof(ContentHelper).GetMethod("Load", new Type[] { typeof(string) });
            load = load.MakeGenericMethod(type);
            return load.Invoke(null, new object[] { path });
        }

        public T LoadRelative<T>(string path, bool usePrefix) where T : class
        {
            Type t = typeof(T);
            if (ContentParsers.ContainsKey(t))
            {
                ICachedContentParser parser = ContentParsers[t];
                if (usePrefix)
                    path = Path.Combine(Content.RootDirectory, parser.Prefix, path);
                return (T)parser.Load(path);
            }
            if (typeof(T) == typeof(SpriteFont)) { path = @"Fonts\" + path; }
            if (typeof(T) == typeof(Effect))
            {
                path = @"HLSL\" + path + ".mgfx";
            }
            // TODO: Could be totally removed if fonts can be imported some other way...
            return Content.Load<T>(path);
        }
    }
}
