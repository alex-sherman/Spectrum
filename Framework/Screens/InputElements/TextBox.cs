using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{
    public class TextBox : InputElement
    {
        public bool Selected { get; private set; }
        public string Text = "";
        private int textPosition = 0;
        public TextBox NextBox = null;
        public TextBox PrevBox = null;
        public Action OnContinue;
        public override void Initialize()
        {
            base.Initialize();
            Width = (int)Font.MeasureString("a").X * 10;
            Height = Font.LineSpacing;
            OnClick += TextBox_OnClick;
        }

        void TextBox_OnClick(Element clicked)
        {
            Selected = true;
            textPosition = 0;
            int mouseX = Mouse.GetState().X;
            while (textPosition < Text.Count() && Font.MeasureString(Text.Substring(0, textPosition + 1)).X + AbsoluteX - Font.Spacing < mouseX)
            {
                textPosition++;
            }
        }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            if (input.IsNewMousePress(0) && !MouseInside(input))
            {
                Selected = false;
            }
            if (otherTookInput) { return false; }
            if (Selected)
            {
                if (input.IsNewKeyPress("MenuLeft"))
                {
                    textPosition = Math.Max(0, textPosition - 1);
                }
                else if (input.IsNewKeyPress("MenuRight"))
                {
                    textPosition = Math.Min(Text.Length, textPosition + 1);
                }
                else if (input.IsNewKeyPress("MenuCycleB"))
                {
                    if (PrevBox != null)
                    {
                        Selected = false;
                        PrevBox.Selected = true;
                        return true;
                    }
                }
                else if (input.IsNewKeyPress("MenuCycleF") && NextBox != null)
                {
                    Selected = false;
                    NextBox.Selected = true;
                    return true;
                }
                else if (input.IsNewKeyPress("Continue") && OnContinue != null)
                {
                    OnContinue();
                }
                input.TakeKeyboardInput(ref textPosition, ref Text);
            }
            return base.HandleInput(otherTookInput, input) || Selected;
        }
        public override void Draw(float time, SpriteBatch spritebatch)
        {
            spritebatch.DrawString(Font, Text, new Vector2(Rect.X + Font.Spacing, Rect.Y), Color.Black, Layer(3));
            if (Selected)
            {
                spritebatch.DrawString(Font, "|", new Vector2(AbsoluteX + Font.MeasureString(Text.Substring(0, textPosition)).X, AbsoluteY), Color.Black, Layer(3));
            }
            base.Draw(time, spritebatch);
        }
    }
}
