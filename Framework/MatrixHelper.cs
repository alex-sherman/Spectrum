using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework
{
    public class MatrixHelper
    {
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
        static public Matrix CreateRotationFromDirection(Vector3 vDirection)
        {
            vDirection.Normalize();
            vDirection *= -1;
            // Step 1. Setup basis vectors describing the rotation given the input vector and assuming an initial up direction of (0, 1, 0)
            Vector3 vUp = new Vector3(0, 1.0f, 0.0f);           // Y Up vector
            Vector3 vRight = Vector3.Cross(vUp, vDirection);    // The perpendicular vector to Up and Direction
            vUp = Vector3.Cross(vDirection, vRight);            // The actual up vector given the direction and the right vector

            // Step 2. Put the three vectors into the matrix to bulid a basis rotation matrix
            // This step isnt necessary, but im adding it because often you would want to convert from matricies to quaternions instead of vectors to quaternions
            // If you want to skip this step, you can use the vector values directly in the quaternion setup below
            Matrix mBasis = new Matrix(vRight.X, vRight.Y, vRight.Z, 0.0f,
                                        vUp.X, vUp.Y, vUp.Z, 0.0f,
                                        vDirection.X, vDirection.Y, vDirection.Z, 0.0f,
                                        0.0f, 0.0f, 0.0f, 1.0f);
            return mBasis;
        }
    }
}
