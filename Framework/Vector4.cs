using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public struct Vector4
    {
        public static Vector4 Zero => new Vector4();
        public static Vector4 UnitX => new Vector4() { X = 1 };
        public static Vector4 UnitY => new Vector4() { Y = 1 };
        public static Vector4 UnitZ => new Vector4() { Z = 1 };
        public static Vector4 UnitW => new Vector4() { W = 1 };
        public static Vector4 One => new Vector4(1);
        public Vector4(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }
        public Vector4(float s) { X = s; Y = s; Z = s; W = s; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
        public static Vector4 operator -(Vector4 a) => new Vector4(-a.X, -a.Y, -a.Z, -a.W);
        public static Vector4 operator -(Vector4 a, Vector4 b) => new Vector4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        public static Vector4 operator +(Vector4 a, Vector4 b) => new Vector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        public static Vector4 operator /(Vector4 a, int d) => new Vector4(a.X / d, a.Y / d, a.Z / d, a.W / d);
        public static Vector4 operator /(Vector4 a, float d) => new Vector4(a.X / d, a.Y / d, a.Z / d, a.W / d);
        public static Vector4 operator *(Vector4 a, int d) => new Vector4(a.X * d, a.Y * d, a.Z * d, a.W * d);
        public static Vector4 operator *(Vector4 a, float d) => new Vector4(a.X * d, a.Y * d, a.Z * d, a.W * d);
        public static Vector4 operator *(int d, Vector4 a) => new Vector4(a.X * d, a.Y * d, a.Z * d, a.W * d);
        public static Vector4 operator *(float d, Vector4 a) => new Vector4(a.X * d, a.Y * d, a.Z * d, a.W * d);
        public static implicit operator Microsoft.Xna.Framework.Vector4(Vector4 vector) => new Microsoft.Xna.Framework.Vector4(vector.X, vector.Y, vector.Z, vector.W);
        public static implicit operator Vector4(Microsoft.Xna.Framework.Vector4 vector) => new Vector4(vector.X, vector.Y, vector.Z, vector.W);
        public Vector4 Normal() => new Vector4(X / Length, Y / Length, Z / Length, W / Length);
        public float Length => (float)Math.Pow(LengthSquared, 0.5);
        public float LengthSquared => (float)(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2) + Math.Pow(W, 2));
    }
}
