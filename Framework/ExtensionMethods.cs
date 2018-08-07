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
            return (float)time.ElapsedGameTime.TotalMilliseconds / 1000.0f;
        }

        public static Plugin GetPlugin(this Type type)
        {
            return TypeHelper.GetPlugin(type);
        }

        public static string FixedLenString(this Vector3 vector)
        {
            return "<" + vector.X.ToString("0.00") + ", " + vector.Y.ToString("0.00") + ", " + vector.Z.ToString("0.00") + ">";
        }
        public static Vector3 Homogeneous(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z) / vector.W;
        }
        public static bool IsInSameDirection(this Vector3 vector, Vector3 otherVector)
        {
            return Vector3.Dot(vector, otherVector) > 0;
        }

        public static bool IsInOppositeDirection(this Vector3 vector, Vector3 otherVector)
        {
            return Vector3.Dot(vector, otherVector) < 0;
        }

        public static Vector3 Project(this Vector3 source, Vector3 normal)
        {
            return Vector3.Dot(source, normal) * normal;
        }

        public static void DrawString(this SpriteBatch spritebatch, SpriteFont font, string text, Vector2 pos, Color textColor, float layer)
        {
            spritebatch.DrawString(font, text, pos, textColor, 0, Vector2.Zero, 1, SpriteEffects.None, layer);
        }

        public static void DrawString(this SpriteBatch spritebatch, SpriteFont font, string text, Vector3 position, Color textColor, float scale = 1, float layer = 1)
        {
            Vector3 cameraPosition = GraphicsEngine.ViewPosition(position);
            Vector3 screenPos = GraphicsEngine.ViewToScreenPosition(cameraPosition);
            float size = scale * (screenPos - GraphicsEngine.ViewToScreenPosition(cameraPosition + Vector3.Up)).Length() / font.LineSpacing / font.LineSpacing;
            if (screenPos.Z < 1 && screenPos.Z > 0)
            {

                spritebatch.DrawString(font, text, new Vector2(screenPos.X, screenPos.Y) - font.MeasureString(text) * size / 2,
                    textColor, 0, Vector2.Zero, size, SpriteEffects.None, layer);
            }
        }

        public static void Draw(this SpriteBatch spritebatch, Texture2D tex, Rectangle rect, Color c, float layer)
        {
            spritebatch.Draw(tex, rect, null, c, 0, Vector2.Zero, SpriteEffects.None, layer);
        }

        public static T Pop<T>(this List<T> list)
        {
            var ele = list[0];
            list.RemoveAt(0);
            return ele;
        }
        /// <summary>
        /// Solution from: https://stackoverflow.com/questions/2869801/is-there-a-fast-alternative-to-creating-a-texture2d-from-a-bitmap-object-in-xna
        /// </summary>
        public static Texture2D GetTexture2DFromBitmap(this System.Drawing.Bitmap bitmap, GraphicsDevice device)
        {
            Texture2D tex = new Texture2D(device, bitmap.Width, bitmap.Height, false, SurfaceFormat.Bgra32);
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int bufferSize = data.Height * data.Stride;
            byte[] bytes = new byte[bufferSize];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            tex.SetData(bytes);
            bitmap.UnlockBits(data);
            return tex;
        }
        public static float Roll(this Quaternion quaternion)
        {
            // yaw (z-axis rotation)
            double siny = +2.0 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
            double cosy = +1.0 - 2.0 * (quaternion.X * quaternion.X + quaternion.Z * quaternion.Z);
            return (float)Math.Atan2(siny, cosy);
        }
        public static float Yaw(this Quaternion quaternion)
        {
            // roll (y-axis rotation)
            double sinr = +2.0 * (quaternion.W * quaternion.Y + quaternion.X * quaternion.Z);
            double cosr = +1.0 - 2.0 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
            return (float)Math.Atan2(sinr, cosr);
            //// pitch (y-axis rotation)
            //double sinp = +2.0 * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
            //if (Math.Abs(sinp) >= 1)
            //    return (float)(Math.Sign(sinp) * Math.PI / 2); // use 90 degrees if out of range
            //return (float)Math.Asin(sinp);
        }
        public static float Pitch(this Quaternion quaternion)
        {
            // pitch (x-axis rotation)
            double sinp = +2.0 * (quaternion.W * quaternion.X - quaternion.Z * quaternion.Y);
            if (Math.Abs(sinp) >= 1)
                return (float)(Math.Sign(sinp) * Math.PI / 2); // use 90 degrees if out of range
            return (float)Math.Asin(sinp);
        }
    }
}
