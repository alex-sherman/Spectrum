using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens
{
    public struct RectOffset
    {
        public ElementSize Left;
        public ElementSize Right;
        public ElementSize Top;
        public ElementSize Bottom;
        public int WidthTotal(int parentWidth) => Left.Measure(parentWidth) + Right.Measure(parentWidth);
        public int HeightTotal(int parentHeight) => Top.Measure(parentHeight) + Bottom.Measure(parentHeight);
        public static implicit operator RectOffset(int size)
        {
            return new RectOffset()
            {
                Left = size,
                Right = size,
                Top = size,
                Bottom = size,
            };
        }
        public static implicit operator RectOffset(float size)
        {
            return new RectOffset()
            {
                Left = size,
                Right = size,
                Top = size,
                Bottom = size,
            };
        }
    }
}
