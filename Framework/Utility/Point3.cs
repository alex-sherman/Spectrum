using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public struct Point3 : IEquatable<Point3>
    {
        public int X;
        public int Y;
        public int Z;

        public Point3(int x, int y, int z)
        {
            X = x; Y = y; Z = z;
        }

        public override bool Equals(object obj)
        {
            if (obj is Point3 point)
                return this.Equals(point);
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }
        public static Point3 Zero => new Point3();
        public static explicit operator Vector3(Point3 point) => new Vector3(point.X, point.Y, point.Z);
        public static explicit operator Point3(Vector3 vector) => new Point3() { X = (int)Math.Floor(vector.X), Y = (int)Math.Floor(vector.Y), Z = (int)Math.Floor(vector.Z) };
        public static Point3 operator +(Point3 a, Point3 b) => new Point3() { X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z };
        public static Point3 operator -(Point3 a, Point3 b) => new Point3() { X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z };
        public static Point3 operator *(Point3 a, int s) => new Point3() { X = a.X * s, Y = a.Y * s, Z = a.Z * s };
        public static bool operator ==(Point3 a, Point3 b) => a.Equals(b);
        public static bool operator !=(Point3 a, Point3 b) => !a.Equals(b);
        public static Point3 Round(Vector3 vector) => new Point3() { X = (int)Math.Round(vector.X), Y = (int)Math.Round(vector.Y), Z = (int)Math.Round(vector.Z) };
        public override string ToString()
        {
            return $"{X},{Y},{Z}";
        }

        public bool Equals(Point3 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z;
        }
    }
}
