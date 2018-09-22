using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Input;
using Spectrum.Framework.VR;

namespace Spectrum.Framework.Screens.InputElements
{
    public class TextInput : InputElement
    {
        public event Action<bool> OnFocusChanged;
        private bool _focused = false;
        public bool Focused
        {
            get => _focused;
            set
            {
                if (value != _focused)
                {
                    _focused = value;
                    OnFocusChanged?.Invoke(value);
                }
            }
        }
        public int CursorPosition = 0;
        private string text = "";
        public string Text { get => text; set => text = value ?? ""; }
        private bool isVR = false;
        public TextInput()
        {
            Width = 250;
            OnClick += (_) =>
            {
                if (isVR)
                    if (!SpecVR.IsKeyboardVisible)
                        SpecVR.ShowKeyboard(text);
                Focused = true;
            };
            OnDisplayChanged += (display) => Focused &= display;
        }
        public override void Initialize()
        {
            isVR = Selector.IsVR.Matches(this);
            OnFocusChanged += (focus) =>
            {
                if (!focus && isVR)
                    SpecVR.HideKeyboard();
            };
        }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            if (!Display)
                return false;
            otherTookInput = base.HandleInput(otherTookInput, input);
            if (Focused)
            {
                if (input.IsNewMousePress(0) && !MouseInside(input))
                {
                    Focused = false;
                    return false;
                }
                if (isVR)
                    text = SpecVR.GetKeyBoardText();
                else
                    input.TakeKeyboardInput(ref CursorPosition, ref text);
                return true;
            }
            return otherTookInput;
        }
        public override void OnMeasure(int width, int height)
        {
            MeasuredWidth = MeasureWidth(width, (int)Font.MeasureString(text).X);
            MeasuredHeight = MeasureHeight(height, (int)Math.Max(Font.MeasureString("a").Y, Font.MeasureString(text).Y));
        }
        public override void Draw(float time, SpriteBatch spritebatch)
        {
            base.Draw(time, spritebatch);
            spritebatch.DrawString(Font, text, new Vector2(Rect.X, Rect.Y), FontColor, LayerDepth);
        }
    }
}
