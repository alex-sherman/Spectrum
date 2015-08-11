using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{
    public class ListOption : InputElement
    {
        public TextElement text;
        public ListOption(object tag, string text)
            : base()
        {
            this.text = new TextElement(text);
            this.Data = tag;
        }
        public override void Initialize()
        {
            base.Initialize();
            AddElement(text);
            text.Center();
            Height.Flat = text.Height.Flat;
            Width.Relative = 1.0f;
        }
    }
}
