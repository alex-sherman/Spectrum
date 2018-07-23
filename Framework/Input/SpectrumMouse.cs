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
        Point mousePosition;
        public SpectrumMouse()
        {
            SpectrumGame.Game.WindowForm.MouseMove += WindowForm_MouseMove;
            mouse = new Mouse(new DirectInput());
            mouse.Acquire();
        }

        private void WindowForm_MouseMove(object sender, MouseEventArgs e)
        {
            mousePosition.X = e.X;
            mousePosition.Y = e.Y;
        }

        public SpectrumMouseState GetCurrentState(SpectrumMouseState previous = null)
        {
            bool[] buttons = new bool[16];
            RawMouse.buttons.CopyTo(buttons, 0);
            return new SpectrumMouseState()
            {
                buttons = buttons,
                X = mousePosition.X,
                Y = mousePosition.Y,
                DX = RawMouse.lastX / 2.0f,
                DY = RawMouse.lastY / 2.0f,
                Scroll = RawMouse.lastZ,
            };
        }
    }
}
