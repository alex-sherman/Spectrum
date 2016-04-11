using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public static bool IsInSameDirection(this Vector3 vector, Vector3 otherVector)
        {
            return Vector3.Dot(vector, otherVector) > 0;
        }

        public static bool IsInOppositeDirection(this Vector3 vector, Vector3 otherVector)
        {
            return Vector3.Dot(vector, otherVector) < 0;
        }

        public static void DrawString(this SpriteBatch spritebatch, SpriteFont font, string text, Vector2 pos, Color textColor, float layer)
        {
            spritebatch.DrawString(font, text, pos, textColor, 0, Vector2.Zero, 1, SpriteEffects.None, layer);
        }

        public static void DrawString(this SpriteBatch spritebatch, SpriteFont font, string text, Vector3 position, Color textColor, float scale = 1, ElementSize2D? offset = null, float layer = 1)
        {
            ElementSize2D offsetValue = offset ?? ElementSize2D.Zero;
            Vector3 cameraPosition = GraphicsEngine.ViewPosition(position);
            Vector3 screenPos = GraphicsEngine.ViewToScreenPosition(cameraPosition);
            float size = scale * (screenPos - GraphicsEngine.ViewToScreenPosition(cameraPosition + Vector3.Up)).Length() / font.LineSpacing / font.LineSpacing;
            if (screenPos.Z < 1 && screenPos.Z > 0)
                spritebatch.DrawString(font, text, offsetValue.Apply(font.MeasureString(text) * size, new Vector2(screenPos.X, screenPos.Y)),
                    textColor, 0, Vector2.Zero, size, SpriteEffects.None, layer);
        }

        public static void Draw(this SpriteBatch spritebatch, Texture2D tex, Rectangle rect, Color c, float layer)
        {
            spritebatch.Draw(tex, rect, null, c, 0, Vector2.Zero, SpriteEffects.None, layer);
        }
    }
}
