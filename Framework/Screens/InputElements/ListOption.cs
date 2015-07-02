﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{
    public class ListOption : InputElement
    {
        public string text;
        public ListOption(object tag, string text)
            : base()
        {
            this.text = text;
            this.Data = tag;
        }
        public override void Draw(GameTime time)
        {
            base.Draw(time);
            ScreenManager.CurrentManager.DrawString(Font, text, new Vector2(AbsoluteX, AbsoluteY), Color.Black, Layer(2));
        }
    }
}
