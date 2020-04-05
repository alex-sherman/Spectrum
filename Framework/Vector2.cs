using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public struct Vector2 : IEquatable<Vector2>
    {
        public static Vector2 Zero => new Vector2();
        public static Vector2 UnitX => new Vector2(1, 0);
        public static Vector2 UnitY => new Vector2(0, 1);
        public static Vector2 One => new Vector2(1, 1);
        public Vector2(float x, float y) { X = x; Y = y; }
        public Vector2(float d) { X = d; Y = d; }
        public float X { get; set; }
        public float Y { get; set; }
        public void Normalize()
        {
            var length = Length;
            X /= length; Y /= length;
        }
        public Vector2 Transform(Matrix matrix) => new Vector2((X * matrix.M11) + (Y * matrix.M21) + matrix.M41, (X * matrix.M12) + (Y * matrix.M22) + matrix.M42);
        public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;
        public override bool Equals(object obj) => obj is Vector2 vector && Equals(vector);
        public bool Equals(Vector2 other) => X == other.X && Y == other.Y;
        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.X, -a.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator /(Vector2 a, int l) => new Vector2(a.X / l, a.Y / l);
        public static Vector2 operator /(Vector2 a, float l) => new Vector2(a.X / l, a.Y / l);
        public static Vector2 operator *(Vector2 a, int l) => new Vector2(a.X * l, a.Y * l);
        public static Vector2 operator *(Vector2 a, float l) => new Vector2(a.X * l, a.Y * l);
        public static bool operator ==(Vector2 left, Vector2 right) => left.Equals(right);
        public static bool operator !=(Vector2 left, Vector2 right) => !(left == right);
        public float Length => (float)Math.Pow(LengthSquared, 0.5);
        public float LengthSquared => (float)(Math.Pow(X, 2) + Math.Pow(Y, 2));
        public static implicit operator Microsoft.Xna.Framework.Vector2(Vector2 vector)
        {
            return new Microsoft.Xna.Framework.Vector2(vector.X, vector.Y);
        }
        public static implicit operator Vector2(Microsoft.Xna.Framework.Vector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
    }
}
