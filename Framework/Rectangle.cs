using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    [DebuggerDisplay("[X:{X} Y:{Y} Width:{Width} Height:{Height}]")]
    public struct Rectangle
    {
        public Rectangle(int x, int y, int width, int height) { X = x; Y = y; Width = width; Height = height; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Top => Y;
        public int Right => X + Width;
        public int Bottom => Y + Height;
        public int Left => X;
        public Point TopLeft => new Point(X, Y);
        public Rectangle Clip(Rectangle other)
        {
            var x = Math.Max(X, other.X);
            var y = Math.Max(Y, other.Y);
            return new Rectangle(x, y,
                Math.Min(Right, other.Right) - x, Math.Min(Bottom, other.Bottom) - y);
        }
        public bool Intersects(Rectangle other)
        {
            return other.X < X + Width && other.X + other.Width > X
                && other.Y < Y + Height && other.Y + other.Height > Y;
        }
        public bool Contains(Point p) => Left <= p.X && p.X <= Right && Top <= p.Y && p.Y <= Bottom;
        public Rectangle Translate(Point p) => new Rectangle(X + p.X, Y + p.Y, Width, Height);
        /// <summary>
        /// Scales the source rectangle (and optionally centers it) to the destination rectangle while maintaining the original aspect ratio.
        /// The crop paremeter determines whether to scale up (overflowing and requiring a crop) or down (requiring no crop).
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="crop">If true will scale the source up, else down</param>
        /// <param name="center"></param>
        /// <returns></returns>
        public Rectangle FitTo(Rectangle destination, bool crop = true, bool center = true)
        {
            bool scaleX = (!crop) ^ (destination.Width * 1.0 / Width * Height > destination.Height);
            double scale = scaleX ? destination.Width * 1.0 / Width : destination.Height * 1.0 / Height;
            var fittedX = X;
            var fittedY = Y;
            var fittedWidth = (int)(Width * scale);
            var fittedHeight = (int)(Height * scale);
            if (center)
            {
                fittedX = (destination.Width - fittedWidth) / 2;
                fittedY = (destination.Height - fittedHeight) / 2;
            }
            return new Rectangle(fittedX, fittedY, fittedWidth, fittedHeight);
        }
        public static implicit operator Microsoft.Xna.Framework.Rectangle(Rectangle r) => new Microsoft.Xna.Framework.Rectangle(r.X, r.Y, r.Width, r.Height);
        public static implicit operator Rectangle(Microsoft.Xna.Framework.Rectangle r) => new Rectangle(r.X, r.Y, r.Width, r.Height);
    }
}
