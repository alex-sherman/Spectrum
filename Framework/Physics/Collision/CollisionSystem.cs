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
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Entities;
#endregion

namespace Spectrum.Framework.Physics.Collision
{


    /// <summary>
    /// A delegate for collision detection.
    /// </summary>
    /// <param name="body1">The first body colliding with the second one.</param>
    /// <param name="body2">The second body colliding with the first one.</param>
    /// <param name="point">The point on body in world coordinates, where collision occur.</param>
    /// <param name="normal">The normal pointing from body2 to body1.</param>
    /// <param name="penetration">Estimated penetration depth of the collision.</param>
    /// <seealso cref="CollisionSystem.Detect(bool)"/>
    /// <seealso cref="CollisionSystem.Detect(RigidBody,RigidBody)"/>
    public delegate void CollisionDetectedHandler(GameObject body1, GameObject body2,
                    Vector3 point, Vector3 normal, float penetration);

    /// <summary>
    /// A delegate to inform the user that a pair of bodies passed the broadsphase
    /// system of the engine.
    /// </summary>
    /// <param name="body1">The first body.</param>
    /// <param name="body2">The second body.</param>
    /// <returns>If false is returned the collision information is dropped. The CollisionDetectedHandler
    /// is never called.</returns>
    public delegate bool PassedBroadphaseHandler(GameObject entity1, GameObject entity2);

    /// <summary>
    /// A delegate to inform the user that a pair of bodies passed the narrowphase
    /// system of the engine.
    /// </summary>
    /// <param name="body1">The first body.</param>
    /// <param name="body2">The second body.</param>
    /// <returns>If false is returned the collision information is dropped. The CollisionDetectedHandler
    /// is never called.</returns>
    public delegate bool PassedNarrowphaseHandler(GameObject body1, GameObject body2,
                    ref Vector3 point, ref Vector3 normal, float penetration);

    /// <summary>
    /// A delegate for raycasting.
    /// </summary>
    /// <param name="body">The body for which collision with the ray is detected.</param>
    /// <param name="normal">The normal of the collision.</param>
    /// <param name="fraction">The fraction which gives information where at the 
    /// ray the collision occured. The hitPoint is calculated by: rayStart+friction*direction.</param>
    /// <returns>If false is returned the collision information is dropped.</returns>
    public delegate bool RaycastCallback(GameObject body, Vector3 normal, float fraction);

    /// <summary>
    /// CollisionSystem. Used by the world class to detect all collisions. 
    /// Can be used seperatly from the physics.
    /// </summary>
    public abstract class CollisionSystem
    {

        /// <summary>
        /// Helper class which holds two bodies. Mostly used
        /// for multithreaded detection. (Passing this as
        /// the object parameter to ThreadManager.Instance.AddTask)
        /// </summary>
        #region protected class BroadphasePair
        protected class BroadphasePair
        {
            /// <summary>
            /// The first body.
            /// </summary>
            public GameObject Entity1;
            /// <summary>
            /// The second body.
            /// </summary>
            public GameObject Entity2;

            /// <summary>
            /// A resource pool of Pairs.
            /// </summary>
            public static ResourcePool<BroadphasePair> Pool = new ResourcePool<BroadphasePair>();
        }
        #endregion


        protected List<GameObject> bodyList = new List<GameObject>();

        public virtual bool RemoveEntity(GameObject body)
        {
            return bodyList.Remove(body);
        }

        public virtual void AddEntity(GameObject body)
        {

            bodyList.Add(body);
        }

        /// <summary>
        /// Gets called when the broadphase system has detected possible collisions.
        /// </summary>
        public event PassedBroadphaseHandler PassedBroadphase;

        /// <summary>
        /// Gets called when broad- and narrow phase collision were positive.
        /// </summary>
        public event CollisionDetectedHandler CollisionDetected;

        protected ThreadManager threadManager = ThreadManager.Instance;

        private bool speculativeContacts = false;
        public bool EnableSpeculativeContacts
        {
            get { return speculativeContacts; }
            set { speculativeContacts = value; }
        }

        /// <summary>
        /// Initializes a new instance of the CollisionSystem.
        /// </summary>
        public CollisionSystem()
        {
        }

        /// <summary>
        /// Checks two bodies for collisions using narrowphase.
        /// </summary>
        /// <param name="body1">The first body.</param>
        /// <param name="body2">The second body.</param>
        #region public virtual void Detect(IBroadphaseEntity body1, IBroadphaseEntity body2)
        public virtual bool Detect(ICollidable entity1, ICollidable entity2)
        {
            Debug.Assert(entity1 != entity2, "CollisionSystem reports selfcollision. Something is wrong.");

            Vector3 point, normal;
            float penetration;
            bool output = false;
            if (GetContact(entity1, entity2, out point, out normal, out penetration))
            {
                output = true;
                if (this.CollisionDetected != null)
                    this.CollisionDetected(entity1 as GameObject, entity2 as GameObject, point, normal, penetration);
            }
            return output;
        }

        public List<ICollidable> GetCollisions(ICollidable entity)
        {
            List<ICollidable> output = new List<ICollidable>();
            foreach (ICollidable other in bodyList)
            {
                if (!CheckBoundingBoxes(entity, other)) continue;
                if(Detect(entity, other))
                {
                    output.Add(other);
                }
            }
            return output;
        }

        private bool GetContact(ISupportMappable s1, ISupportMappable s2, ICollidable b1, ICollidable b2,
            out Vector3 point, out Vector3 normal, out float penetration)
        {
            return XenoCollide.Detect(s1, s2, b1.Orientation,
                    b2.Orientation, b1.Position, b2.Position,
                    out point, out normal, out penetration);
            //else if (speculative)
            //{
            //    Vector3 hit1, hit2;

            //    if (GJKCollide.ClosestPoints(s1, s2, b1.Orientation, b2.Orientation,
            //        b1.Position, b2.Position, out hit1, out hit2, out normal))
            //    {
            //        Vector3 delta = hit2 - hit1;

            //        if (delta.LengthSquared() < (body1.SweptDirection - body2.SweptDirection).LengthSquared())
            //        {
            //            penetration = Vector3.Dot(delta, normal);

            //            if (penetration < 0.0f)
            //            {
            //                RaiseCollisionDetected(b1, b2, ref hit1, ref hit2, ref normal, penetration);
            //            }
            //        }
            //    }
            //}
        }

        public bool GetContact(ICollidable body1, ICollidable body2, out Vector3 point, out Vector3 normal, out float penetration)
        {
            point = Vector3.Zero;
            normal = Vector3.Zero;
            penetration = -1;

            int shape1count = 1;
            int shape2count = 1;

            Multishape ms1 = (body1.Shape as Multishape);
            Multishape ms2 = (body2.Shape as Multishape);
            if (ms1 != null)
            {
                ms1 = ms1.RequestWorkingClone();
                JBBox transformedBoundingBox = body2.BoundingBox;
                transformedBoundingBox.InverseTransform(body1.Position, body1.Orientation);

                shape1count = ms1.Prepare(ref transformedBoundingBox);
            }

            if (ms2 != null)
            {
                ms2 = ms2.RequestWorkingClone();
                JBBox transformedBoundingBox = body1.BoundingBox;
                transformedBoundingBox.InverseTransform(body2.Position, body2.Orientation);

                shape2count = ms2.Prepare(ref transformedBoundingBox);
            }

            if (shape1count == 0 || shape2count == 0)
            {
                if (ms1 != null)
                    ms1.ReturnWorkingClone();
                if (ms2 != null)
                    ms2.ReturnWorkingClone();
                return false;
            }

            bool collisionDetected = false;
            ISupportMappable s1 = ms1 == null ? body1.Shape : ms1;
            ISupportMappable s2 = ms2 == null ? body2.Shape : ms2;
            Vector3 tempPoint, tempNormal;
            float tempPenetration;
            for (int i = 0; i < shape1count; i++)
            {
                if (ms1 != null)
                    ms1.SetCurrentShape(i);
                for (int j = 0; j < shape2count; j++)
                {
                    if (ms2 != null)
                        ms2.SetCurrentShape(j);
                    if (XenoCollide.Detect(s1, s2, body1.Orientation,
                        body2.Orientation, body1.Position, body2.Position,
                        out tempPoint, out tempNormal, out tempPenetration))
                    {
                        if(tempPenetration > penetration)
                        {
                            point = tempPoint;
                            normal = tempNormal;
                            penetration = tempPenetration;
                        }
                        collisionDetected = true;
                    }
                }
            }

            if (ms1 != null)
                ms1.ReturnWorkingClone();
            if (ms2 != null)
                ms2.ReturnWorkingClone();

            return collisionDetected;
        }


        private void FindSupportPoints(GameObject body1, GameObject body2,
            Shape shape1, Shape shape2, ref Vector3 point, ref Vector3 normal,
            out Vector3 point1, out Vector3 point2)
        {
            Vector3 mn; Vector3.Negate(ref normal, out mn);

            Vector3 sA; SupportMapping(body1, shape1, ref mn, out sA);
            Vector3 sB; SupportMapping(body2, shape2, ref normal, out sB);

            Vector3.Subtract(ref sA, ref point, out sA);
            Vector3.Subtract(ref sB, ref point, out sB);

            float dot1 = Vector3.Dot(sA, normal);
            float dot2 = Vector3.Dot(sB, normal);

            Vector3.Multiply(ref normal, dot1, out sA);
            Vector3.Multiply(ref normal, dot2, out sB);

            Vector3.Add(ref point, ref sA, out point1);
            Vector3.Add(ref point, ref sB, out point2);
        }

        private void SupportMapping(GameObject body, Shape workingShape, ref Vector3 direction, out Vector3 result)
        {
            result = Vector3.Transform(direction, body.InvOrientation);
            workingShape.SupportMapping(ref result, out result);
            result = Vector3.Transform(direction, body.Orientation);
            result += body.Position;
        }

        #endregion

        /// <summary>
        /// Sends a ray (definied by start and direction) through the scene (all bodies added).
        /// NOTE: For performance reasons terrain and trianglemeshshape aren't checked
        /// against rays (rays are of infinite length). They are checked against segments
        /// which start at rayOrigin and end in rayOrigin + rayDirection.
        /// </summary>
        public abstract bool Raycast(Vector3 rayOrigin, Vector3 rayDirection, Func<GameObject, Vector3, float, bool> rayCast, out GameObject body, out Vector3 normal, out float fraction);

        /// <summary>
        /// Raycasts a single body. NOTE: For performance reasons terrain and trianglemeshshape aren't checked
        /// against rays (rays are of infinite length). They are checked against segments
        /// which start at rayOrigin and end in rayOrigin + rayDirection.
        /// </summary>
        public abstract bool Raycast(GameObject body, Vector3 rayOrigin, Vector3 rayDirection, out Vector3 normal, out float fraction);


        /// <summary>
        /// Checks the state of two bodies.
        /// </summary>
        /// <param name="entity1">The first body.</param>
        /// <param name="entity2">The second body.</param>
        /// <returns>Returns true if both are static or inactive.</returns>
        public bool CheckBothStaticOrInactive(GameObject entity1, GameObject entity2)
        {
            return ((entity1.IsStatic && entity2.IsStatic) || entity1.Ignore || entity2.Ignore);
        }

        /// <summary>
        /// Checks the AABB of the two rigid bodies.
        /// </summary>
        /// <param name="entity1">The first body.</param>
        /// <param name="entity2">The second body.</param>
        /// <returns>Returns true if an intersection occours.</returns>
        public bool CheckBoundingBoxes(ICollidable entity1, ICollidable entity2)
        {
            JBBox box1 = entity1.BoundingBox;
            JBBox box2 = entity2.BoundingBox;

            return ((((box1.Max.Z >= box2.Min.Z) && (box1.Min.Z <= box2.Max.Z)) &&
                ((box1.Max.Y >= box2.Min.Y) && (box1.Min.Y <= box2.Max.Y))) &&
                ((box1.Max.X >= box2.Min.X) && (box1.Min.X <= box2.Max.X)));
        }

        /// <summary>
        /// Raises the PassedBroadphase event.
        /// </summary>
        /// <param name="entity1">The first body.</param>
        /// <param name="entity2">The second body.</param>
        /// <returns>Returns false if the collision information
        /// should be dropped</returns>
        public bool RaisePassedBroadphase(GameObject entity1, GameObject entity2)
        {
            if (this.PassedBroadphase != null)
                return this.PassedBroadphase(entity1, entity2);

            // allow this detection by default
            return true;
        }


        /// <summary>
        /// Raises the CollisionDetected event.
        /// </summary>
        /// <param name="body1">The first body involved in the collision.</param>
        /// <param name="body2">The second body involved in the collision.</param>
        /// <param name="point">The collision point.</param>
        /// <param name="normal">The normal pointing to body1.</param>
        /// <param name="penetration">The penetration depth.</param>
        protected void RaiseCollisionDetected(GameObject body1, GameObject body2,
                                            ref Vector3 point1, ref Vector3 point2,
                                            ref Vector3 normal, float penetration)
        {
        }

        /// <summary>
        /// Tells the collisionsystem to check all bodies for collisions. Hook into the <see cref="PassedBroadphase"/>
        /// and <see cref="CollisionDetected"/> events to get the results.
        /// </summary>
        /// <param name="multiThreaded">If true internal multithreading is used.</param>
        public abstract void Detect(bool multiThreaded);
    }
}
