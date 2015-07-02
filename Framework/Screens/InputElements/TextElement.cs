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

        public override void Initialize()
        {
            base.Initialize();
            Width.Flat = (int)Font.MeasureString(Text).X;
            Height.Flat = (int)Font.MeasureString(Text).Y;
        }

        public override void Draw(GameTime time)
        {
            base.Draw(time);
            if (Text != null)
            {
                Vector2 pos = new Vector2(Rect.X, Rect.Y);
                ScreenManager.CurrentManager.DrawString(Font, Text, pos, FontColor, Layer(1));
            }
        }
    }
}
