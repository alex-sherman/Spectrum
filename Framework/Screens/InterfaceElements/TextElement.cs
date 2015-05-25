using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public override void Initialize()
        {
            base.Initialize();
            FlatWidth = (int)Font.MeasureString(Text).X;
            FlatHeight = (int)Font.MeasureString(Text).Y;
        }

        public override void Draw(GameTime time)
        {
            if (Text != null)
            {
                Vector2 pos = new Vector2(Rect.X, Rect.Y);
                ScreenManager.CurrentManager.DrawString(Font, Text, pos, Color.Black, Layer(1));
            }
        }
        public override bool HandleInput(bool otherTookInput, Input.InputState input)
        {
            return otherTookInput;
        }
    }
}
