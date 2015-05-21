using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InterfaceElements
{
    public class Button : InterfaceElement
    {
        public object DrawObject;
        public string Text;

        public Button(GameScreen screen, int width, int height, SpriteFont font = null, ScalableTexture texture = null)
            : base(screen, font: font, texture: texture)
        {
            FlatWidth = width;
            FlatHeight = height;
        }

        public Button(GameScreen screen, string text, SpriteFont font = null, ScalableTexture texture = null)
            : base(screen, font: font, texture: texture)
        {
            this.Text = text;
            FlatWidth = (int)Font.MeasureString(text).X + 2 * Texture.BorderWidth;
            FlatHeight = (int)Font.LineSpacing + 2 * Texture.BorderWidth;
        }
        public override void Draw(GameTime time, float layer)
        {
            if (DrawObject != null)
            {
                ScreenManager.CurrentManager.Draw(ScreenManager.CurrentManager.TextureLoader.ObjectTexture(DrawObject), InsideRect, Color.White, ScreenManager.Layer(1, layer));
            }
            if (Text != null)
            {
                Vector2 pos = new Vector2(Rect.X, Rect.Y) + (new Vector2(Rect.Width, Rect.Height) - Font.MeasureString(Text)) / 2;
                ScreenManager.CurrentManager.DrawString(Font, Text, pos, Color.Black, ScreenManager.Layer(2, layer));
            }
            base.Draw(time, layer);
        }
    }
}
