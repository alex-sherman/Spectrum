using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public static Point operator +(Point a, Point b) => new Point(a.X + b.X, a.Y + b.Y);
        public static Point operator -(Point a, Point b) => new Point(a.X - b.X, a.Y - b.Y);
        public Point(int x, int y) { X = x; Y = y; }
        public static explicit operator Vector2(Point p) => new Vector2(p.X, p.Y);
    }
}
