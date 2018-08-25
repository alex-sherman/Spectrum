using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{
    public class Button : InputElement
    {
        public Button(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Button(string text)
        {
            AddElement(new TextElement(text));
            Width.WrapContent = true;
            Height.WrapContent = true;
        }
    }
}
