using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InterfaceElements
{
    public class TextElement : InterfaceElement
    {
        public string Text { get; protected set; }

        public TextElement(string text)
            : base()
        {
            Text = text;
        }

        public override void Draw(GameTime time)
        {
            if (Text != null)
            {
                Vector2 pos = new Vector2(Rect.X, Rect.Y) + (new Vector2(Rect.Width, Rect.Height) - Font.MeasureString(Text)) / 2;
                ScreenManager.CurrentManager.DrawString(Font, Text, pos, Color.Black, Layer(1));
            }
        }
    }
}
