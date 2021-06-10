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
using System.Diagnostics;
using Replicate;
#endregion

namespace Spectrum.Framework.Physics.Collision.Shapes
{
    [ReplicateType]
    public class Float2DLerp
    {
        public float[,] HeightsA;
        public float[,] HeightsB;
        public float w = 0;
        public Float2DLerp() { }
        public Float2DLerp(float[,] heights)
        {
            HeightsA = heights;
            HeightsB = heights;
        }
        public float this[int x, int y]
        {
            get { return  HeightsA[x, y] * (1.0f - w) + HeightsB[x, y] * w; }
        }
    }
    /// <summary>
    /// Represents a terrain.
    /// </summary>
    [ReplicateType]
    public class TerrainShape : Multishape
    {
        public Float2DLerp Heights;
        private float scaleXZ;
        private int heightsLength0, heightsLength1;

        private int minX, maxX;
        private int minZ, maxZ;
        private int numX, numZ;

        private JBBox boundings;

        private float sphericalExpansion = 0.01f;
        private const float planarExpansion = 0.05f;

        /// <summary>
        /// Expands the triangles by the specified amount.
        /// This stabilizes collision detection for flat shapes.
        /// </summary>
        public float SphericalExpansion
        {
            get { return sphericalExpansion; }
            set { sphericalExpansion = value; }
        }

        /// <summary>
        /// Initializes a new instance of the TerrainShape class.
        /// </summary>
        /// <param name="heights">An array containing the heights of the terrain surface.</param>
        /// <param name="scaleX">The x-scale factor. (The x-space between neighbour heights)</param>
        /// <param name="scaleZ">The y-scale factor. (The y-space between neighbour heights)</param>
        public TerrainShape(float[,] heights, float scaleXZ)
        {
            heightsLength0 = heights.GetLength(0);
            heightsLength1 = heights.GetLength(1);



            this.Heights = new Float2DLerp(heights);
            this.scaleXZ = scaleXZ;

            UpdateShape();
        }

        internal TerrainShape() { }


        protected override Multishape CreateWorkingClone()
        {
            TerrainShape clone = new TerrainShape();
            clone.Heights = this.Heights;
            clone.scaleXZ = this.scaleXZ;
            clone.boundings = this.boundings;
            clone.heightsLength0 = this.heightsLength0;
            clone.heightsLength1 = this.heightsLength1;
            clone.sphericalExpansion = this.sphericalExpansion;
            return clone;
        }


        private Vector3[] points = new Vector3[3];
        private Vector3 normal = Vector3.Up;

        /// <summary>
        /// Sets the current shape. First <see cref="Prepare"/> has to be called.
        /// After SetCurrentShape the shape immitates another shape.
        /// </summary>
        /// <param name="index"></param>
        public override void SetCurrentShape(int index)
        {
            bool leftTriangle = false;

            if (index >= numX * numZ)
            {
                leftTriangle = true;
                index -= numX * numZ;
            }

            int quadIndexX = index % numX;
            int quadIndexZ = index / numX;

            // each quad has two triangles, called 'leftTriangle' and !'leftTriangle'
            if (leftTriangle)
            {
                points[0] = new Vector3((minX + quadIndexX + 0) * scaleXZ + boundings.Min.X, Heights[minX + quadIndexX + 0, minZ + quadIndexZ + 0], (minZ + quadIndexZ + 0) * scaleXZ + boundings.Min.Z);
                points[1] = new Vector3((minX + quadIndexX + 1) * scaleXZ + boundings.Min.X, Heights[minX + quadIndexX + 1, minZ + quadIndexZ + 0], (minZ + quadIndexZ + 0) * scaleXZ + boundings.Min.Z);
                points[2] = new Vector3((minX + quadIndexX + 0) * scaleXZ + boundings.Min.X, Heights[minX + quadIndexX + 0, minZ + quadIndexZ + 1], (minZ + quadIndexZ + 1) * scaleXZ + boundings.Min.Z);
            }
            else
            {
                points[0] = new Vector3((minX + quadIndexX + 1) * scaleXZ + boundings.Min.X, Heights[minX + quadIndexX + 1, minZ + quadIndexZ + 0], (minZ + quadIndexZ + 0) * scaleXZ + boundings.Min.Z);
                points[1] = new Vector3((minX + quadIndexX + 1) * scaleXZ + boundings.Min.X, Heights[minX + quadIndexX + 1, minZ + quadIndexZ + 1], (minZ + quadIndexZ + 1) * scaleXZ + boundings.Min.Z);
                points[2] = new Vector3((minX + quadIndexX + 0) * scaleXZ + boundings.Min.X, Heights[minX + quadIndexX + 0, minZ + quadIndexZ + 1], (minZ + quadIndexZ + 1) * scaleXZ + boundings.Min.Z);
            }

            geomCen = (points[0] + points[1] + points[2])/ 3;
            normal = (points[1] - points[0]).Cross(points[2] - points[0]);
        }

        public void CollisionNormal(out Vector3 normal)
        {
            normal = this.normal;
        }


        /// <summary>
        /// Passes a axis aligned bounding box to the shape where collision
        /// could occour.
        /// </summary>
        /// <param name="box">The bounding box where collision could occur.</param>
        /// <returns>The upper index with which <see cref="SetCurrentShape"/> can be 
        /// called.</returns>
        public override int Prepare(ref JBBox box)
        {
            if (boundings.Contains(box) == JBBox.ContainmentType.Disjoint)
                return 0;
            // simple idea: the terrain is a grid. x and z is the position in the grid.
            // y the height. we know compute the min and max grid-points. All quads
            // between these points have to be checked.

            // including overflow exception prevention

            if (box.Min.X < boundings.Min.X) minX = 0;
            else
            {
                minX = (int)Math.Floor((float)((box.Min.X - sphericalExpansion - boundings.Min.X) / scaleXZ));
                minX = Math.Max(minX, 0);
            }

            if (box.Max.X > boundings.Max.X) maxX = heightsLength0 - 1;
            else
            {
                maxX = (int)Math.Ceiling((float)((box.Max.X + sphericalExpansion - boundings.Min.X) / scaleXZ));
                maxX = Math.Min(maxX, heightsLength0 - 1);
            }

            if (box.Min.Z < boundings.Min.Z) minZ = 0;
            else
            {
                minZ = (int)Math.Floor((float)((box.Min.Z - sphericalExpansion - boundings.Min.Z) / scaleXZ));
                minZ = Math.Max(minZ, 0);
            }

            if (box.Max.Z > boundings.Max.Z) maxZ = heightsLength1 - 1;
            else
            {
                maxZ = (int)Math.Ceiling((float)((box.Max.Z + sphericalExpansion - boundings.Min.Z) / scaleXZ));
                maxZ = Math.Min(maxZ, heightsLength1 - 1);
            }

            numX = maxX - minX;
            numZ = maxZ - minZ;

            // since every quad contains two triangles we multiply by 2.
            return numX * numZ * 2;
        }
        public override void CalculateMassInertia()
        {
            this.inertia = Matrix.Identity;
            this.Mass = 1.0f;
        }
        public override void GetBoundingBox(ref Matrix orientation, out JBBox box)
        {
            box = boundings;

            #region Expand Spherical
            box.Min.X -= sphericalExpansion;
            box.Min.Y -= sphericalExpansion;
            box.Min.Z -= sphericalExpansion;
            box.Max.X += sphericalExpansion;
            box.Max.Y += sphericalExpansion;
            box.Max.Z += sphericalExpansion;
            #endregion

            box.Transform(ref orientation);
        }
        public override void MakeHull(ref List<Vector3> triangleList, int generationThreshold)
        {
            for (int index = 0; index < (heightsLength0 - 1) * (heightsLength1 - 1); index++)
            {
                int quadIndexX = index % (heightsLength0 - 1);
                int quadIndexZ = index / (heightsLength0 - 1);

                triangleList.Add(new Vector3((0 + quadIndexX + 0) * scaleXZ, Heights[0 + quadIndexX + 0, 0 + quadIndexZ + 0], (0 + quadIndexZ + 0) * scaleXZ));
                triangleList.Add(new Vector3((0 + quadIndexX + 1) * scaleXZ, Heights[0 + quadIndexX + 1, 0 + quadIndexZ + 0], (0 + quadIndexZ + 0) * scaleXZ));
                triangleList.Add(new Vector3((0 + quadIndexX + 0) * scaleXZ, Heights[0 + quadIndexX + 0, 0 + quadIndexZ + 1], (0 + quadIndexZ + 1) * scaleXZ));

                triangleList.Add(new Vector3((0 + quadIndexX + 1) * scaleXZ, Heights[0 + quadIndexX + 1, 0 + quadIndexZ + 0], (0 + quadIndexZ + 0) * scaleXZ));
                triangleList.Add(new Vector3((0 + quadIndexX + 1) * scaleXZ, Heights[0 + quadIndexX + 1, 0 + quadIndexZ + 1], (0 + quadIndexZ + 1) * scaleXZ));
                triangleList.Add(new Vector3((0 + quadIndexX + 0) * scaleXZ, Heights[0 + quadIndexX + 0, 0 + quadIndexZ + 1], (0 + quadIndexZ + 1) * scaleXZ));
            }
        }
        public override void SupportMapping(ref Vector3 direction, out Vector3 result)
        {
            throw new InvalidOperationException("This shouldn't be able to be called! Overriding the other support mapping should prevent it.");
        }
        public override void SupportMapping(ref Vector3 direction, out Vector3 result, bool retrievingInformation)
        {
            Vector3 expandVector = direction.Normal() * sphericalExpansion;

            int minIndex = 0;
            float min = points[0].Dot(direction);
            float dot = points[1].Dot(direction);
            if (dot > min)
            {
                min = dot;
                minIndex = 1;
            }
            dot = points[2].Dot(direction);
            if (dot > min)
            {
                min = dot;
                minIndex = 2;
            }
            result = points[minIndex] + expandVector;

            if(retrievingInformation)
            {
                Vector3 sectionNormal = (points[0] - points[1]).Cross(points[0] - points[2]).Normal();
                dot = direction.Dot(sectionNormal);
                //This is necessary to avoid floating point rounding errors when the direction
                //and normal are in almost identical directions
                if (Math.Abs(dot) < (0.9f))
                {
                    Vector3 normalD = sectionNormal * dot;
                    Vector3 planarExpansionDirection = (direction - normalD).Normal();
                    planarExpansionDirection *= scaleXZ * planarExpansion;
                    result += planarExpansionDirection;
                }
            }

        }

        public override int Prepare(ref Vector3 rayOrigin, ref Vector3 rayDelta)
        {
            JBBox box = JBBox.SmallBox;

            var rayEnd = rayOrigin + rayDelta + rayDelta.Normal() * sphericalExpansion;

            box.AddPoint(ref rayOrigin);
            box.AddPoint(ref rayEnd);

            return Prepare(ref box);
        }

        public override void UpdateShape()
        {
            #region Bounding Box
            boundings = JBBox.SmallBox;

            for (int i = 0; i < heightsLength0; i++)
            {
                for (int e = 0; e < heightsLength1; e++)
                {
                    if (Heights.HeightsA[i, e] > boundings.Max.Y)
                        boundings.Max.Y = Heights.HeightsA[i, e];
                    if (Heights.HeightsB[i, e] > boundings.Max.Y)
                        boundings.Max.Y = Heights.HeightsB[i, e];
                    if (Heights.HeightsA[i, e] < boundings.Min.Y)
                        boundings.Min.Y = Heights.HeightsA[i, e];
                    if (Heights.HeightsB[i, e] < boundings.Min.Y)
                        boundings.Min.Y = Heights.HeightsB[i, e];
                }
            }

            boundings.Min.X = -sphericalExpansion;
            boundings.Min.Y -= sphericalExpansion;
            boundings.Min.Z = -sphericalExpansion;

            boundings.Max.X = checked(heightsLength0 * scaleXZ) + sphericalExpansion;
            boundings.Max.Y += sphericalExpansion;
            boundings.Max.Z = checked(heightsLength1 * scaleXZ) + sphericalExpansion;

            #endregion

            base.UpdateShape();
        }

        public override void SetScale(float scale)
        {
            scaleXZ = scale;
            UpdateShape();
        }
    }
}
