using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens
{
    public struct ElementSize
    {
        public float Relative;
        public int Flat;
        public ElementSize(int flat, float relative)
        {
            Relative = relative;
            Flat = flat;
        }
    }
}
