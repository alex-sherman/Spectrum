using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public struct Matrix
    {
        public float M11 { get; set; }
        public float M12 { get; set; }
        public float M13 { get; set; }
        public float M14 { get; set; }
        public float M21 { get; set; }
        public float M22 { get; set; }
        public float M23 { get; set; }
        public float M24 { get; set; }
        public float M31 { get; set; }
        public float M32 { get; set; }
        public float M33 { get; set; }
        public float M34 { get; set; }
        public float M41 { get; set; }
        public float M42 { get; set; }
        public float M43 { get; set; }
        public float M44 { get; set; }
        public Matrix(
            float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44)
        {
            M11 = m11; M12 = m12; M13 = m13; M14 = m14;
            M21 = m21; M22 = m22; M23 = m23; M24 = m24;
            M31 = m31; M32 = m32; M33 = m33; M34 = m34;
            M41 = m41; M42 = m42; M43 = m43; M44 = m44;
        }

        public static Matrix Identity { get; } = new Matrix(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        public static Matrix CreatePerspective(float width, float height, float near, float far)
        {
            if (!(near < far && near > 0))
                throw new ArgumentException("0 <= near < far");
            return new Matrix
            {
                M11 = (2f * near) / width,
                M22 = (2f * near) / height,
                M33 = far / (near - far),
                M34 = -1f,
                M43 = (near * far) / (near - far)
            };
        }
        public static Matrix CreatePerspectiveFOV(float fieldOfView, float aspectRatio, float near, float far)
        {
            if (fieldOfView <= 0 || fieldOfView >= Math.PI)
                throw new ArgumentException("0 < fieldOfView < PI");
            if (!(near < far && near > 0))
                throw new ArgumentException("0 <= near < far");
            float tan = 1f / (float)Math.Tan(fieldOfView * 0.5);
            return new Matrix
            {
                M11 = tan / aspectRatio,
                M22 = tan,
                M33 = far / (near - far),
                M34 = -1,
                M43 = (near * far) / (near - far)
            };
        }
        public static Matrix CreateOrthographic(float width, float height, float near, float far)
        {
            return new Matrix
            {
                M11 = 2f / width,
                M22 = 2f / height,
                M33 = 1f / (near - far),
                M43 = near / (near - far),
                M44 = 1f
            };
        }
        public static implicit operator Microsoft.Xna.Framework.Matrix(Matrix m)
        {
            return new Microsoft.Xna.Framework.Matrix(
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44);
        }
        public Matrix Transpose()
        {
            return new Matrix(
                M11, M21, M31, M41,
                M12, M22, M32, M42,
                M13, M23, M33, M43,
                M14, M24, M34, M44);
        }
        public float Determinant()
        {
            float num18 = (M33 * M44) - (M34 * M43);
            float num17 = (M32 * M44) - (M34 * M42);
            float num16 = (M32 * M43) - (M33 * M42);
            float num15 = (M31 * M44) - (M34 * M41);
            float num14 = (M31 * M43) - (M33 * M41);
            float num13 = (M31 * M42) - (M32 * M41);
            return
                +(M11 * ((M22 * num18) - (M23 * num17) + (M24 * num16)))
                - (M12 * ((M21 * num18) - (M23 * num15) + (M24 * num14)))
                + (M13 * ((M21 * num17) - (M22 * num15) + (M24 * num13)))
                - (M14 * ((M21 * num16) - (M22 * num14) + (M23 * num13)));
        }
        public static Matrix operator *(Matrix a, Matrix b)
        {
            return new Matrix()
            {
                M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
                M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
                M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
                M14 = a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,
                M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
                M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
                M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
                M24 = a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,
                M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
                M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
                M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
                M34 = a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,
                M41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
                M42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
                M43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
                M44 = a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44
            };
        }
        public static Matrix operator *(Matrix a, float b)
        {
            return new Matrix()
            {
                M11 = a.M11 * b,
                M12 = a.M12 * b,
                M13 = a.M13 * b,
                M14 = a.M14 * b,
                M21 = a.M21 * b,
                M22 = a.M22 * b,
                M23 = a.M23 * b,
                M24 = a.M24 * b,
                M31 = a.M31 * b,
                M32 = a.M32 * b,
                M33 = a.M33 * b,
                M34 = a.M34 * b,
                M41 = a.M41 * b,
                M42 = a.M42 * b,
                M43 = a.M43 * b,
                M44 = a.M44 * b
            };
        }
        public static Matrix operator +(Matrix a, Matrix b)
        {
            return new Matrix()
            {
                M11 = a.M11 + b.M11,
                M12 = a.M12 + b.M12,
                M13 = a.M13 + b.M13,
                M14 = a.M14 + b.M14,
                M21 = a.M21 + b.M21,
                M22 = a.M22 + b.M22,
                M23 = a.M23 + b.M23,
                M24 = a.M24 + b.M24,
                M31 = a.M31 + b.M31,
                M32 = a.M32 + b.M32,
                M33 = a.M33 + b.M33,
                M34 = a.M34 + b.M34,
                M41 = a.M41 + b.M41,
                M42 = a.M42 + b.M42,
                M43 = a.M43 + b.M43,
                M44 = a.M44 + b.M44
            };
        }
        public static Matrix operator -(Matrix a, Matrix b)
        {
            return new Matrix()
            {
                M11 = a.M11 - b.M11,
                M12 = a.M12 - b.M12,
                M13 = a.M13 - b.M13,
                M14 = a.M14 - b.M14,
                M21 = a.M21 - b.M21,
                M22 = a.M22 - b.M22,
                M23 = a.M23 - b.M23,
                M24 = a.M24 - b.M24,
                M31 = a.M31 - b.M31,
                M32 = a.M32 - b.M32,
                M33 = a.M33 - b.M33,
                M34 = a.M34 - b.M34,
                M41 = a.M41 - b.M41,
                M42 = a.M42 - b.M42,
                M43 = a.M43 - b.M43,
                M44 = a.M44 - b.M44
            };
        }
        public static Vector3 operator *(Matrix m, Vector3 v)
        {
            return new Vector3(
                v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + m.M41,
                v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + m.M42,
                v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + m.M43);
        }
        public static Vector2 operator *(Matrix m, Vector2 v)
        {
            return new Vector2(
                v.X * m.M11 + v.Y * m.M21 + m.M41,
                v.X * m.M12 + v.Y * m.M22 + m.M42);
        }
        public Matrix NoTranslate()
        {
            return new Matrix()
            {
                M11 = M11,
                M12 = M12,
                M13 = M13,
                M21 = M21,
                M22 = M22,
                M23 = M23,
                M31 = M31,
                M32 = M32,
                M33 = M33
            };
        }
        public Matrix NoScale()
        {
            var a = new Vector3(M11, M12, M13).Normal();
            var b = new Vector3(M21, M22, M23).Normal();
            var c = new Vector3(M31, M32, M33).Normal();
            var output = this;
            output.M11 = a.X; output.M12 = a.Y; output.M13 = a.Z;
            output.M21 = b.X; output.M22 = b.Y; output.M23 = b.Z;
            output.M31 = c.X; output.M32 = c.Y; output.M33 = c.Z;
            return output;
        }
        public Matrix Invert()
        {
            Matrix m = this;
            Matrix inv = new Matrix();

            inv[0] = m[5] * m[10] * m[15] -
                     m[5] * m[11] * m[14] -
                     m[9] * m[6] * m[15] +
                     m[9] * m[7] * m[14] +
                     m[13] * m[6] * m[11] -
                     m[13] * m[7] * m[10];

            inv[4] = -m[4] * m[10] * m[15] +
                      m[4] * m[11] * m[14] +
                      m[8] * m[6] * m[15] -
                      m[8] * m[7] * m[14] -
                      m[12] * m[6] * m[11] +
                      m[12] * m[7] * m[10];

            inv[8] = m[4] * m[9] * m[15] -
                     m[4] * m[11] * m[13] -
                     m[8] * m[5] * m[15] +
                     m[8] * m[7] * m[13] +
                     m[12] * m[5] * m[11] -
                     m[12] * m[7] * m[9];

            inv[12] = -m[4] * m[9] * m[14] +
                       m[4] * m[10] * m[13] +
                       m[8] * m[5] * m[14] -
                       m[8] * m[6] * m[13] -
                       m[12] * m[5] * m[10] +
                       m[12] * m[6] * m[9];

            inv[1] = -m[1] * m[10] * m[15] +
                      m[1] * m[11] * m[14] +
                      m[9] * m[2] * m[15] -
                      m[9] * m[3] * m[14] -
                      m[13] * m[2] * m[11] +
                      m[13] * m[3] * m[10];

            inv[5] = m[0] * m[10] * m[15] -
                     m[0] * m[11] * m[14] -
                     m[8] * m[2] * m[15] +
                     m[8] * m[3] * m[14] +
                     m[12] * m[2] * m[11] -
                     m[12] * m[3] * m[10];

            inv[9] = -m[0] * m[9] * m[15] +
                      m[0] * m[11] * m[13] +
                      m[8] * m[1] * m[15] -
                      m[8] * m[3] * m[13] -
                      m[12] * m[1] * m[11] +
                      m[12] * m[3] * m[9];

            inv[13] = m[0] * m[9] * m[14] -
                      m[0] * m[10] * m[13] -
                      m[8] * m[1] * m[14] +
                      m[8] * m[2] * m[13] +
                      m[12] * m[1] * m[10] -
                      m[12] * m[2] * m[9];

            inv[2] = m[1] * m[6] * m[15] -
                     m[1] * m[7] * m[14] -
                     m[5] * m[2] * m[15] +
                     m[5] * m[3] * m[14] +
                     m[13] * m[2] * m[7] -
                     m[13] * m[3] * m[6];

            inv[6] = -m[0] * m[6] * m[15] +
                      m[0] * m[7] * m[14] +
                      m[4] * m[2] * m[15] -
                      m[4] * m[3] * m[14] -
                      m[12] * m[2] * m[7] +
                      m[12] * m[3] * m[6];

            inv[10] = m[0] * m[5] * m[15] -
                      m[0] * m[7] * m[13] -
                      m[4] * m[1] * m[15] +
                      m[4] * m[3] * m[13] +
                      m[12] * m[1] * m[7] -
                      m[12] * m[3] * m[5];

            inv[14] = -m[0] * m[5] * m[14] +
                       m[0] * m[6] * m[13] +
                       m[4] * m[1] * m[14] -
                       m[4] * m[2] * m[13] -
                       m[12] * m[1] * m[6] +
                       m[12] * m[2] * m[5];

            inv[3] = -m[1] * m[6] * m[11] +
                      m[1] * m[7] * m[10] +
                      m[5] * m[2] * m[11] -
                      m[5] * m[3] * m[10] -
                      m[9] * m[2] * m[7] +
                      m[9] * m[3] * m[6];

            inv[7] = m[0] * m[6] * m[11] -
                     m[0] * m[7] * m[10] -
                     m[4] * m[2] * m[11] +
                     m[4] * m[3] * m[10] +
                     m[8] * m[2] * m[7] -
                     m[8] * m[3] * m[6];

            inv[11] = -m[0] * m[5] * m[11] +
                       m[0] * m[7] * m[9] +
                       m[4] * m[1] * m[11] -
                       m[4] * m[3] * m[9] -
                       m[8] * m[1] * m[7] +
                       m[8] * m[3] * m[5];

            inv[15] = m[0] * m[5] * m[10] -
                      m[0] * m[6] * m[9] -
                      m[4] * m[1] * m[10] +
                      m[4] * m[2] * m[9] +
                      m[8] * m[1] * m[6] -
                      m[8] * m[2] * m[5];

            double det = m[0] * inv[0] + m[1] * inv[4] + m[2] * inv[8] + m[3] * inv[12];

            if (det == 0)
                return new Matrix();

            det = 1.0 / det;

            for (int i = 0; i < 16; i++)
                inv[i] = (float)(inv[i] * det);
            return inv;
        }
        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return M11;
                    case 1: return M12;
                    case 2: return M13;
                    case 3: return M14;
                    case 4: return M21;
                    case 5: return M22;
                    case 6: return M23;
                    case 7: return M24;
                    case 8: return M31;
                    case 9: return M32;
                    case 10: return M33;
                    case 11: return M34;
                    case 12: return M41;
                    case 13: return M42;
                    case 14: return M43;
                    case 15: return M44;
                }
                throw new IndexOutOfRangeException();
            }
            set
            {
                switch (i)
                {
                    case 0: M11 = value; break;
                    case 1: M12 = value; break;
                    case 2: M13 = value; break;
                    case 3: M14 = value; break;
                    case 4: M21 = value; break;
                    case 5: M22 = value; break;
                    case 6: M23 = value; break;
                    case 7: M24 = value; break;
                    case 8: M31 = value; break;
                    case 9: M32 = value; break;
                    case 10: M33 = value; break;
                    case 11: M34 = value; break;
                    case 12: M41 = value; break;
                    case 13: M42 = value; break;
                    case 14: M43 = value; break;
                    case 15: M44 = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }
        public float this[int row, int column]
        {
            get => this[row * 4 + column];
            set => this[row * 4 + column] = value;
        }
        public Vector3 Translation
        {
            get => new Vector3(M41, M42, M43);
            set { M41 = value.X; M42 = value.Y; M43 = value.Z; }
        }
        public Vector3 Forward
        {
            get => new Vector3(-M31, -M32, -M33);
            set { M41 = -value.X; M42 = -value.Y; M43 = -value.Z; }
        }
        public static Matrix CreateTranslation(float x, float y, float z) => CreateTranslation(new Vector3(x, y, z));
        public static Matrix CreateTranslation(Vector3 v)
        {
            var m = Identity;
            m.Translation = v;
            return m;
        }
        public static Matrix CreateScale(float x, float y, float z) => new Matrix() { M11 = x, M22 = y, M33 = z, M44 = 1 };
        public static Matrix CreateScale(Vector3 v) => new Matrix() { M11 = v.X, M22 = v.Y, M33 = v.Z, M44 = 1 };
        public static Matrix CreateScale(float s) => CreateScale(s, s, s);
        public static Matrix CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            var backward = (cameraPosition - cameraTarget).Normal();
            var right = cameraUpVector.Cross(backward).Normal();
            var up = backward.Cross(right);
            return new Matrix
            {
                M11 = right.X,
                M12 = up.X,
                M13 = backward.X,
                M21 = right.Y,
                M22 = up.Y,
                M23 = backward.Y,
                M31 = right.Z,
                M32 = up.Z,
                M33 = backward.Z,
                M41 = -right.Dot(cameraPosition),
                M42 = -up.Dot(cameraPosition),
                M43 = -backward.Dot(cameraPosition),
                M44 = 1f
            };
        }
        public static Matrix CreateFromYawPitchRoll(float yaw, float pitch, float roll) => Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll).ToMatrix();
        public static Matrix CreateRotationX(float radians)
        {
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);
            var result = Identity;
            result.M22 = cos;
            result.M23 = sin;
            result.M32 = -sin;
            result.M33 = cos;
            return result;
        }
        public static Matrix CreateRotationY(float radians)
        {
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);
            var result = Identity;
            result.M11 = cos;
            result.M13 = -sin;
            result.M31 = sin;
            result.M33 = cos;
            return result;
        }
        public static Matrix CreateRotationZ(float radians)
        {
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);
            var result = Identity;
            result.M11 = cos;
            result.M12 = sin;
            result.M21 = -sin;
            result.M22 = cos;
            return result;
        }
        public static Matrix CreateFromAxisAngle(Vector3 axis, float angle)
        {
            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;
            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);
            float x2 = x * x;
            float y2 = y * y;
            float z2 = z * z;
            float xy = x * y;
            float xz = x * z;
            float yz = y * z;
            return new Matrix
            {
                M11 = x2 + (cos * (1f - x2)),
                M12 = (xy - (cos * xy)) + (sin * z),
                M13 = (xz - (cos * xz)) - (sin * y),
                M14 = 0,
                M21 = (xy - (cos * xy)) - (sin * z),
                M22 = y2 + (cos * (1f - y2)),
                M23 = (yz - (cos * yz)) + (sin * x),
                M24 = 0,
                M31 = (xz - (cos * xz)) + (sin * y),
                M32 = (yz - (cos * yz)) - (sin * x),
                M33 = z2 + (cos * (1f - z2)),
                M34 = 0,
                M41 = 0,
                M42 = 0,
                M43 = 0,
                M44 = 1
            };
        }
        public static Matrix Create(Vector3? translation = null, Vector3? scale = null, Quaternion? rotation = null)
        {
            var output = rotation?.ToMatrix() ?? Identity;
            if (scale.HasValue)
            {
                output.M11 *= scale.Value.X;
                output.M21 *= scale.Value.X;
                output.M31 *= scale.Value.X;
                output.M12 *= scale.Value.Y;
                output.M22 *= scale.Value.Y;
                output.M32 *= scale.Value.Y;
                output.M13 *= scale.Value.Z;
                output.M23 *= scale.Value.Z;
                output.M33 *= scale.Value.Z;
            }
            if (translation.HasValue) output.Translation = translation.Value;
            return output;
        }

        public static Matrix CreateRotationFromDirection(Vector3 vDirection)
        {
            vDirection = -vDirection.Normal();
            Vector3 vUp = Vector3.Up;
            if (vDirection.X == 0 && vDirection.Z == 0)
                vUp = Vector3.Left;
            Vector3 vRight = vUp.Cross(vDirection).Normal();
            vUp = vDirection.Cross(vRight).Normal();
            Matrix mBasis = new Matrix(vRight.X, vRight.Y, vRight.Z, 0.0f,
                                        vUp.X, vUp.Y, vUp.Z, 0.0f,
                                        vDirection.X, vDirection.Y, vDirection.Z, 0.0f,
                                        0.0f, 0.0f, 0.0f, 1.0f);
            return mBasis;
        }

        public static Matrix CreateYawRotationFromDirection(Vector3 vDirection)
        {
            vDirection.Y = 0;
            return CreateRotationFromDirection(vDirection);
        }

        public Quaternion ToQuaternion()
        {
            Quaternion quaternion = new Quaternion();
            float sqrt;
            float half;
            float scale = M11 + M22 + M33;

            if (scale > 0.0f)
            {
                sqrt = (float)Math.Sqrt(scale + 1.0f);
                quaternion.W = sqrt * 0.5f;
                sqrt = 0.5f / sqrt;

                quaternion.X = (M23 - M32) * sqrt;
                quaternion.Y = (M31 - M13) * sqrt;
                quaternion.Z = (M12 - M21) * sqrt;

                return quaternion;
            }
            if ((M11 >= M22) && (M11 >= M33))
            {
                sqrt = (float)Math.Sqrt(1.0f + M11 - M22 - M33);
                half = 0.5f / sqrt;

                quaternion.X = 0.5f * sqrt;
                quaternion.Y = (M12 + M21) * half;
                quaternion.Z = (M13 + M31) * half;
                quaternion.W = (M23 - M32) * half;

                return quaternion;
            }
            if (M22 > M33)
            {
                sqrt = (float)Math.Sqrt(1.0f + M22 - M11 - M33);
                half = 0.5f / sqrt;

                quaternion.X = (M21 + M12) * half;
                quaternion.Y = 0.5f * sqrt;
                quaternion.Z = (M32 + M23) * half;
                quaternion.W = (M31 - M13) * half;

                return quaternion;
            }
            sqrt = (float)Math.Sqrt(1.0f + M33 - M11 - M22);
            half = 0.5f / sqrt;

            quaternion.X = (M31 + M13) * half;
            quaternion.Y = (M32 + M23) * half;
            quaternion.Z = 0.5f * sqrt;
            quaternion.W = (M12 - M21) * half;

            return quaternion.Normal();
        }
    }
}
