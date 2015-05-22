using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InterfaceElements
{
    public class MenuButton : InterfaceElement
    {
        public string Text;

        public MenuButton(string text)
            : base()
        {
            FlatWidth = (int)this.Font.MeasureString(text).X + 2 * Texture.BorderWidth;
            FlatHeight = (int)(this.Font.MeasureString(text).Y) + 2 * Texture.BorderWidth;
            this.Text = text;
        }
        public override void Draw(GameTime time)
        {
            base.Draw(time);
            ScreenManager.CurrentManager.DrawString(Font, Text,new Vector2(InsideRect.X, InsideRect.Y), Color.Gray, Layer(ZLayers - 1));
        }
    }
}
