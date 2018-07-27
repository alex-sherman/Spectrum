﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Input;

namespace Spectrum.Framework.Screens.InputElements
{
    public class TextInput : InputElement
    {
        public bool Selected = false;
        public int CursorPosition = 0;
        public string Text = "";
        public TextInput()
        {
            Width = 250;
            Height = Font.LineSpacing;
            OnClick += (_) => Selected = true;
        }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            if (!Display)
                return false;
            otherTookInput = base.HandleInput(otherTookInput, input);
            if(Selected)
            {
                if (input.IsNewMousePress(0) && !MouseInside())
                {
                    Selected = false;
                    return false;
                }
                input.TakeKeyboardInput(ref CursorPosition, ref Text);
                return true;
            }
            return otherTookInput;
        }
        public override void OnMeasure(int width, int height)
        {
            MeasuredWidth = Width.Measure(width, (int)Font.MeasureString(Text).X);
            MeasuredHeight = Height.Measure(height, (int)Math.Max(Font.LineSpacing, Font.MeasureString(Text).Y));
        }
        public override void Draw(GameTime time, SpriteBatch spritebatch)
        {
            base.Draw(time, spritebatch);
            spritebatch.DrawString(Font, Text, new Vector2(Rect.X, Rect.Y), FontColor, Layer(2));
        }
    }
}