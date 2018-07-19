using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{
    public class Button : InputElement
    {
        public string Text;

        public Button(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Button(string text)
        {
            AddElement(new TextElement(text));
            Width.WrapContent = true;
            Height.WrapContent = true;
        }

        public override void Draw(GameTime time, SpriteBatch spritebatch)
        {
            base.Draw(time, spritebatch);
            if (Text != null)
            {
                Vector2 pos = new Vector2(Rect.X, Rect.Y) + (new Vector2(Rect.Width, Rect.Height) - Font.MeasureString(Text)) / 2;
                spritebatch.DrawString(Font, Text, pos, FontColor, Layer(2));
            }
        }
        public override void Measure(int width, int height)
        {
            base.Measure(width, height);
        }
    }
}
