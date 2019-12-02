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
        public static Vector2 operator *(Quaternion q, Vector2 v)
        {
            float x = 2 * (-q.Z * v.Y);
            float y = 2 * (q.Z * v.X);
            float z = 2 * (q.X * v.Y - q.Y * v.X);
            return new Vector2(
                v.X + x * q.W + (q.Y * z - q.Z * y),
                v.Y + y * q.W + (q.Z * x - q.X * z));
        }
        public Quaternion Inverse()
        {
            float invLengthSquared = 1f / (X * X + Y * Y + Z * Z + W * W);
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
                Y = Y * invLength,
                Z = Z * invLength,
                W = W * invLength,
            };
        }
        public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            float halfRoll = roll * 0.5f;
            float halfPitch = pitch * 0.5f;
            float halfYaw = yaw * 0.5f;

            float sinRoll = (float)Math.Sin(halfRoll);
            float cosRoll = (float)Math.Cos(halfRoll);
            float sinPitch = (float)Math.Sin(halfPitch);
            float cosPitch = (float)Math.Cos(halfPitch);
            float sinYaw = (float)Math.Sin(halfYaw);
            float cosYaw = (float)Math.Cos(halfYaw);

            return new Quaternion((cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll),
                                  (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll),
                                  (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll),
                                  (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll));
        }
        public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
        {
            float half = angle * 0.5f;
            float sin = (float)Math.Sin(half);
            float cos = (float)Math.Cos(half);
            return new Quaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);
        }
        public Matrix ToMatrix()
        {
            float x2 = X * X;
            float y2 = Y * Y;
            float z2 = Z * Z;
            float xy = X * Y;
            float zw = Z * W;
            float zx = Z * X;
            float yw = Y * W;
            float yz = Y * Z;
            float xw = X * W;
            return new Matrix
            {
                M11 = 1f - (2f * (y2 + z2)),
                M12 = 2f * (xy + zw),
                M13 = 2f * (zx - yw),
                M21 = 2f * (xy - zw),
                M22 = 1f - (2f * (z2 + x2)),
                M23 = 2f * (yz + xw),
                M31 = 2f * (zx + yw),
                M32 = 2f * (yz - xw),
                M33 = 1f - (2f * (y2 + x2)),
                M44 = 1f
            };
        }
        [Obsolete]
        public static Quaternion Concatenate(Quaternion a, Quaternion b) => a.Concat(b);
        public Quaternion Concat(Quaternion other)
        {
            float x2 = other.X;
            float y2 = other.Y;
            float z2 = other.Z;
            float w2 = other.W;
            return new Quaternion
            {
                X = ((x2 * W) + (X * w2)) + ((y2 * Z) - (z2 * Y)),
                Y = ((y2 * W) + (Y * w2)) + ((z2 * X) - (x2 * Z)),
                Z = ((z2 * W) + (Z * w2)) + ((x2 * Y) - (y2 * X)),
                W = (w2 * W) - (((x2 * X) + (y2 * Y)) + (z2 * Z))
            };
        }
    }
}
