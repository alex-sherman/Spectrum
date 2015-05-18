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

        public MenuButton(MenuScreen parent, string text)
            : base(parent)
        {
            this.Font = ScreenManager.Font;
            this._rect.Width = (int)this.Font.MeasureString(text).X + 2 * Texture.BorderWidth;
            this._rect.Height = (int)(this.Font.MeasureString(text).Y) + 2 * Texture.BorderWidth;
            this.Text = text;
            parent.CenterElement(this);
        }
        public override void Draw(GameTime time, float layer)
        {
            base.Draw(time, layer);
            ScreenManager.CurrentManager.DrawString(Font, Text,new Vector2(InsideRect.X, InsideRect.Y), Color.Gray, ScreenManager.TopLayer(layer));
        }
    }
}
