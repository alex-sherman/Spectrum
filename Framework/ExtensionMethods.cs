using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework
{
    public static class ExtensionMethods
    {
        public static IEnumerable<T> Union<T>(this IEnumerable<T> source, T item)
        {
            return source.Union(Enumerable.Repeat(item, 1));
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
            normal.Normalize();
            return source - Vector3.Dot(source, normal) * normal;
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
    }
}
