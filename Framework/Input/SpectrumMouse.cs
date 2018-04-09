using Microsoft.Xna.Framework;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Spectrum.Framework.Input
{
    public class SpectrumMouseState
    {
        public bool[] buttons;
        public int X;
        public int Y;
        public float DX;
        public float DY;
        public int Scroll;
    }

    public class SpectrumMouse
    {
        public static bool UseRaw = true;
        private Mouse mouse;
        public SpectrumMouse(DirectInput di)
        {
            mouse = new Mouse(di);
            mouse.Acquire();
        }
        public SpectrumMouseState GetCurrentState()
        {
            MouseState state = mouse.GetCurrentState();
            bool[] buttons = new bool[16];
            if (UseRaw)
                RawMouse.buttons.CopyTo(buttons, 0);
            else
                state.Buttons.CopyTo(buttons, 0);
            Microsoft.Xna.Framework.Input.MouseState xnaMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            return new SpectrumMouseState()
            {
                buttons = buttons,
                X = xnaMouseState.X,
                Y = xnaMouseState.Y,
                DX = UseRaw ? (RawMouse.lastX / 2.0f) : xnaMouseState.X - SpectrumGame.Game.GraphicsDevice.Viewport.Width / 2,
                DY = UseRaw ? (RawMouse.lastY / 2.0f) : xnaMouseState.Y - SpectrumGame.Game.GraphicsDevice.Viewport.Height / 2,
                Scroll =state.Z,
            };
        }
    }
}
