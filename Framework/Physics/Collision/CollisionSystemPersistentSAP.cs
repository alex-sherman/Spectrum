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
using System.Collections;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Entities;
#endregion

namespace Spectrum.Framework.Physics.Collision
{
    /// <summary>
    /// Full 3-Axis SweepAndPrune using persistent updates.
    /// </summary>
    public class CollisionSystemPersistentSAP : CollisionSystem
    {
        private const int AddedObjectsBruteForceIsUsed = 250;

        #region private class SweepPoint
        //private class SweepPoint
        //{
        //    private static float GetXA(GameObject Body) => Body.boundingBox.Min.X;
        //    private static float GetXB(GameObject Body) => Body.boundingBox.Max.X;
        //    private static float GetYA(GameObject Body) => Body.boundingBox.Min.Y;
        //    private static float GetYB(GameObject Body) => Body.boundingBox.Max.Y;
        //    private static float GetZA(GameObject Body) => Body.boundingBox.Min.Z;
        //    private static float GetZB(GameObject Body) => Body.boundingBox.Max.Z;
        //    public GameObject Body;
        //    public bool Begin;
        //    public int Axis;
        //    private Func<GameObject, float> GetFunc;

        //    public SweepPoint(GameObject body, bool begin, int axis)
        //    {
        //        Body = body;
        //        Begin = begin;
        //        Axis = axis;
        //        if (Begin)
        //        {
        //            if (Axis == 0) GetFunc = GetXA;
        //            else if (Axis == 1) GetFunc = GetYA;
        //            else GetFunc = GetZA;
        //        }
        //        else
        //        {
        //            if (Axis == 0) GetFunc = GetXB;
        //            else if (Axis == 1) GetFunc = GetYB;
        //            else GetFunc = GetZB;
        //        }
        //    }

        //    public float Value => GetFunc(Body);
        //}
        private class SweepPoint
        {
            public GameObject Body;
            public bool Begin;
            public int Axis;

            public SweepPoint(GameObject body, bool begin, int axis)
            {
                Body = body;
                Begin = begin;
                Axis = axis;
            }

            public float Value
            {
                get
                {
                    if (Begin)
                    {
                        if (Axis == 0) return Body.boundingBox.Min.X;
                        else if (Axis == 1) return Body.boundingBox.Min.Y;
                        else return Body.boundingBox.Min.Z;
                    }
                    else
                    {
                        if (Axis == 0) return Body.boundingBox.Max.X;
                        else if (Axis == 1) return Body.boundingBox.Max.Y;
                        else return Body.boundingBox.Max.Z;
                    }
                }
            }
        }
        #endregion

        #region private struct OverlapPair
        private struct OverlapPair
        {
            // internal values for faster access within the engine
            public GameObject Entity1, Entity2;

            /// <summary>
            /// Initializes a new instance of the BodyPair class.
            /// </summary>
            /// <param name="entity1"></param>
            /// <param name="entity2"></param>
            public OverlapPair(GameObject entity1, GameObject entity2)
            {
                this.Entity1 = entity1;
                this.Entity2 = entity2;
            }

            /// <summary>
            /// Don't call this, while the key is used in the arbitermap.
            /// It changes the hashcode of this object.
            /// </summary>
            /// <param name="entity1">The first body.</param>
            /// <param name="entity2">The second body.</param>
            internal void SetBodies(GameObject entity1, GameObject entity2)
            {
                this.Entity1 = entity1;
                this.Entity2 = entity2;
            }

            /// <summary>
            /// Checks if two objects are equal.
            /// </summary>
            /// <param name="obj">The object to check against.</param>
            /// <returns>Returns true if they are equal, otherwise false.</returns>
            public override bool Equals(object obj)
            {
                OverlapPair other = (OverlapPair)obj;
                return (other.Entity1.Equals(Entity1) && other.Entity2.Equals(Entity2) ||
                    other.Entity1.Equals(Entity2) && other.Entity2.Equals(Entity1));
            }

            /// <summary>
            /// Returns the hashcode of the BodyPair.
            /// The hashcode is the same if an BodyPair contains the same bodies.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return Entity1.GetHashCode() + Entity2.GetHashCode();
            }
        }
        #endregion

        private List<SweepPoint> axis1 = new List<SweepPoint>();
        private List<SweepPoint> axis2 = new List<SweepPoint>();
        private List<SweepPoint> axis3 = new List<SweepPoint>();

        private HashSet<OverlapPair> fullOverlaps = new HashSet<OverlapPair>();

        public CollisionSystemPersistentSAP()
        {
        }

        #region Incoherent Update - Quicksort

        private int QuickSort(SweepPoint sweepPoint1, SweepPoint sweepPoint2)
        {
            float val1 = sweepPoint1.Value;
            float val2 = sweepPoint2.Value;

            if (val1 > val2) return 1;
            else if (val2 > val1) return -1;
            else return 0;
        }

        List<GameObject> activeList = new List<GameObject>();

        private void DirtySortAxis(List<SweepPoint> axis)
        {
            axis.Sort(QuickSort);
            activeList.Clear();

            for (int i = 0; i < axis.Count; i++)
            {
                SweepPoint keyelement = axis[i];

                if (keyelement.Begin)
                {
                    foreach (GameObject body in activeList)
                    {
                        if (CheckBoundingBoxes(body, keyelement.Body))
                            fullOverlaps.Add(new OverlapPair(body, keyelement.Body));
                    }

                    activeList.Add(keyelement.Body);
                }
                else
                {
                    activeList.Remove(keyelement.Body);
                }
            }
        }
        #endregion

        #region Coherent Update - Insertionsort

        private void SortAxis(List<SweepPoint> axis)
        {
            for (int j = 1; j < axis.Count; j++)
            {
                SweepPoint keyelement = axis[j];
                float key = keyelement.Value;

                int i = j - 1;

                while (i >= 0 && axis[i].Value > key)
                {
                    SweepPoint swapper = axis[i];

                    if (keyelement.Begin && !swapper.Begin)
                    {
                        if (CheckBoundingBoxes(swapper.Body, keyelement.Body))
                        {
                            lock (fullOverlaps) fullOverlaps.Add(new OverlapPair(swapper.Body, keyelement.Body));
                        }
                    }

                    if (!keyelement.Begin && swapper.Begin)
                    {
                        lock (fullOverlaps) fullOverlaps.Remove(new OverlapPair(swapper.Body, keyelement.Body));
                    }

                    axis[i + 1] = swapper;
                    i = i - 1;
                }
                axis[i + 1] = keyelement;
            }
        }
        #endregion

        int addCounter = 0;
        public override void AddEntity(GameObject body)
        {
            base.AddEntity(body);
            axis1.Add(new SweepPoint(body, true, 0)); axis1.Add(new SweepPoint(body, false, 0));
            axis2.Add(new SweepPoint(body, true, 1)); axis2.Add(new SweepPoint(body, false, 1));
            axis3.Add(new SweepPoint(body, true, 2)); axis3.Add(new SweepPoint(body, false, 2));

            addCounter++;
        }

        Stack<OverlapPair> depricated = new Stack<OverlapPair>();
        public override bool RemoveEntity(GameObject body)
        {
            int count;

            count = 0;
            for (int i = 0; i < axis1.Count; i++)
            { if (axis1[i].Body == body) { count++; axis1.RemoveAt(i); if (count == 2) break; i--; } }

            count = 0;
            for (int i = 0; i < axis2.Count; i++)
            { if (axis2[i].Body == body) { count++; axis2.RemoveAt(i); if (count == 2) break; i--; } }

            count = 0;
            for (int i = 0; i < axis3.Count; i++)
            { if (axis3[i].Body == body) { count++; axis3.RemoveAt(i); if (count == 2) break; i--; } }

            foreach (var pair in fullOverlaps) if (pair.Entity1 == body || pair.Entity2 == body) depricated.Push(pair);
            while (depricated.Count > 0) fullOverlaps.Remove(depricated.Pop());

            base.RemoveEntity(body);

            return true;
        }

        bool swapOrder = false;

        /// <summary>
        /// Tells the collisionsystem to check all bodies for collisions. Hook into the
        /// <see cref="CollisionSystem.PassedBroadphase"/>
        /// and <see cref="CollisionSystem.CollisionDetected"/> events to get the results.
        /// </summary>
        /// <param name="multiThreaded">If true internal multithreading is used.</param>
        public override void Detect(bool multiThreaded)
        {
            if (addCounter > AddedObjectsBruteForceIsUsed)
            {
                fullOverlaps.Clear();

                DirtySortAxis(axis1);
                DirtySortAxis(axis2);
                DirtySortAxis(axis3);
            }
            else
            {
                if (multiThreaded)
                {
                    threadManager.AddTask(SortAxis, axis1);
                    threadManager.AddTask(SortAxis, axis2);
                    threadManager.AddTask(SortAxis, axis3);

                    threadManager.Execute();
                }
                else
                {
                    SortAxis(axis1);
                    SortAxis(axis2);
                    SortAxis(axis3);
                }
            }

            addCounter = 0;

            foreach (OverlapPair key in fullOverlaps)
            {
                if (CheckBothStaticOrInactive(key.Entity1, key.Entity2)) continue;

                if (multiThreaded)
                {
                    BroadphasePair pair = BroadphasePair.Pool.GetNew();
                    if (swapOrder) { pair.Entity1 = key.Entity1; pair.Entity2 = key.Entity2; }
                    else { pair.Entity2 = key.Entity2; pair.Entity1 = key.Entity1; }
                    threadManager.AddTask(DetectCallback, pair);
                }
                else
                {
                    if (swapOrder) { Detect(key.Entity1, key.Entity2); }
                    else Detect(key.Entity2, key.Entity1);
                }

                swapOrder = !swapOrder;
            }

            threadManager.Execute();

        }

        private void DetectCallback(object obj)
        {
            BroadphasePair pair = obj as BroadphasePair;
            base.Detect(pair.Entity1, pair.Entity2);
            BroadphasePair.Pool.GiveBack(pair);
        }


        /// <summary>
        /// Sends a ray (definied by start and direction) through the scene (all bodies added).
        /// NOTE: For performance reasons terrain and trianglemeshshape aren't checked
        /// against rays (rays are of infinite length). They are checked against segments
        /// which start at rayOrigin and end in rayOrigin + rayDirection.
        /// </summary>
        public override bool Raycast(Vector3 rayOrigin, Vector3 rayDirection, Func<GameObject, Vector3, float, bool> raycast,
            out GameObject body, out Vector3 normal, out float fraction)
        {
            body = null; normal = Vector3.Zero; fraction = float.MaxValue;

            Vector3 tempNormal; float tempFraction;
            bool result = false;

            // TODO: This can be done better in CollisionSystemPersistenSAP
            foreach (GameObject e in bodyList)
            {
                if (Raycast(e, rayOrigin, rayDirection, out tempNormal, out tempFraction))
                {
                    if (tempFraction < fraction && (raycast == null || raycast(e, tempNormal, tempFraction)))
                    {
                        body = e;
                        normal = tempNormal;
                        fraction = tempFraction;
                        result = true;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Raycasts a single body. NOTE: For performance reasons terrain and trianglemeshshape aren't checked
        /// against rays (rays are of infinite length). They are checked against segments
        /// which start at rayOrigin and end in rayOrigin + rayDirection.
        /// </summary>
        public override bool Raycast(ICollidable body, Vector3 rayOrigin, Vector3 rayDirection, out Vector3 normal, out float fraction)
        {
            fraction = float.MaxValue; normal = Vector3.Zero;

            if (!body.BoundingBox.RayIntersect(ref rayOrigin, ref rayDirection) || body.Shape == null) return false;

            if (body.Shape is Multishape)
            {
                Multishape ms = (body.Shape as Multishape).RequestWorkingClone();

                Vector3 tempNormal; float tempFraction;
                bool multiShapeCollides = false;

                Vector3 transformedOrigin = body.InvOrientation * (rayOrigin - body.Position);
                Vector3 transformedDirection = body.InvOrientation * rayDirection;

                int msLength = ms.Prepare(ref transformedOrigin, ref transformedDirection);

                for (int i = 0; i < msLength; i++)
                {
                    ms.SetCurrentShape(i);

                    if (GJKCollide.Raycast(ms, body.Orientation, body.InvOrientation, body.Position, body.Velocity,
                        rayOrigin, rayDirection, out tempFraction, out tempNormal))
                    {
                        if (tempFraction < fraction)
                        {
                            //if (useTerrainNormal && ms is TerrainShape)
                            //{
                            //    (ms as TerrainShape).CollisionNormal(out tempNormal);
                            //    Vector3.Transform(ref tempNormal, ref body.orientation, out tempNormal);
                            //    tempNormal *= -1;
                            //}

                            normal = tempNormal;
                            fraction = tempFraction;
                            multiShapeCollides = true;
                        }
                    }
                }

                ms.ReturnWorkingClone();
                return multiShapeCollides;
            }
            else
            {
                return (GJKCollide.Raycast(body.Shape, body.Orientation, body.InvOrientation, body.Position, body.Velocity,
                    rayOrigin, rayDirection, out fraction, out normal));
            }


        }


    }
}
