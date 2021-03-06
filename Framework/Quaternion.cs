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
        public Quaternion(Vector3 v, float w) { X = v.X; Y = v.Y; Z = v.Z; W = w; }
        public float Dot(Quaternion other) => X * other.X + Y * other.Y + Z * other.Z + W * other.W;
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
        // TODO: Should be able to skip going to matrix
        public static Quaternion CreateFromDirection(Vector3 direction)
        {
            return Matrix.CreateRotationFromDirection(direction).ToQuaternion();
        }
        /// <summary>
        /// Returns a quaternion representing the arc from a to b
        /// </summary>
        public static Quaternion CreateFromVectors(Vector3 a, Vector3 b)
        {
            var cross = a.Cross(b);
            return new Quaternion(cross, a.LengthSquared * b.LengthSquared + a.Dot(b))
                .Normal();
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
        public static Quaternion Slerp(Quaternion a, Quaternion b, float w)
        {
            float bWeight;
            float aWeight;
            Quaternion quaternion = new Quaternion();
            float dot = a.Dot(b);
            int dotSign = Math.Sign(dot);
            dot *= dotSign;
            if (dot > 0.999999f)
            {
                aWeight = 1f - w;
                bWeight = w * dotSign;
            }
            else
            {
                float num5 = (float)Math.Acos(dot);
                float num6 = (float)(1.0 / Math.Sin(num5));
                aWeight = (float)Math.Sin((1f - w) * num5) * num6;
                bWeight = dotSign * (float)Math.Sin(w * num5) * num6;
            }
            quaternion.X = (aWeight * a.X) + (bWeight * b.X);
            quaternion.Y = (aWeight * a.Y) + (bWeight * b.Y);
            quaternion.Z = (aWeight * a.Z) + (bWeight * b.Z);
            quaternion.W = (aWeight * a.W) + (bWeight * b.W);
            return quaternion;
        }
    }
}
