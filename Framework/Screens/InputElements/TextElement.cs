using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{
    public class TextElement : Element
    {
        public string Text { get; set; }

        public TextElement(string text)
            : base()
        {
            Text = text;
        }

        public override void OnMeasure(int width, int height)
        {
            if (Text == null)
            {
                MeasuredWidth = 0;
                MeasuredHeight = Font.LineSpacing;
            }
            else {
                MeasuredWidth = (int)Width.Measure(width, Font.MeasureString(Text).X);
                MeasuredHeight = (int)Height.Measure(height, Math.Max(Font.LineSpacing, Font.MeasureString(Text).Y));
            }
        }

        public override void Draw(GameTime time, SpriteBatch spritebatch)
        {
            base.Draw(time, spritebatch);
            if (Text != null)
            {
                Vector2 pos = new Vector2(Rect.X, Rect.Y);
                spritebatch.DrawString(Font, Text, pos, FontColor, Layer(1));
            }
        }
    }
}
