using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public struct Vector3 : IEquatable<Vector3>
    {
        public static Vector3 Zero => new Vector3();
        public static Vector3 UnitX => new Vector3() { X = 1 };
        public static Vector3 UnitY => new Vector3() { Y = 1 };
        public static Vector3 UnitZ => new Vector3() { Z = 1 };
        public static Vector3 Left => new Vector3() { X = -1 };
        public static Vector3 Right => new Vector3() { X = 1 };
        public static Vector3 Up => new Vector3() { Y = 1 };
        public static Vector3 Down => new Vector3() { Y = -1 };
        public static Vector3 Forward => new Vector3() { Z = -1 };
        public static Vector3 Backward => new Vector3() { Z = 1 };
        public static Vector3 One => new Vector3(1);
        public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
        public Vector3(float s) { X = s; Y = s; Z = s; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        [Obsolete]
        public void Normalize()
        {
            this /= Length;
        }
        public Vector3 Normal() => this / Length;
        [Obsolete]
        public static Vector3 Normalize(Vector3 v) => v.Normal();
        [Obsolete]
        public static float Dot(Vector3 a, Vector3 b) => a.Dot(b);
        public float Dot(Vector3 b) => X * b.X + Y * b.Y + Z * b.Z;
        public Vector3 Cross(Vector3 b) => new Vector3(
                Y * b.Z - b.Y * Z,
                -(X * b.Z - b.X * Z),
                X * b.Y - b.X * Y);
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.X, -a.Y, -a.Z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3 operator /(Vector3 a, int d) => new Vector3(a.X / d, a.Y / d, a.Z / d);
        public static Vector3 operator /(Vector3 a, float d) => new Vector3(a.X / d, a.Y / d, a.Z / d);
        public static Vector3 operator *(Vector3 a, int d) => new Vector3(a.X * d, a.Y * d, a.Z * d);
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.X * d, a.Y * d, a.Z * d);
        public static Vector3 operator *(int d, Vector3 a) => new Vector3(a.X * d, a.Y * d, a.Z * d);
        public static Vector3 operator *(float d, Vector3 a) => new Vector3(a.X * d, a.Y * d, a.Z * d);
        [Obsolete]
        public static Vector3 Transform(Vector3 a, Matrix b) => b * a;
        [Obsolete]
        public static Vector3 Transform(Vector3 a, Quaternion b) => b * a;
        #region Equality
        public override bool Equals(object obj)
        {
            return obj is Vector3 vector && Equals(vector);
        }
        public bool Equals(Vector3 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z;
        }
        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !(left == right);
        }
        public override int GetHashCode()
        {
            var hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }
        #endregion
        /// <summary>Returns the element-wise minimum of two vectors</summary>
        public static Vector3 Min(Vector3 a, Vector3 b)
        {
            return new Vector3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
        }
        /// <summary>Returns the element-wise maximum of two vectors</summary>
        public static Vector3 Max(Vector3 a, Vector3 b)
        {
            return new Vector3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        }
        public static Vector3 Lerp(Vector3 a, Vector3 b, float w) => a * (1 - w) + b * w;
        public float Length => (float)Math.Pow(LengthSquared, 0.5);
        public float LengthSquared => (float)(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
        public static implicit operator Microsoft.Xna.Framework.Vector3(Vector3 vector) => new Microsoft.Xna.Framework.Vector3(vector.X, vector.Y, vector.Z);
        public static implicit operator Vector3(Microsoft.Xna.Framework.Vector3 vector) => new Vector3(vector.X, vector.Y, vector.Z);
    }
}
