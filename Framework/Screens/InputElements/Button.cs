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
        public readonly TextElement TextElement;
        public string Text
        {
            get => TextElement.Text;
            set => TextElement.Text = value;
        }
        public Button(string text)
        {
            TextElement = AddElement(new TextElement(text) { Positioning = PositionType.Center });
        }
    }
}
