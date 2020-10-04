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
        public static explicit operator Vector2(Point point) => new Vector2(point.X, point.Y);
        public static explicit operator Point(Vector2 vector) => new Point() { X = (int)Math.Floor(vector.X), Y = (int)Math.Floor(vector.Y) };
        public override string ToString()
        {
            return $"{X},{Y}";
        }
    }
}
