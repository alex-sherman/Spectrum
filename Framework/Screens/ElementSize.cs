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
        public static ElementSize Zero = new ElementSize(0, 0);
        public SizeType Type;
        public int Size { get; set; }
        public ElementSize(SizeType type, int size)
        {
            Type = type;
            Size = size;
        }
        public int Measure(int parent, int content)
        {
            switch (Type)
            {
                case SizeType.Flat:
                    return Size;
                case SizeType.MatchParent:
                    return parent;
                case SizeType.WrapContent:
                    return Math.Max(content, Size);
            }
            return 0;
        }
        /// <summary>
        /// Sets a flat size for the element
        /// </summary>
        public int Flat { set { Size = value; Type = SizeType.Flat; } }
        /// <summary>
        /// Sets a minimum size for the element and otherwise wraps its contents
        /// </summary>
        public int MinWidth { set { Size = value; Type = SizeType.WrapContent; } }
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
            if(obj is ElementSize)
            {
                var size = (ElementSize)obj;
                return size.Type == Type && size.Size == Size;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Type.GetHashCode() << 5 ^ Size.GetHashCode();
        }
    }
}
