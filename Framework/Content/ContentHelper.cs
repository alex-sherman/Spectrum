﻿using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Audio;
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
        public static Dictionary<Type, ICachedContentParser> ContentParsers = new Dictionary<Type, ICachedContentParser>()
            {
                {typeof(SpecModel), new ModelParser()},
                {typeof(AnimationPlayer), new AnimationParser()},
                {typeof(Texture2D), new Texture2DParser()},
                {typeof(ScalableTexture), new ScalableTextureParser()},
                {typeof(float[,]), new HeightmapParser()},
                {typeof(SoundEffect), new SoundParser()}
            };
        public ContentManager Content { get; private set; }
        public ContentHelper(ContentManager content)
        {
            Content = content;
        }

        public static T Load<T>(string path, bool usePrefix) where T : class
        {
            if (usePrefix && path.Contains('@'))
            {
                string[] split = path.Split('@');
                Plugin plugin;
                if (SpectrumGame.Game.Plugins.TryGetValue(split[0], out plugin))
                    return plugin.Content._load<T>(split[1], true);
            }
            return (T)Single._load<T>(path, usePrefix);
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

        private T _load<T>(string path, bool usePrefix) where T : class
        {
            try
            {
                if (ContentParsers.ContainsKey(typeof(T)))
                {
                    ICachedContentParser parser = ContentParsers[typeof(T)];
                    if (usePrefix)
                        path = Content.RootDirectory + "\\" + parser.Prefix + path;
                    path += parser.Suffix;
                    return (T)ContentParsers[typeof(T)].Load(path);
                }
                if (typeof(T) == typeof(SpriteFont)) { path = @"Fonts\" + path; }
                if (typeof(T) == typeof(Effect))
                {
                    path = @"HLSL\" + path + ".mgfx";
                }
                return Content.Load<T>(path);
            }
            catch (FileNotFoundException)
            {
                throw new ContentLoadException();
            }
        }
    }
}
