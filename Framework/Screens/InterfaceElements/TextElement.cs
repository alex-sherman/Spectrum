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

        public TextElement(Element parent, string text)
            : base(parent)
        {
            Text = text;
        }

        public override void Draw(GameTime time, float layer)
        {
            if (Text != null)
            {
                Vector2 pos = new Vector2(Rect.X, Rect.Y) + (new Vector2(Rect.Width, Rect.Height) - Font.MeasureString(Text)) / 2;
                ScreenManager.CurrentManager.DrawString(Font, Text, pos, Color.Black, ScreenManager.Layer(2, layer));
            }
        }
    }
}
