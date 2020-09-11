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
using Spectrum.Framework;
using Spectrum.Framework.Physics.Dynamics;
using Spectrum.Framework.Physics.LinearMath;
using Spectrum.Framework.Physics.Collision.Shapes;
using Microsoft.Xna.Framework;
#endregion

namespace Spectrum.Framework.Physics.Collision
{
    public struct GJKResult
    {
        public Vector3 Position;
        public Vector3 ExtrudedPosition;
    }


    /// <summary>
    /// GJK based implementation of Raycasting.
    /// </summary>
    public sealed class GJKCollide
    {
        public static float Timestep = 1 / 60f;
        private const int MaxIterations = 20;

        private static ResourcePool<VoronoiSimplexSolver> simplexSolverPool = new ResourcePool<VoronoiSimplexSolver>();

        public static void SupportMapTransformed(ISupportMappable support, ref Quaternion orientation, ref Vector3 position, ref Vector3 velocity, ref Vector3 direction, out GJKResult result)
        {
            //Vector3.Transform(ref direction, ref invOrientation, out result);
            //support.SupportMapping(ref result, out result);
            //Vector3.Transform(ref result, ref orientation, out result);
            //Vector3.Add(ref result, ref position, out result);
            Vector3 newDirection = orientation.Inverse() * direction;

            support.SupportMapping(ref newDirection, out result.Position, false);
            result.Position = orientation * result.Position;
            result.Position += position;
            result.ExtrudedPosition = orientation * result.Position;
            result.ExtrudedPosition += position;
            if (Vector3.Dot(velocity, direction) > 0)
            {
                result.ExtrudedPosition += velocity * Timestep;
            }
        }
        public static void SupportMapTransformed(ISupportMappable support, ref Matrix orientation, ref Vector3 position, ref Vector3 direction, out Vector3 result)
        {
            //Vector3.Transform(ref direction, ref invOrientation, out result);
            //support.SupportMapping(ref result, out result);
            //Vector3.Transform(ref result, ref orientation, out result);
            //Vector3.Add(ref result, ref position, out result);
            Vector3 newDirection = direction;
            newDirection.X = ((direction.X * orientation.M11) + (direction.Y * orientation.M12)) + (direction.Z * orientation.M13);
            newDirection.Y = ((direction.X * orientation.M21) + (direction.Y * orientation.M22)) + (direction.Z * orientation.M23);
            newDirection.Z = ((direction.X * orientation.M31) + (direction.Y * orientation.M32)) + (direction.Z * orientation.M33);

            support.SupportMapping(ref newDirection, out result, false);
            result = orientation * result;
            result += position;
        }

        public static bool ClosestPoints(ISupportMappable support1, ISupportMappable support2, Matrix orientation1,
            Matrix orientation2, Vector3 position1, Vector3 position2,
            out Vector3 p1, out Vector3 p2, out Vector3 normal)
        {

            VoronoiSimplexSolver simplexSolver = simplexSolverPool.GetNew();
            simplexSolver.Reset();
            p1 = p2 = Vector3.Zero;

            Vector3 r = position1 - position2;
            Vector3 w, v;

            Vector3 supVertexA;
            Vector3 rn, vn;

            rn = -r;

            SupportMapTransformed(support1, ref orientation1, ref position1, ref rn, out supVertexA);

            Vector3 supVertexB;
            SupportMapTransformed(support2, ref orientation2, ref position2, ref r, out supVertexB);

            v = supVertexA - supVertexB;

            normal = Vector3.Zero;

            int maxIter = 15;

            float distSq = v.LengthSquared;
            float epsilon = 0.000001f;

            while ((distSq > epsilon) && (maxIter-- != 0))
            {
                vn = -v;
                SupportMapTransformed(support1, ref orientation1, ref position1, ref vn, out supVertexA);
                SupportMapTransformed(support2, ref orientation2, ref position2, ref v, out supVertexB);
                w = supVertexA - supVertexB;

                if (!simplexSolver.InSimplex(w)) simplexSolver.AddVertex(w, supVertexA, supVertexB);
                if (simplexSolver.Closest(out v))
                {
                    distSq = v.LengthSquared;
                    normal = v;
                }
                else distSq = 0.0f;
            }


            simplexSolver.ComputePoints(out p1, out p2);

            if (normal.LengthSquared > JMath.Epsilon * JMath.Epsilon)
                normal.Normalize();

            simplexSolverPool.GiveBack(simplexSolver);

            return true;

        }


        #region GJK Detect
        /// <summary>
        /// Checks two shapes for collisions.
        /// </summary>
        /// <param name="support1">The SupportMappable implementation of the first shape to test.</param>
        /// <param name="support2">The SupportMappable implementation of the seconds shape to test.</param>
        /// <param name="orientation1">The orientation of the first shape.</param>
        /// <param name="orientation2">The orientation of the second shape.</param>
        /// <param name="position1">The position of the first shape.</param>
        /// <param name="position2">The position of the second shape</param>
        /// <param name="point">The pointin world coordinates, where collision occur.</param>
        /// <param name="normal">The normal pointing from body2 to body1.</param>
        /// <param name="penetration">Estimated penetration depth of the collision.</param>
        /// <returns>Returns true if there is a collision, false otherwise.</returns>
        public static bool Detect(ISupportMappable support1, ISupportMappable support2,
            Quaternion orientation1, Quaternion orientation2,
            Vector3 position1, Vector3 position2,
            Vector3 velocity1, Vector3 velocity2,
            out List<EPAVertex> simplex, bool speculative = false)
        {
            simplex = new List<EPAVertex>();
            GJKResult g1, g2;
            Vector3 s, s1, s2;
            Vector3 direction = Vector3.One;
            Vector3 negativeDirection = -direction;
            //Get an initial point on the Minkowski difference.
            SupportMapTransformed(support1, ref orientation1, ref position1, ref velocity1, ref negativeDirection, out g1);
            SupportMapTransformed(support2, ref orientation2, ref position2, ref velocity2, ref direction, out g2);
            s1 = speculative ? g1.ExtrudedPosition : g1.Position;
            s2 = speculative ? g2.ExtrudedPosition : g2.Position;
            s = s2 - s1;

            simplex.Add(new EPAVertex(s, s1, s2));

            //Choose an initial direction toward the origin.
            direction = -s;

            //Choose a maximim number of iterations to avoid an 
            //infinite loop during a non-convergent search.
            int maxIterations = 50;

            for (int i = 0; i < maxIterations; i++)
            {
                //Get our next simplex point toward the origin.
                if (direction.LengthSquared > 0)
                    direction.Normalize();
                negativeDirection = -direction;
                SupportMapTransformed(support1, ref orientation1, ref position1, ref velocity1, ref negativeDirection, out g1);
                SupportMapTransformed(support2, ref orientation2, ref position2, ref velocity2, ref direction, out g2);
                s1 = speculative ? g1.ExtrudedPosition : g1.Position;
                s2 = speculative ? g2.ExtrudedPosition : g2.Position;
                Vector3 a = s2 - s1;

                simplex.Add(new EPAVertex(a, s1, s2));

                //If we move toward the origin and didn't pass it 
                //then we never will and there's no intersection.
                if (a.IsInOppositeDirection(direction))
                {
                    return false;
                }

                //Here we either find a collision or we find the closest feature of
                //the simplex to the origin, make that the new simplex and update the direction
                //to move toward the origin from that feature.
                if (ProcessSimplex(simplex, ref direction))
                {
                    return true;
                }
            }
            //If we still couldn't find a simplex 
            //that contains the origin then we
            //"probably" have an intersection.
            return false;
        }

        /// <summary>
        ///Either finds a collision or the closest feature of the simplex to the origin, 
        ///and updates the simplex and direction.
        /// </summary>
        static bool ProcessSimplex(List<EPAVertex> simplex, ref Vector3 direction)
        {
            if (simplex.Count == 2)
            {
                return ProcessLine(simplex, ref direction);
            }
            else if (simplex.Count == 3)
            {
                return ProcessTriangle(simplex, ref direction);
            }
            else
            {
                return ProcessTetrehedron(simplex, ref direction);
            }
        }

        static bool ProcessLine(List<EPAVertex> simplex, ref Vector3 direction)
        {
            Vector3 a = simplex[1].Position;
            Vector3 b = simplex[0].Position;
            Vector3 ab = b - a;
            Vector3 aO = -a;

            direction = ab.Cross(aO).Cross(ab);
            return false;
        }

        static bool ProcessTriangle(List<EPAVertex> simplex, ref Vector3 direction)
        {
            Vector3 a = simplex[2].Position;
            Vector3 b = simplex[1].Position;
            Vector3 c = simplex[0].Position;
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 abc = ab.Cross(ac);
            Vector3 aO = -a;

            if (!abc.IsInSameDirection(aO))
            {
                abc = -abc;
            }
            else
            {
                simplex.Reverse();
            }
            direction = abc;
            return false;
        }

        static bool ProcessTetrehedron(List<EPAVertex> simplex, ref Vector3 direction)
        {
            Vector3 a = simplex[3].Position;
            Vector3 b = simplex[2].Position;
            Vector3 c = simplex[1].Position;
            Vector3 d = simplex[0].Position;
            Vector3 ac = c - a;
            Vector3 ad = d - a;
            Vector3 ab = b - a;

            Vector3 adc = ad.Cross(ac);
            Vector3 abd = ab.Cross(ad);
            Vector3 abc = ac.Cross(ab);
            //No need to check BCD since A was the last added point, it certainly must be in
            //the direction of the origin from BCD
            Vector3 aO = -a;
            if (abc.IsInSameDirection(aO))
                simplex.RemoveAt(0);
            else if (adc.IsInSameDirection(aO))
                simplex.RemoveAt(2);
            else if (abd.IsInSameDirection(aO))
                simplex.RemoveAt(1);
            else
                return true;
            // The remaining triangle needs to be correctly wound such that 1-0 x 2-0 points to the origin
            return ProcessTriangle(simplex, ref direction);
        }
        #endregion

        #region GJK Raycast
        /// <summary>
        /// Checks if a ray definied through it's origin and direction collides
        /// with a shape.
        /// </summary>
        /// <param name="support">The supportmap implementation representing the shape.</param>
        /// <param name="orientation">The orientation of the shape.</param>
        /// <param name="invOrientation">The inverse orientation of the shape.</param>
        /// <param name="position">The position of the shape.</param>
        /// <param name="origin">The origin of the ray.</param>
        /// <param name="direction">The direction of the ray.</param>
        /// <param name="fraction">The fraction which gives information where at the 
        /// ray the collision occured. The hitPoint is calculated by: origin+friction*direction.</param>
        /// <param name="normal">The normal from the ray collision.</param>
        /// <returns>Returns true if the ray hit the shape, false otherwise.</returns>
        public static bool Raycast(ISupportMappable support, Quaternion orientation, Quaternion invOrientation,
            Vector3 position, Vector3 velocity, Vector3 origin, Vector3 direction, out float fraction, out Vector3 normal)
        {
            VoronoiSimplexSolver simplexSolver = simplexSolverPool.GetNew();
            simplexSolver.Reset();

            normal = Vector3.Zero;
            fraction = float.MaxValue;

            float lambda = 0.0f;

            Vector3 r = direction;
            Vector3 x = origin;
            Vector3 w, p, v;

            Vector3 arbitraryPoint;
            GJKResult result;
            SupportMapTransformed(support, ref orientation, ref position, ref velocity, ref r, out result);
            arbitraryPoint = result.Position;
            v = x - arbitraryPoint;

            int maxIter = MaxIterations;

            float distSq = v.LengthSquared;
            float epsilon = 0.000001f;

            float VdotR;

            while ((distSq > epsilon) && (maxIter-- != 0))
            {
                SupportMapTransformed(support, ref orientation, ref position, ref velocity, ref v, out result);
                p = result.Position;
                w = x - p;

                float VdotW = Vector3.Dot(v, w);

                if (VdotW > 0.0f)
                {
                    VdotR = Vector3.Dot(v, r);

                    if (VdotR >= -JMath.Epsilon)
                    {
                        simplexSolverPool.GiveBack(simplexSolver);
                        return false;
                    }
                    else
                    {
                        lambda = lambda - VdotW / VdotR;
                        x = r * lambda;
                        x += origin;
                        w = x - p;
                    }
                }
                // Importantly, the simplexSolver should be computing Closest(conv({x} - Y))
                // where in this case Y is the set of points in simplexSolver.PointsQ. Previously
                // this was storing historicaly versions of x and computing Closest(conv({x1, x2, x3...}, Y)).
                for (int i = 0; i < simplexSolver.NumVertices; i++)
                {
                    simplexSolver.PointsP[i] = x;
                    simplexSolver.PointsW[i] = x - simplexSolver.PointsQ[i];
                    simplexSolver.NeedsUpdate = true;
                }
                if (!simplexSolver.InSimplex(w))
                    simplexSolver.AddVertex(w, x, p);
                Vector3 oldV = v;
                // If we get the same point out twice in a row the iteration is stuck!
                // Some paper suggests this is due to affine dependency or something and says to just return success
                if (simplexSolver.Closest(out v) && oldV != v)
                    distSq = v.LengthSquared;
                else
                    distSq = 0.0f;
            }
            if (maxIter <= 0 && distSq > epsilon)
                return false;

            #region Retrieving hitPoint

            // Giving back the fraction like this *should* work
            // but is inaccurate against large objects:
            // fraction = lambda;

            simplexSolver.ComputePoints(out Vector3 p1, out Vector3 p2);

            p1 -= origin;
            p2 -= origin;
            fraction = Math.Min(p1.Length, p2.Length) / direction.Length;

            #endregion
            normal = v;
            //if (!(normal.LengthSquared() > JMath.Epsilon * JMath.Epsilon))
            {
                if (simplexSolver.NumVertices == 3)
                {
                    normal = (simplexSolver.PointsW[1] - simplexSolver.PointsW[0]).Cross(simplexSolver.PointsW[2] - simplexSolver.PointsW[0]);
                    if (Vector3.Dot(normal, direction) > 0)
                        normal *= -1;
                }
            }
            normal.Normalize();

            simplexSolverPool.GiveBack(simplexSolver);

            return true;
        }
        #endregion

        // see: btVoronoiSimplexSolver.cpp
        #region private class VoronoiSimplexSolver - Bullet

        // Bullet has problems with raycasting large objects - so does jitter
        // hope to fix that in the next versions.

        /*
          Bullet for XNA Copyright (c) 2003-2007 Vsevolod Klementjev http://www.codeplex.com/xnadevru
          Bullet original C++ version Copyright (c) 2003-2007 Erwin Coumans http://bulletphysics.com

          This software is provided 'as-is', without any express or implied
          warranty.  In no event will the authors be held liable for any damages
          arising from the use of this software.

          Permission is granted to anyone to use this software for any purpose,
          including commercial applications, and to alter it and redistribute it
          freely, subject to the following restrictions:

          1. The origin of this software must not be misrepresented; you must not
             claim that you wrote the original software. If you use this software
             in a product, an acknowledgment in the product documentation would be
             appreciated but is not required.
          2. Altered source versions must be plainly marked as such, and must not be
             misrepresented as being the original software.
          3. This notice may not be removed or altered from any source distribution.
        */

        private class UsageBitfield
        {
            private bool _usedVertexA, _usedVertexB, _usedVertexC, _usedVertexD;

            public bool UsedVertexA { get { return _usedVertexA; } set { _usedVertexA = value; } }
            public bool UsedVertexB { get { return _usedVertexB; } set { _usedVertexB = value; } }
            public bool UsedVertexC { get { return _usedVertexC; } set { _usedVertexC = value; } }
            public bool UsedVertexD { get { return _usedVertexD; } set { _usedVertexD = value; } }

            public void Reset()
            {
                _usedVertexA = _usedVertexB = _usedVertexC = _usedVertexD = false;
            }
        }

        private class SubSimplexClosestResult
        {
            public Vector3 ClosestPointOnSimplex;

            //MASK for m_usedVertices
            //stores the simplex vertex-usage, using the MASK, 
            // if m_usedVertices & MASK then the related vertex is used
            public UsageBitfield UsedVertices = new UsageBitfield();
            public float[] BarycentricCoords = new float[4];

            public void Reset()
            {
                SetBarycentricCoordinates();
                UsedVertices.Reset();
            }

            public bool IsValid
            {
                get
                {
                    return (BarycentricCoords[0] >= 0f) &&
                            (BarycentricCoords[1] >= 0f) &&
                            (BarycentricCoords[2] >= 0f) &&
                            (BarycentricCoords[3] >= 0f);
                }
            }

            public void SetBarycentricCoordinates()
            {
                SetBarycentricCoordinates(0f, 0f, 0f, 0f);
            }

            public void SetBarycentricCoordinates(float a, float b, float c, float d)
            {
                BarycentricCoords[0] = a;
                BarycentricCoords[1] = b;
                BarycentricCoords[2] = c;
                BarycentricCoords[3] = d;
            }
        }

        /// VoronoiSimplexSolver is an implementation of the closest point distance
        /// algorithm from a 1-4 points simplex to the origin.
        /// Can be used with GJK, as an alternative to Johnson distance algorithm. 
        private class VoronoiSimplexSolver
        {
            private const int VertexA = 0, VertexB = 1, VertexC = 2, VertexD = 3;

            private const int VoronoiSimplexMaxVerts = 4;


            public Vector3[] PointsW = new Vector3[VoronoiSimplexMaxVerts];
            public Vector3[] PointsP = new Vector3[VoronoiSimplexMaxVerts];
            public Vector3[] PointsQ = new Vector3[VoronoiSimplexMaxVerts];

            private Vector3 _cachedPA;
            private Vector3 _cachedPB;
            private Vector3 _cachedV;
            private Vector3 _lastW;
            private bool _cachedValidClosest;

            private SubSimplexClosestResult _cachedBC = new SubSimplexClosestResult();

            // Note that this assumes ray-casts and point-casts will always be called from the
            // same thread which I assume is true from the _cachedBC member
            // If this needs to made multi-threaded a resource pool will be needed
            private SubSimplexClosestResult tempResult = new SubSimplexClosestResult();

            public bool NeedsUpdate;

            #region ISimplexSolver Members

            public bool FullSimplex => NumVertices == VoronoiSimplexMaxVerts;

            public int NumVertices { get; private set; }

            public void Reset()
            {
                _cachedValidClosest = false;
                NumVertices = 0;
                NeedsUpdate = true;
                _lastW = new Vector3(1e30f, 1e30f, 1e30f);
                _cachedBC.Reset();
            }

            public void AddVertex(Vector3 w, Vector3 p, Vector3 q)
            {
                _lastW = w;
                NeedsUpdate = true;

                PointsW[NumVertices] = w;
                PointsP[NumVertices] = p;
                PointsQ[NumVertices] = q;

                NumVertices++;
            }

            //return/calculate the closest vertex
            public bool Closest(out Vector3 v)
            {
                bool succes = UpdateClosestVectorAndPoints();
                v = _cachedV;
                return succes;
            }

            public float MaxVertex
            {
                get
                {
                    int numverts = NumVertices;
                    float maxV = 0f, curLen2;
                    for (int i = 0; i < numverts; i++)
                    {
                        curLen2 = PointsW[i].LengthSquared;
                        if (maxV < curLen2) maxV = curLen2;
                    }
                    return maxV;
                }
            }

            //return the current simplex
            public int GetSimplex(out Vector3[] pBuf, out Vector3[] qBuf, out Vector3[] yBuf)
            {
                int numverts = NumVertices;
                pBuf = new Vector3[numverts];
                qBuf = new Vector3[numverts];
                yBuf = new Vector3[numverts];
                for (int i = 0; i < numverts; i++)
                {
                    yBuf[i] = PointsW[i];
                    pBuf[i] = PointsP[i];
                    qBuf[i] = PointsQ[i];
                }
                return numverts;
            }

            public bool InSimplex(Vector3 w)
            {
                //check in case lastW is already removed
                if (w == _lastW) return true;

                //w is in the current (reduced) simplex
                int numverts = NumVertices;
                for (int i = 0; i < numverts; i++)
                    if (PointsW[i] == w) return true;

                return false;
            }

            public void BackupClosest(out Vector3 v)
            {
                v = _cachedV;
            }

            public bool EmptySimplex
            {
                get
                {
                    return NumVertices == 0;
                }
            }

            public void ComputePoints(out Vector3 p1, out Vector3 p2)
            {
                UpdateClosestVectorAndPoints();
                p1 = _cachedPA;
                p2 = _cachedPB;
            }

            #endregion

            public void RemoveVertex(int index)
            {
                NumVertices--;
                PointsW[index] = PointsW[NumVertices];
                PointsP[index] = PointsP[NumVertices];
                PointsQ[index] = PointsQ[NumVertices];
                PointsW[NumVertices] = PointsW[NumVertices] = PointsW[NumVertices] = Vector3.Zero;
            }

            public void ReduceVertices(UsageBitfield usedVerts)
            {
                if ((NumVertices >= 4) && (!usedVerts.UsedVertexD)) RemoveVertex(3);
                if ((NumVertices >= 3) && (!usedVerts.UsedVertexC)) RemoveVertex(2);
                if ((NumVertices >= 2) && (!usedVerts.UsedVertexB)) RemoveVertex(1);
                if ((NumVertices >= 1) && (!usedVerts.UsedVertexA)) RemoveVertex(0);
            }

            public bool UpdateClosestVectorAndPoints()
            {
                if (NeedsUpdate || true)
                {
                    _cachedBC.Reset();
                    NeedsUpdate = false;

                    Vector3 p, a, b, c, d;
                    switch (NumVertices)
                    {
                        case 0:
                            _cachedValidClosest = false;
                            break;
                        case 1:
                            _cachedPA = PointsP[0];
                            _cachedPB = PointsQ[0];
                            _cachedV = _cachedPA - _cachedPB;
                            _cachedBC.Reset();
                            _cachedBC.SetBarycentricCoordinates(1f, 0f, 0f, 0f);
                            _cachedValidClosest = _cachedBC.IsValid;
                            break;
                        case 2:
                            //closest point origin from line segment
                            Vector3 from = PointsW[0];
                            Vector3 to = PointsW[1];
                            Vector3 nearest;

                            Vector3 diff = from * (-1);
                            Vector3 v = to - from;
                            float t = Vector3.Dot(v, diff);

                            if (t > 0)
                            {
                                float dotVV = v.LengthSquared;
                                if (t < dotVV)
                                {
                                    t /= dotVV;
                                    diff -= t * v;
                                    _cachedBC.UsedVertices.UsedVertexA = true;
                                    _cachedBC.UsedVertices.UsedVertexB = true;
                                }
                                else
                                {
                                    t = 1;
                                    diff -= v;
                                    //reduce to 1 point
                                    _cachedBC.UsedVertices.UsedVertexB = true;
                                }
                            }
                            else
                            {
                                t = 0;
                                //reduce to 1 point
                                _cachedBC.UsedVertices.UsedVertexA = true;
                            }

                            _cachedBC.SetBarycentricCoordinates(1 - t, t, 0, 0);
                            nearest = from + t * v;

                            _cachedPA = PointsP[0] + t * (PointsP[1] - PointsP[0]);
                            _cachedPB = PointsQ[0] + t * (PointsQ[1] - PointsQ[0]);
                            _cachedV = _cachedPA - _cachedPB;

                            ReduceVertices(_cachedBC.UsedVertices);

                            _cachedValidClosest = _cachedBC.IsValid;
                            break;
                        case 3:
                            //closest point origin from triangle
                            p = new Vector3();
                            a = PointsW[0];
                            b = PointsW[1];
                            c = PointsW[2];

                            ClosestPtPointTriangle(p, a, b, c, ref _cachedBC);
                            _cachedPA = PointsP[0] * _cachedBC.BarycentricCoords[0] +
                                            PointsP[1] * _cachedBC.BarycentricCoords[1] +
                                            PointsP[2] * _cachedBC.BarycentricCoords[2] +
                                            PointsP[3] * _cachedBC.BarycentricCoords[3];

                            _cachedPB = PointsQ[0] * _cachedBC.BarycentricCoords[0] +
                                            PointsQ[1] * _cachedBC.BarycentricCoords[1] +
                                            PointsQ[2] * _cachedBC.BarycentricCoords[2] +
                                            PointsQ[3] * _cachedBC.BarycentricCoords[3];

                            _cachedV = _cachedPA - _cachedPB;

                            ReduceVertices(_cachedBC.UsedVertices);
                            _cachedValidClosest = _cachedBC.IsValid;
                            break;
                        case 4:
                            p = new Vector3();
                            a = PointsW[0];
                            b = PointsW[1];
                            c = PointsW[2];
                            d = PointsW[3];

                            bool hasSeperation = ClosestPtPointTetrahedron(p, a, b, c, d, ref _cachedBC);

                            if (hasSeperation)
                            {
                                _cachedPA = PointsP[0] * _cachedBC.BarycentricCoords[0] +
                                                PointsP[1] * _cachedBC.BarycentricCoords[1] +
                                                PointsP[2] * _cachedBC.BarycentricCoords[2] +
                                                PointsP[3] * _cachedBC.BarycentricCoords[3];

                                _cachedPB = PointsQ[0] * _cachedBC.BarycentricCoords[0] +
                                                PointsQ[1] * _cachedBC.BarycentricCoords[1] +
                                                PointsQ[2] * _cachedBC.BarycentricCoords[2] +
                                                PointsQ[3] * _cachedBC.BarycentricCoords[3];

                                _cachedV = _cachedPA - _cachedPB;
                                ReduceVertices(_cachedBC.UsedVertices);
                            }
                            else
                            {
                                _cachedValidClosest = true;
                                //degenerate case == false, penetration = true + zero
                                _cachedV.X = _cachedV.Y = _cachedV.Z = 0f;
                                break; // !!!!!!!!!!!! proverit na vsakiy sluchai
                            }

                            _cachedValidClosest = _cachedBC.IsValid;

                            //closest point origin from tetrahedron
                            break;
                        default:
                            _cachedValidClosest = false;
                            break;
                    }
                }

                return _cachedValidClosest;
            }

            public bool ClosestPtPointTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c,
                ref SubSimplexClosestResult result)
            {
                result.UsedVertices.Reset();

                float v, w;

                // Check if P in vertex region outside A
                Vector3 ab = b - a;
                Vector3 ac = c - a;
                Vector3 ap = p - a;
                float d1 = Vector3.Dot(ab, ap);
                float d2 = Vector3.Dot(ac, ap);
                if (d1 <= 0f && d2 <= 0f)
                {
                    result.ClosestPointOnSimplex = a;
                    result.UsedVertices.UsedVertexA = true;
                    result.SetBarycentricCoordinates(1, 0, 0, 0);
                    return true; // a; // barycentric coordinates (1,0,0)
                }

                // Check if P in vertex region outside B
                Vector3 bp = p - b;
                float d3 = Vector3.Dot(ab, bp);
                float d4 = Vector3.Dot(ac, bp);
                if (d3 >= 0f && d4 <= d3)
                {
                    result.ClosestPointOnSimplex = b;
                    result.UsedVertices.UsedVertexB = true;
                    result.SetBarycentricCoordinates(0, 1, 0, 0);

                    return true; // b; // barycentric coordinates (0,1,0)
                }
                // Check if P in edge region of AB, if so return projection of P onto AB
                float vc = d1 * d4 - d3 * d2;
                if (vc <= 0f && d1 >= 0f && d3 <= 0f)
                {
                    v = d1 / (d1 - d3);
                    result.ClosestPointOnSimplex = a + v * ab;
                    result.UsedVertices.UsedVertexA = true;
                    result.UsedVertices.UsedVertexB = true;
                    result.SetBarycentricCoordinates(1 - v, v, 0, 0);
                    return true;
                    //return a + v * ab; // barycentric coordinates (1-v,v,0)
                }

                // Check if P in vertex region outside C
                Vector3 cp = p - c;
                float d5 = Vector3.Dot(ab, cp);
                float d6 = Vector3.Dot(ac, cp);
                if (d6 >= 0f && d5 <= d6)
                {
                    result.ClosestPointOnSimplex = c;
                    result.UsedVertices.UsedVertexC = true;
                    result.SetBarycentricCoordinates(0, 0, 1, 0);
                    return true;//c; // barycentric coordinates (0,0,1)
                }

                // Check if P in edge region of AC, if so return projection of P onto AC
                float vb = d5 * d2 - d1 * d6;
                if (vb <= 0f && d2 >= 0f && d6 <= 0f)
                {
                    w = d2 / (d2 - d6);
                    result.ClosestPointOnSimplex = a + w * ac;
                    result.UsedVertices.UsedVertexA = true;
                    result.UsedVertices.UsedVertexC = true;
                    result.SetBarycentricCoordinates(1 - w, 0, w, 0);
                    return true;
                    //return a + w * ac; // barycentric coordinates (1-w,0,w)
                }

                // Check if P in edge region of BC, if so return projection of P onto BC
                float va = d3 * d6 - d5 * d4;
                if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
                {
                    w = (d4 - d3) / ((d4 - d3) + (d5 - d6));

                    result.ClosestPointOnSimplex = b + w * (c - b);
                    result.UsedVertices.UsedVertexB = true;
                    result.UsedVertices.UsedVertexC = true;
                    result.SetBarycentricCoordinates(0, 1 - w, w, 0);
                    return true;
                    // return b + w * (c - b); // barycentric coordinates (0,1-w,w)
                }

                // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
                float denom = 1.0f / (va + vb + vc);
                v = vb * denom;
                w = vc * denom;

                result.ClosestPointOnSimplex = a + ab * v + ac * w;
                result.UsedVertices.UsedVertexA = true;
                result.UsedVertices.UsedVertexB = true;
                result.UsedVertices.UsedVertexC = true;
                result.SetBarycentricCoordinates(1 - v - w, v, w, 0);

                return true;
            }

            /// Test if point p and d lie on opposite sides of plane through abc
            public bool PointOutsideOfPlane(Vector3 p, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                Vector3 normal = (b - a).Cross(c - a);

                float signp = Vector3.Dot(p - a, normal); // [AP AB AC]
                float signd = Vector3.Dot(d - a, normal); // [AD AB AC]

                // Points on opposite sides if expression signs are opposite
                return signp * signd < 0f;
            }

            public bool ClosestPtPointTetrahedron(Vector3 p, Vector3 a, Vector3 b, Vector3 c, Vector3 d,
                ref SubSimplexClosestResult finalResult)
            {
                tempResult.Reset();

                // Start out assuming point inside all halfspaces, so closest to itself
                finalResult.ClosestPointOnSimplex = p;
                finalResult.UsedVertices.Reset();
                finalResult.UsedVertices.UsedVertexA = true;
                finalResult.UsedVertices.UsedVertexB = true;
                finalResult.UsedVertices.UsedVertexC = true;
                finalResult.UsedVertices.UsedVertexD = true;

                bool pointOutsideABC = PointOutsideOfPlane(p, a, b, c, d);
                bool pointOutsideACD = PointOutsideOfPlane(p, a, c, d, b);
                bool pointOutsideADB = PointOutsideOfPlane(p, a, d, b, c);
                bool pointOutsideBDC = PointOutsideOfPlane(p, b, d, c, a);

                bool isDegenerate = !pointOutsideABC && !pointOutsideACD && !pointOutsideADB && !pointOutsideBDC;

                float bestSqDist = float.MaxValue;
                // If point outside face abc then compute closest point on abc
                if (isDegenerate || pointOutsideABC)
                {
                    ClosestPtPointTriangle(p, a, b, c, ref tempResult);
                    Vector3 q = tempResult.ClosestPointOnSimplex;

                    float sqDist = ((Vector3)(q - p)).LengthSquared;
                    // Update best closest point if (squared) distance is less than current best
                    if (sqDist < bestSqDist)
                    {
                        bestSqDist = sqDist;
                        finalResult.ClosestPointOnSimplex = q;
                        //convert result bitmask!
                        finalResult.UsedVertices.Reset();
                        finalResult.UsedVertices.UsedVertexA = tempResult.UsedVertices.UsedVertexA;
                        finalResult.UsedVertices.UsedVertexB = tempResult.UsedVertices.UsedVertexB;
                        finalResult.UsedVertices.UsedVertexC = tempResult.UsedVertices.UsedVertexC;
                        finalResult.SetBarycentricCoordinates(
                                tempResult.BarycentricCoords[VertexA],
                                tempResult.BarycentricCoords[VertexB],
                                tempResult.BarycentricCoords[VertexC],
                                0);
                    }
                }

                // Repeat test for face acd
                if (isDegenerate || pointOutsideACD)
                {
                    ClosestPtPointTriangle(p, a, c, d, ref tempResult);
                    Vector3 q = tempResult.ClosestPointOnSimplex;
                    //convert result bitmask!

                    float sqDist = ((Vector3)(q - p)).LengthSquared;
                    if (sqDist < bestSqDist)
                    {
                        bestSqDist = sqDist;
                        finalResult.ClosestPointOnSimplex = q;
                        finalResult.UsedVertices.Reset();
                        finalResult.UsedVertices.UsedVertexA = tempResult.UsedVertices.UsedVertexA;
                        finalResult.UsedVertices.UsedVertexC = tempResult.UsedVertices.UsedVertexB;
                        finalResult.UsedVertices.UsedVertexD = tempResult.UsedVertices.UsedVertexC;
                        finalResult.SetBarycentricCoordinates(
                                tempResult.BarycentricCoords[VertexA],
                                0,
                                tempResult.BarycentricCoords[VertexB],
                                tempResult.BarycentricCoords[VertexC]);
                    }
                }
                // Repeat test for face adb

                if (isDegenerate || pointOutsideADB)
                {
                    ClosestPtPointTriangle(p, a, d, b, ref tempResult);
                    Vector3 q = tempResult.ClosestPointOnSimplex;
                    //convert result bitmask!

                    float sqDist = ((Vector3)(q - p)).LengthSquared;
                    if (sqDist < bestSqDist)
                    {
                        bestSqDist = sqDist;
                        finalResult.ClosestPointOnSimplex = q;
                        finalResult.UsedVertices.Reset();
                        finalResult.UsedVertices.UsedVertexA = tempResult.UsedVertices.UsedVertexA;
                        finalResult.UsedVertices.UsedVertexD = tempResult.UsedVertices.UsedVertexB;
                        finalResult.UsedVertices.UsedVertexB = tempResult.UsedVertices.UsedVertexC;
                        finalResult.SetBarycentricCoordinates(
                                tempResult.BarycentricCoords[VertexA],
                                tempResult.BarycentricCoords[VertexC],
                                0,
                                tempResult.BarycentricCoords[VertexB]);

                    }
                }
                // Repeat test for face bdc

                if (isDegenerate || pointOutsideBDC)
                {
                    ClosestPtPointTriangle(p, b, d, c, ref tempResult);
                    Vector3 q = tempResult.ClosestPointOnSimplex;
                    //convert result bitmask!
                    float sqDist = ((Vector3)(q - p)).LengthSquared;
                    if (sqDist < bestSqDist)
                    {
                        bestSqDist = sqDist;
                        finalResult.ClosestPointOnSimplex = q;
                        finalResult.UsedVertices.Reset();
                        finalResult.UsedVertices.UsedVertexB = tempResult.UsedVertices.UsedVertexA;
                        finalResult.UsedVertices.UsedVertexD = tempResult.UsedVertices.UsedVertexB;
                        finalResult.UsedVertices.UsedVertexC = tempResult.UsedVertices.UsedVertexC;

                        finalResult.SetBarycentricCoordinates(
                                0,
                                tempResult.BarycentricCoords[VertexA],
                                tempResult.BarycentricCoords[VertexC],
                                tempResult.BarycentricCoords[VertexB]);
                    }
                }

                return true;
            }
        }

        #endregion

    }
}
