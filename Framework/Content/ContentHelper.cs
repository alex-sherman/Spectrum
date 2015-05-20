using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private Dictionary<Type, ICachedContentParser> parserCache = new Dictionary<Type, ICachedContentParser>()
            {
                {typeof(SpecModel), new ModelParser()},
                {typeof(AnimationPlayer), new AnimationParser()},
                {typeof(Texture2D), new ImageParser()}
            };
        public ContentManager Content { get; private set; }
        public ContentHelper(ContentManager content)
        {
            Content = content;
        }

        public static T Load<T>(string plugin, string path) where T : class
        {
            Plugin p = SpectrumGame.Game.Plugins[plugin];
            if (p == null) { return null; }
            return p.Content._load<T>(path);
        }

        public static T Load<T>(string path) where T : class
        {
            return (T)single._load<T>(path);
        }

        public static object LoadType(Type type, string path, string plugin)
        {
            MethodInfo load;
            if (plugin == null)
                load = typeof(ContentHelper).GetMethod("Load", new Type[] { typeof(string) });
            else
                load = typeof(ContentHelper).GetMethod("Load", new Type[] { typeof(string), typeof(string) });
            load = load.MakeGenericMethod(type);
            if (plugin == null)
                return load.Invoke(null, new object[] { path });
            else
                return load.Invoke(null, new object[] { plugin, path });
        }

        private T _load<T>(string path) where T : class
        {
            try
            {
                if (parserCache.ContainsKey(typeof(T)))
                {
                    ICachedContentParser parser = parserCache[typeof(T)];
                    path = parser.Prefix + path + parser.Suffix;
                    return (T)parserCache[typeof(T)].Load(Content.RootDirectory + "\\" + path);
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

        public Texture2D ObjectTexture(object obj)
        {
            if (obj == null) { return Blank; }
            PropertyInfo[] pinfos = obj.GetType().GetProperties();
            foreach (PropertyInfo pinfo in pinfos)
            {
                if (pinfo.Name == "GUITex")
                {
                    Texture2D output = pinfo.GetValue(obj, null) as Texture2D;
                    if (output != null) { return output; }
                }
            }
            return Blank;
        }
    }
}
