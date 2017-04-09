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
    public delegate void InterfaceEventHandler(InputElement clicked);
    public class InputElement : Element
    {
        public event InterfaceEventHandler OnClick;
        public event InterfaceEventHandler OnRightClick;
        public object Data;

        public string TitleText { get { return this["title"]; } }

        public InputElement()
        {
            Fields["title"] = new ElementField(
                this,
                "title",
                (value) => (value)
                );
        }

        public bool MouseInside()
        {
            return Rect.Contains(Mouse.GetState().X, Mouse.GetState().Y);
        }
        public override void Draw(GameTime time, SpriteBatch spritebatch)
        {
            base.Draw(time, spritebatch);
            if (HasFocus && MouseInside() && TitleText != null)
            {
                spritebatch.DrawString(Font, TitleText, new Vector2(Mouse.GetState().X + 15, Mouse.GetState().Y), Color.Black, 0);
            }
        }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            otherTookInput |= base.HandleInput(otherTookInput, input);
            if (otherTookInput) { return true; }
            if (MouseInside())
            {
                if (OnClick != null && input.IsNewMousePress(0))
                {
                    OnClick(this);
                    return true;
                }
                if (OnRightClick != null && input.IsNewMousePress(1))
                {
                    OnRightClick(this);
                    return true;
                }
            }
            return false;
        }
    }
}
