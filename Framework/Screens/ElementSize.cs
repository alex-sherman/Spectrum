﻿using Microsoft.Xna.Framework;
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
        public float Measure(float parent, float content)
        {
            switch (Type)
            {
                case SizeType.Flat:
                    return Size;
                case SizeType.MatchParent:
                    return parent;
                case SizeType.WrapContent:
                    return content;
            }
            return 0;
        }
        public int Flat { set { Size = value; Type = SizeType.Flat; } }
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
