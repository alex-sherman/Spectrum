using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InterfaceElements
{
    public class ListOption : InterfaceElement
    {
        public string text;
        public ListOption(GameScreen parent, Rectangle rect, object tag, string text) : base(parent, rect)
        {
            this._rect = rect;
            this.text = text;
            this.Tag = tag;
        }
        public override Rectangle Rect
        {
            get
            {
                return _rect;
            }
        }
        public override void Draw(GameTime time, float layer)
        {
            base.Draw(time, ScreenManager.Layer(2, layer));
            ScreenManager.CurrentManager.DrawString(Font, text, new Vector2(InsideRect.X, InsideRect.Y), Color.Black, ScreenManager.Layer(3, layer));
        }
    }
}
