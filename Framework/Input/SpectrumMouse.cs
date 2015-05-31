using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Input
{
    public class SpectrumMouseState
    {
        public bool[] buttons;
        public int X;
        public int Y;
        public int Scroll;
        public SpectrumMouseState(bool[] buttons, int x, int y, int scroll)
        {
            this.buttons = buttons;
            X = x;
            Y = y;
            Scroll = scroll;
        }
    }

    class SpectrumMouse
    {
        private DirectInput di;
        private Mouse mouse;
        public SpectrumMouse()
        {
            di = new DirectInput();
            mouse = new Mouse(di);
            mouse.Acquire();
        }
        public SpectrumMouseState GetCurrentState()
        {
            MouseState state = mouse.GetCurrentState();
            bool[] buttons = state.Buttons;
            Microsoft.Xna.Framework.Input.MouseState xnaMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            return new SpectrumMouseState(buttons, xnaMouseState.X, xnaMouseState.Y, state.Z);
        }
    }
}
