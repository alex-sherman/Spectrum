using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InterfaceElements
{
    public class TextBox : InterfaceElement
    {
        public bool Selected { get; private set; }
        public string Text = "";
        private int textPosition = 0;
        public TextBox NextBox = null;
        public TextBox PrevBox = null;
        public InterfaceEventHandler OnContinue;
        public TextBox(GameScreen parent, SpriteFont font = null, ScalableTexture texture = null)
            : base(parent, font, texture)
        {
            FlatWidth = (int)Font.MeasureString("a").X * 20 + 2 * Texture.BorderWidth;
            FlatHeight = Font.LineSpacing + 2 * Texture.BorderWidth;
            OnClick += TextBox_OnClick;
        }

        void TextBox_OnClick(InterfaceElement clicked)
        {
            Selected = true;
            textPosition = 0;
            int mouseX = Mouse.GetState().X;
            while (textPosition < Text.Count() && Font.MeasureString(Text.Substring(0, textPosition + 1)).X + InsideRect.X - Font.Spacing < mouseX)
            {
                textPosition++;
            }
        }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            if (input.IsNewMousePress(0) && !Rect.Contains(Mouse.GetState().X, Mouse.GetState().Y))
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
                    OnContinue(this);
                }
                ScreenManager.TakeKeyboardInput(ref textPosition, ref Text, input);
            }
            return base.HandleInput(otherTookInput, input) || Selected;
        }
        public override void Draw(GameTime time, float layer)
        {
            ScreenManager.CurrentManager.DrawString(Font, Text, new Vector2(Rect.X + Font.Spacing + Texture.BorderWidth, Rect.Y + Texture.BorderWidth), Color.Black, ScreenManager.Layer(3, layer));
            if (Selected)
            {
                ScreenManager.CurrentManager.DrawString(Font, "|", new Vector2(InsideRect.X + Font.MeasureString(Text.Substring(0, textPosition)).X, InsideRect.Y), Color.Black, ScreenManager.Layer(3, layer));
            }
            base.Draw(time, layer);
        }
    }
}
