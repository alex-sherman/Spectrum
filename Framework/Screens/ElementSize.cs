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
        public bool ParentRelative;
        public static ElementSize Zero = new ElementSize(0);
        private double Size;
        public ElementSize(double size)
        {
            Size = size;
            WrapContent = false;
            ParentRelative = false;
        }
        public int CropParentSize(int parent)
        {
            if (ParentRelative)
                return parent;
            else if (WrapContent)
                return 0;
            return (int)Size;
        }
        public int Measure(int parent, int content)
        {
            if(WrapContent)
            {
                if (ParentRelative)
                    return Math.Max(content, (int)(parent * Size));
                else
                    return Math.Max(content, (int)Size);
            }
            else
            {
                if (ParentRelative)
                    return (int)(parent * Size);
                else
                    return (int)Size;
            }
        }
        public static implicit operator ElementSize(int size)
        {
            return new ElementSize(size);
        }
        public static implicit operator ElementSize(double size)
        {
            return new ElementSize(size) { ParentRelative = true };
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
                   ParentRelative == size.ParentRelative &&
                   Size == size.Size;
        }

        public override int GetHashCode()
        {
            var hashCode = 76549531;
            hashCode = hashCode * -1521134295 + WrapContent.GetHashCode();
            hashCode = hashCode * -1521134295 + ParentRelative.GetHashCode();
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            return hashCode;
        }
        #endregion
    }
}
