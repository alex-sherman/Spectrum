using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Spectrum.Framework.Input
{
    public class CursorState
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
        Point mousePosition;
        public SpectrumMouse()
        {
            SpectrumGame.Game.WindowForm.MouseMove += WindowForm_MouseMove;
        }

        private void WindowForm_MouseMove(object sender, MouseEventArgs e)
        {
            mousePosition.X = e.X;
            mousePosition.Y = e.Y;
        }

        public CursorState GetCurrentState(CursorState previous = null)
        {
            bool[] buttons = new bool[16];
            RawMouse.buttons.CopyTo(buttons, 0);
            return new CursorState()
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
