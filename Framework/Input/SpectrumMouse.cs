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
        public Point P;
        public float DX;
        public float DY;
        public int ScrollX;
        public int ScrollY;
        public CursorState()
        {
            P = new Point { X = -1, Y = -1 };
            buttons = new bool[16];
        }
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
                P = mousePosition,
                DX = RawMouse.lastX / 2.0f,
                DY = RawMouse.lastY / 2.0f,
                ScrollY = RawMouse.lastZ,
            };
        }
    }
}
