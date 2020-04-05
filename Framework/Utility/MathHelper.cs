using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public static class MathHelper
    {
        public static float Lerp(float a, float b, float w)
        {
            return a * (1 - w) + b * w;
        }
        public static double Lerp(double a, double b, double w)
        {
            return a * (1 - w) + b * w;
        }
        public static float Clamp(float v, float min, float max)
        {
            return Math.Min(Math.Max(v, min), max);
        }
    }
}
