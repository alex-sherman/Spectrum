using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public static class VectorExtensions
    {
        public static string FixedLenString(this Vector3 vector)
        {
            return "<" + vector.X.ToString("0.00") + ", " + vector.Y.ToString("0.00") + ", " + vector.Z.ToString("0.00") + ">";
        }
        public static Vector3 Homogeneous(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z) / vector.W;
        }
        public static bool IsInSameDirection(this Vector3 vector, Vector3 otherVector)
        {
            return vector.Dot(otherVector) > 0;
        }
        public static bool IsInOppositeDirection(this Vector3 vector, Vector3 otherVector)
        {
            return vector.Dot(otherVector) < 0;
        }
        public static Vector3 Project(this Vector3 source, Vector3 normal)
        {
            return source.Dot(normal) * normal;
        }
        public static Vector3 ProjectUnto(this Vector3 source, Vector3 planeNormal)
        {
            return source - source.Project(planeNormal);
        }
        public static float Roll(this Quaternion quaternion)
        {
            // yaw (z-axis rotation)
            double siny = +2.0 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
            double cosy = +1.0 - 2.0 * (quaternion.X * quaternion.X + quaternion.Z * quaternion.Z);
            return (float)Math.Atan2(siny, cosy);
        }
        public static float Yaw(this Quaternion quaternion)
        {
            // roll (y-axis rotation)
            double sinr = +2.0 * (quaternion.W * quaternion.Y + quaternion.X * quaternion.Z);
            double cosr = +1.0 - 2.0 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
            return (float)Math.Atan2(sinr, cosr);
            //// pitch (y-axis rotation)
            //double sinp = +2.0 * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
            //if (Math.Abs(sinp) >= 1)
            //    return (float)(Math.Sign(sinp) * Math.PI / 2); // use 90 degrees if out of range
            //return (float)Math.Asin(sinp);
        }
        public static float Pitch(this Quaternion quaternion)
        {
            // pitch (x-axis rotation)
            double sinp = +2.0 * (quaternion.W * quaternion.X - quaternion.Z * quaternion.Y);
            if (Math.Abs(sinp) >= 1)
                return (float)(Math.Sign(sinp) * Math.PI / 2); // use 90 degrees if out of range
            return (float)Math.Asin(sinp);
        }
    }
}
