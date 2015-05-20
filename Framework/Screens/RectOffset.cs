using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens
{
    public struct RectOffset
    {
        public float LeftRelative;
        public int LeftOffset;
        public int Left(int ParentWidth)
        {
            return LeftOffset + (int)(LeftRelative * ParentWidth);
        }

        public float RightRelative;
        public int RightOffset;
        public int Right(int ParentWidth)
        {
            return RightOffset + (int)(RightRelative * ParentWidth);
        }

        public float TopRelative;
        public int TopOffset;
        public int Top(int ParentWidth)
        {
            return TopOffset + (int)(TopRelative * ParentWidth);
        }

        public float BottomRelative;
        public int BottomOffset;
        public int Bottom(int ParentWidth)
        {
            return BottomOffset + (int)(BottomRelative * ParentWidth);
        }
    }
}
