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
    public delegate void InterfaceEventHandler(InterfaceElement clicked);
    public class InterfaceElement : Element
    {
        public static ScalableTexture DefaultTexture;
        public event InterfaceEventHandler OnClick;
        public object Tag;

        public string MouseOverText;
        public bool MouseInside()
        {
            return Rect.Contains(Mouse.GetState().X, Mouse.GetState().Y);
        }
        public override void Draw(GameTime time)
        {
            base.Draw(time);
            if (MouseInside() && MouseOverText != null)
            {
                ScreenManager.CurrentManager.DrawString(Font, MouseOverText, new Vector2(Mouse.GetState().X + 15, Mouse.GetState().Y), Color.Black, Layer(ZLayers - 1));
            }
        }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            if (otherTookInput) { return false; }
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
