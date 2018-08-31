using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework
{
    public static class MatrixHelper
    {
        public static float[] ToArray(this Matrix matrix)
        {
            return new float[]
            {
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44,
            };
        }
        public static Matrix FromArray(float[] array)
        {
            return new Matrix(
                array[0], array[1], array[2], array[3],
                array[4], array[5], array[6], array[7],
                array[8], array[9], array[10], array[11],
                array[12], array[13], array[14], array[15]
            );
        }
        public static Matrix YUpToZUp()
        {
            var output = Matrix.Identity;
            output[1, 1] = 0;
            output[1, 2] = 1;
            output[2, 2] = 0;
            output[2, 1] = -1;
            return output;
        }
        public static Matrix CreateTranslation(JToken jobj)
        {
            return Matrix.CreateTranslation(
                        (float)jobj[0],
                        (float)jobj[1],
                        (float)jobj[2]);
        }
        public static Matrix CreateRotation(JToken jobj)
        {
            return Matrix.CreateFromQuaternion(
                    new Quaternion(
                        (float)jobj[0],
                        (float)jobj[1],
                        (float)jobj[2],
                        (float)jobj[3]));
        }
        public static Matrix RotationFromDirection(Vector3 vDirection)
        {
            vDirection.Normalize();
            vDirection *= -1;
            Vector3 vUp = Vector3.Up;
            if (vDirection.X == 0 && vDirection.Z == 0)
                vUp = Vector3.Left;
            Vector3 vRight = Vector3.Cross(vUp, vDirection);
            vRight.Normalize();
            vUp = Vector3.Cross(vDirection, vRight);
            vUp.Normalize();
            Matrix mBasis = new Matrix(vRight.X, vRight.Y, vRight.Z, 0.0f,
                                        vUp.X, vUp.Y, vUp.Z, 0.0f,
                                        vDirection.X, vDirection.Y, vDirection.Z, 0.0f,
                                        0.0f, 0.0f, 0.0f, 1.0f);
            return mBasis;
        }
        public static Quaternion QuaternionFromDirection(Vector3 vDirection)
        {
            return RotationFromDirection(vDirection).ToQuaternion();
        }
        public static Quaternion ToQuaternion(this Matrix matrix)
        {
            var r0 = new Vector3(matrix.M11, matrix.M12, matrix.M13); r0.Normalize();
            var r1 = new Vector3(matrix.M21, matrix.M22, matrix.M23); r1.Normalize();
            var r2 = new Vector3(matrix.M31, matrix.M32, matrix.M33); r2.Normalize();
            return Quaternion.CreateFromRotationMatrix(new Matrix(new Vector4(r0, 0), new Vector4(r1, 0), new Vector4(r2, 0), new Vector4(0, 0, 0, 1)));
        }
        public static Matrix ToMatrix(this Quaternion quaternion)
        {
            return Matrix.CreateFromQuaternion(quaternion);
        }
        public static Quaternion Concat(this Quaternion quata, Quaternion quatb)
        {
            return Quaternion.Concatenate(quata, quatb);
        }
    }
}
