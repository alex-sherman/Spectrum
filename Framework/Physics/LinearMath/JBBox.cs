/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
* 
*  This software is provided 'as-is', without any express or implied
*  warranty.  In no event will the authors be held liable for any damages
*  arising from the use of this software.
*
*  Permission is granted to anyone to use this software for any purpose,
*  including commercial applications, and to alter it and redistribute it
*  freely, subject to the following restrictions:
*
*  1. The origin of this software must not be misrepresented; you must not
*      claim that you wrote the original software. If you use this software
*      in a product, an acknowledgment in the product documentation would be
*      appreciated but is not required.
*  2. Altered source versions must be plainly marked as such, and must not be
*      misrepresented as being the original software.
*  3. This notice may not be removed or altered from any source distribution. 
*/

#region Using Statements
using System;
using System.Collections.Generic;

using Spectrum.Framework.Physics.Dynamics;
using Spectrum.Framework.Physics.LinearMath;
using Spectrum.Framework.Physics.Collision.Shapes;
using Microsoft.Xna.Framework;
using ProtoBuf;
#endregion

namespace Spectrum.Framework.Physics.LinearMath
{
    /// <summary>
    /// Bounding Box defined through min and max vectors. Member
    /// of the math namespace, so every method has it's 'by reference'
    /// equivalent to speed up time critical math operations.
    /// </summary>
    [ProtoContract]
    public struct JBBox
    {
        /// <summary>
        /// Containment type used within the <see cref="JBBox"/> structure.
        /// </summary>
        public enum ContainmentType
        {
            /// <summary>
            /// The objects don't intersect.
            /// </summary>
            Disjoint,
            /// <summary>
            /// One object is within the other.
            /// </summary>
            Contains,
            /// <summary>
            /// The two objects intersect.
            /// </summary>
            Intersects
        }

        /// <summary>
        /// The maximum point of the box.
        /// </summary>
        [ProtoMember(1)]
        public Vector3 Min;

        /// <summary>
        /// The minimum point of the box.
        /// </summary>
        [ProtoMember(2)]
        public Vector3 Max;

        /// <summary>
        /// Returns the largest box possible.
        /// </summary>
        public static readonly JBBox LargeBox;

        /// <summary>
        /// Returns the smalltest box possible.
        /// </summary>
        public static readonly JBBox SmallBox;

        static JBBox()
        {
            LargeBox.Min = Vector3.One * float.MinValue;
            LargeBox.Max = Vector3.One * float.MaxValue;
            SmallBox.Min = Vector3.One * float.MaxValue;
            SmallBox.Max = Vector3.One * float.MinValue;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="min">The minimum point of the box.</param>
        /// <param name="max">The maximum point of the box.</param>
        public JBBox(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;
        }

        /// <summary>
        /// Transforms the bounding box into the space given by orientation and position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="orientation"></param>
        /// <param name="result"></param>
        internal void InverseTransform(Vector3 position, Matrix orientation)
        {
            Max -= position;
            Min -= position;

            Vector3 center = (Max + Min) / 2;

            Vector3 halfExtents = (Max - Min) / 2;
            center = orientation.Transpose() * center;

            Matrix abs; JMath.Absolute(ref orientation, out abs);
            halfExtents = abs.Transpose() * halfExtents;
            Max = center + halfExtents;
            Min = center + halfExtents;
        }

        public void Transform(ref Matrix orientation)
        {
            Vector3 halfExtents = 0.5f * (Max - Min);
            Vector3 center = orientation * (0.5f * (Max + Min));

            Matrix abs; JMath.Absolute(ref orientation, out abs);
            halfExtents = abs * halfExtents;

            Max = center + halfExtents;
            Min = center - halfExtents;
        }

        /// <summary>
        /// Checks whether a point is inside, outside or intersecting
        /// a point.
        /// </summary>
        /// <returns>The ContainmentType of the point.</returns>
        #region public Ray/Segment Intersection

        private bool Intersect1D(float start, float dir, float min, float max,
            ref float enter,ref float exit)
        {
            if (dir * dir < JMath.Epsilon * JMath.Epsilon) return (start >= min && start <= max);

            float t0 = (min - start) / dir;
            float t1 = (max - start) / dir;

            if (t0 > t1) { float tmp = t0; t0 = t1; t1 = tmp; }

            if (t0 > exit || t1 < enter) return false;

            if (t0 > enter) enter = t0;
            if (t1 < exit) exit = t1;
            return true;
        }


        public bool SegmentIntersect(ref Vector3 origin,ref Vector3 direction)
        {
            float enter = 0.0f, exit = 1.0f;

            if (!Intersect1D(origin.X, direction.X, Min.X, Max.X,ref enter,ref exit))
                return false;

            if (!Intersect1D(origin.Y, direction.Y, Min.Y, Max.Y, ref enter, ref exit))
                return false;

            if (!Intersect1D(origin.Z, direction.Z, Min.Z, Max.Z,ref enter,ref exit))
                return false;

            return true;
        }

        public bool RayIntersect(ref Vector3 origin, ref Vector3 direction)
        {
            float enter = 0.0f, exit = float.MaxValue;

            if (!Intersect1D(origin.X, direction.X, Min.X, Max.X, ref enter, ref exit))
                return false;

            if (!Intersect1D(origin.Y, direction.Y, Min.Y, Max.Y, ref enter, ref exit))
                return false;

            if (!Intersect1D(origin.Z, direction.Z, Min.Z, Max.Z, ref enter, ref exit))
                return false;

            return true;
        }

        public bool SegmentIntersect(Vector3 origin, Vector3 direction)
        {
            return SegmentIntersect(ref origin, ref direction);
        }

        public bool RayIntersect(Vector3 origin, Vector3 direction)
        {
            return RayIntersect(ref origin, ref direction);
        }

        /// <summary>
        /// Checks wether a point is within a box or not.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public ContainmentType Contains(Vector3 point)
        {
            return this.Contains(ref point);
        }

        /// <summary>
        /// Checks whether a point is inside, outside or intersecting
        /// a point.
        /// </summary>
        /// <param name="point">A point in space.</param>
        /// <returns>The ContainmentType of the point.</returns>
        public ContainmentType Contains(ref Vector3 point)
        {
            return ((((this.Min.X <= point.X) && (point.X <= this.Max.X)) &&
                ((this.Min.Y <= point.Y) && (point.Y <= this.Max.Y))) &&
                ((this.Min.Z <= point.Z) && (point.Z <= this.Max.Z))) ? ContainmentType.Contains : ContainmentType.Disjoint;
        }

        #endregion

        /// <summary>
        /// Retrieves the 8 corners of the box.
        /// </summary>
        /// <returns>An array of 8 Vector3 entries.</returns>
        #region public void GetCorners(Vector3[] corners)

        public void GetCorners(Vector3[] corners)
        {
            corners[0] = new Vector3(this.Min.X, this.Max.Y, this.Max.Z);
            corners[1] = new Vector3(this.Max.X, this.Max.Y, this.Max.Z);
            corners[2] = new Vector3(this.Max.X, this.Min.Y, this.Max.Z);
            corners[3] = new Vector3(this.Min.X, this.Min.Y, this.Max.Z);
            corners[4] = new Vector3(this.Min.X, this.Max.Y, this.Min.Z);
            corners[5] = new Vector3(this.Max.X, this.Max.Y, this.Min.Z);
            corners[6] = new Vector3(this.Max.X, this.Min.Y, this.Min.Z);
            corners[7] = new Vector3(this.Min.X, this.Min.Y, this.Min.Z);
        }

        #endregion


        public void AddPoint(Vector3 point)
        {
            AddPoint(ref point);
        }

        public void AddPoint(ref Vector3 point)
        {
            Max = Vector3.Max(point, Max);
            Min = Vector3.Min(point, Min);
        }

        /// <summary>
        /// Expands a bounding box with the volume 0 by all points
        /// given.
        /// </summary>
        /// <param name="points">A array of Vector3.</param>
        /// <returns>The resulting bounding box containing all points.</returns>
        #region public static JBBox CreateFromPoints(Vector3[] points)

        public static JBBox CreateFromPoints(Vector3[] points)
        {
            Vector3 vector3 = Vector3.One * float.MaxValue;
            Vector3 vector2 = Vector3.One * float.MinValue;

            for (int i = 0; i < points.Length; i++)
            {
                vector3 = Vector3.Min(points[i], vector3);
                vector2 = Vector3.Max(points[i], vector2);
            }
            return new JBBox(vector3, vector2);
        }

        #endregion

        /// <summary>
        /// Checks whether another bounding box is inside, outside or intersecting
        /// this box. 
        /// </summary>
        /// <param name="box">The other bounding box to check.</param>
        /// <returns>The ContainmentType of the box.</returns>
        #region public ContainmentType Contains(JBBox box)

        public ContainmentType Contains(JBBox box)
        {
            return this.Contains(ref box);
        }

        /// <summary>
        /// Checks whether another bounding box is inside, outside or intersecting
        /// this box. 
        /// </summary>
        /// <param name="box">The other bounding box to check.</param>
        /// <returns>The ContainmentType of the box.</returns>
        public ContainmentType Contains(ref JBBox box)
        {
            ContainmentType result = ContainmentType.Disjoint;
            if ((((this.Max.X >= box.Min.X) && (this.Min.X <= box.Max.X)) && ((this.Max.Y >= box.Min.Y) && (this.Min.Y <= box.Max.Y))) && ((this.Max.Z >= box.Min.Z) && (this.Min.Z <= box.Max.Z)))
            {
                result = ((((this.Min.X <= box.Min.X) && (box.Max.X <= this.Max.X)) && ((this.Min.Y <= box.Min.Y) && (box.Max.Y <= this.Max.Y))) && ((this.Min.Z <= box.Min.Z) && (box.Max.Z <= this.Max.Z))) ? ContainmentType.Contains : ContainmentType.Intersects;
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Creates a new box containing the two given ones.
        /// </summary>
        /// <param name="original">First box.</param>
        /// <param name="additional">Second box.</param>
        /// <returns>A JBBox containing the two given boxes.</returns>
        #region public static JBBox CreateMerged(JBBox original, JBBox additional)

        public static JBBox CreateMerged(JBBox original, JBBox additional)
        {
            JBBox result;
            JBBox.CreateMerged(ref original, ref additional, out result);
            return result;
        }

        /// <summary>
        /// Creates a new box containing the two given ones.
        /// </summary>
        /// <param name="original">First box.</param>
        /// <param name="additional">Second box.</param>
        /// <param name="result">A JBBox containing the two given boxes.</param>
        public static void CreateMerged(ref JBBox original, ref JBBox additional, out JBBox result)
        {
            result.Min = Vector3.Min(original.Min, additional.Min);
            result.Max = Vector3.Max(original.Max, additional.Max);
        }

        #endregion

        public Vector3 Center { get { return (Min + Max)* (1.0f /2.0f); } }

        internal float Perimeter
        {
            get
            {
                return 2.0f * ((Max.X - Min.X) * (Max.Y - Min.Y) +
                    (Max.X - Min.X) * (Max.Z - Min.Z) +
                    (Max.Z - Min.Z) * (Max.Y - Min.Y));
            }
        }
    }
}
