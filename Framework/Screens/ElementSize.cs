using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens
{
    public enum SizeType
    {
        WrapContent = 0,
        Flat,
        MatchParent
    }
    public struct ElementSize
    {
        public bool WrapContent;
        public static ElementSize Zero = new ElementSize(0);
        public static ElementSize WrapFill = new ElementSize(0, 1, true);
        public static ElementSize Wrap = new ElementSize(0, 0, true);
        public double Relative;
        public int Flat;
        public ElementSize(int flat = 0, double relative = 0, bool wrapContent = false)
        {
            Flat = flat;
            Relative = relative;
            WrapContent = wrapContent;
        }
        public int Measure(int parent, int content = 0)
        {
            if (WrapContent)
                return Math.Max(content, (int)(parent * Relative) + Flat);
            else
                return (int)(parent * Relative) + Flat;
        }
        public static implicit operator ElementSize(int size)
        {
            return new ElementSize(size);
        }
        public static implicit operator ElementSize(double size)
        {
            return new ElementSize(relative: size);
        }
        #region Equality
        public static bool operator ==(ElementSize a, ElementSize b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(ElementSize a, ElementSize b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ElementSize))
            {
                return false;
            }

            var size = (ElementSize)obj;
            return WrapContent == size.WrapContent &&
                   Flat == size.Flat &&
                   Relative == size.Relative;
        }

        public override int GetHashCode()
        {
            var hashCode = 76549531;
            hashCode = hashCode * -1521134295 + WrapContent.GetHashCode();
            hashCode = hashCode * -1521134295 + Flat.GetHashCode();
            hashCode = hashCode * -1521134295 + Relative.GetHashCode();
            return hashCode;
        }
        #endregion
    }
}
