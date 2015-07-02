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
        public object Data;

        public string TitleText { get { return this["title"]; } }

        public InputElement()
        {
            this.Fields["title"] = new ElementField(
                this,
                "title",
                (value) => (value)
                );
        }

        public bool MouseInside()
        {
            return Rect.Contains(Mouse.GetState().X, Mouse.GetState().Y);
        }
        public override void Draw(GameTime time)
        {
            base.Draw(time);
            if (MouseInside() && TitleText != null)
            {
                ScreenManager.CurrentManager.DrawString(Font, TitleText, new Vector2(Mouse.GetState().X + 15, Mouse.GetState().Y), Color.Black, Layer(ZLayers - 1));
            }
        }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            otherTookInput |= base.HandleInput(otherTookInput, input);
            if (otherTookInput) { return true; }
            if (input.IsNewMousePress(0))
            {
                if (MouseInside())
                {
                    if (OnClick != null)
                    {
                        OnClick(this);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
