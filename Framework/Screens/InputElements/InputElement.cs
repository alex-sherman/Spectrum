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
    public class InputElement : Element
    {
        public event Action<InputElement> OnClick;
        public event Action<InputElement> OnRightClick;
        public object Data;
        public string HoverText;
        private TextElement hoverElement;
        public InputElement()
        {
            RegisterHandler(new KeyBind(0), (input) =>
            {
                if (OnClick != null && MouseInside(input))
                {
                    OnClick(this);
                    input.ConsumeInput(new KeyBind(0), true);
                    return true;
                }
                return false;
            });
            RegisterHandler(new KeyBind(1), (input) =>
            {
                if (OnRightClick != null && MouseInside(input))
                {
                    OnRightClick(this);
                    input.ConsumeInput(new KeyBind(1), true);
                    return true;
                }
                return false;
            });
        }
        private void ClearHoverText() { hoverElement?.Parent?.RemoveElement(hoverElement); hoverElement = null; }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            if (Display && MouseInside(input))
            {
                if (HoverText != null && hoverElement == null)
                {
                    hoverElement = Root.AddElement(new TextElement(HoverText)
                    {
                        Positioning = PositionType.Absolute,
                        BackgroundColor = Color.White,
                        Z = 1000,
                        ZDetach = true
                    });
                }
            }
            else if (hoverElement != null) { ClearHoverText(); }
            return base.HandleInput(otherTookInput, input);
        }
        public override void Draw(float gameTime, SpriteBatch spritebatch)
        {
            if (hoverElement != null)
            {
                hoverElement.X = InputState.Current.MousePosition.X;
                hoverElement.Y = InputState.Current.MousePosition.Y - hoverElement.MeasuredHeight;
            }
            base.Draw(gameTime, spritebatch);
        }
        public void Click() => OnClick?.Invoke(this);
        public void RightClick() => OnRightClick?.Invoke(this);
    }
}
