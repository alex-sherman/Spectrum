using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Spectrum.Framework
{
    public static class ExtensionMethods
    {
        public static IEnumerable<T> Union<T>(this IEnumerable<T> source, T item)
        {
            return source.Union(Enumerable.Repeat(item, 1));
        }
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source) where T : struct
        {
            return source.Where(t => t.HasValue).Cast<T>();
        }
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source) where T : class
        {
            return source.Where(t => t != null).Cast<T>();
        }
        public static float DT(this GameTime time)
        {
            return (float)(time.ElapsedGameTime.TotalMilliseconds / 1000);
        }

        public static T Pop<T>(this List<T> list)
        {
            var ele = list[0];
            list.RemoveAt(0);
            return ele;
        }
        public static float NextFloat(this Random r, float start, float end)
            => (float)(r.NextDouble() * (end - start) + start);
    }
}
