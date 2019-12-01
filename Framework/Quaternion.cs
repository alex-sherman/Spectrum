using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public struct Quaternion
    {
        public static Quaternion Identity { get; } = new Quaternion { W = 1 };
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
        public Quaternion(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }
        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion
            {
                X = (a.X * b.W) + (b.X * a.W) + (a.Y * b.Z) - (a.Z * b.Y),
                Y = (a.Y * b.W) + (b.Y * a.W) + (a.Z * b.X) - (a.X * b.Z),
                Z = (a.Z * b.W) + (b.Z * a.W) + (a.X * b.Y) - (a.Y * b.X),
                W = (a.W * b.W) - (a.X * b.X) - (a.Y * b.Y) - (a.Z * b.Z)
            };
        }
        public static Vector3 operator *(Quaternion q, Vector3 v)
        {
            float x = 2 * (q.Y * v.Z - q.Z * v.Y);
            float y = 2 * (q.Z * v.X - q.X * v.Z);
            float z = 2 * (q.X * v.Y - q.Y * v.X);
            return new Vector3(
                v.X + x * q.W + (q.Y * z - q.Z * y),
                v.Y + y * q.W + (q.Z * x - q.X * z),
                v.Z + z * q.W + (q.X * y - q.Y * x));
        }
        public Quaternion Inverse()
        {
            float invLengthSquared = 1f / X * X + Y * Y + Z * Z + W * W;
            return new Quaternion
            {
                X = -X * invLengthSquared,
                Y = -Y * invLengthSquared,
                Z = -Z * invLengthSquared,
                W = W * invLengthSquared
            };
        }
        public Quaternion Normal()
        {
            float invLength = 1f / (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W));
            return new Quaternion
            {
                X = X * invLength,
                Y = W * invLength,
                Z = Z * invLength,
                W = W * invLength,
            };
        }
    }
}
