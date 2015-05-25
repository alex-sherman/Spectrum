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
        public ListOption(object tag, string text)
            : base()
        {
            this.text = text;
            this.Tag = tag;
        }
        public override void Draw(GameTime time)
        {
            base.Draw(time);
            ScreenManager.CurrentManager.DrawString(Font, text, new Vector2(X, Y), Color.Black, Layer(2));
        }
    }
}
