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
        public Button(int width, int height) : this()
        {
            Width = width;
            Height = height;
        }

        public Button(string text) : this()
        {
            AddElement(new TextElement(text) { Positioning = PositionType.Center });
        }

        public Button(params Element[] elements)
        {
            foreach (var element in elements)
                AddElement(element);
        }
    }
}
