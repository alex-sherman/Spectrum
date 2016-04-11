using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens
{
    public struct ElementSize
    {
        public static ElementSize Zero = new ElementSize(0, 0);
        public float Relative;
        public float Flat;
        public ElementSize(float flat = 0, float relative = 0)
        {
            Relative = relative;
            Flat = flat;
        }
        public float Apply(float size = 1, float offset = 0)
        {
            return Relative * size + Flat + offset;
        }
    }
    public struct ElementSize2D
    {
        public static ElementSize2D Zero = new ElementSize2D(ElementSize.Zero, ElementSize.Zero);
        public ElementSize X;
        public ElementSize Y;
        public ElementSize2D(float XFlat = 0, float XRelative = 0, float YFlat = 0, float YRelative = 0)
            : this(new ElementSize(XFlat, XRelative), new ElementSize(YFlat, YRelative)) { }
        public ElementSize2D(ElementSize X, ElementSize Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public Vector2 Apply(Vector2? size = null, Vector2? offset = null)
        {
            Vector2 sizeValue = size ?? Vector2.One;
            Vector2 offsetValue = offset ?? Vector2.Zero;
            return new Vector2(X.Relative, Y.Relative) * sizeValue + offsetValue + new Vector2(X.Flat, Y.Flat);
        }
    }
}
