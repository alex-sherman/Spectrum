using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework
{
    // TODO: Probably remove this
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
            return new Quaternion(
                (float)jobj[0],
                (float)jobj[1],
                (float)jobj[2],
                (float)jobj[3]).ToMatrix();
        }
    }
}
