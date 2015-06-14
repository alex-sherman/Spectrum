using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework
{
    public static class ExtensionMethods
    {
        public static bool IsInSameDirection(this Vector3 vector, Vector3 otherVector)
        {
            return Vector3.Dot(vector, otherVector) > 0;
        }

        public static bool IsInOppositeDirection(this Vector3 vector, Vector3 otherVector)
        {
            return Vector3.Dot(vector, otherVector) < 0;
        }
    }
}
