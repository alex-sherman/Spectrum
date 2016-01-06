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
using System.Collections.ObjectModel;
using System.Diagnostics;

using Spectrum.Framework.Physics.Dynamics;
using Spectrum.Framework.Physics.LinearMath;
using Spectrum.Framework.Physics.Collision.Shapes;
using Spectrum.Framework.Physics.Collision;
using Spectrum.Framework.Physics.Dynamics.Constraints;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Entities;
#endregion

namespace Spectrum.Framework.Physics
{

    /// <summary>
    /// This class brings 'dynamics' and 'collisions' together. It handles
    /// all bodies and constraints.
    /// </summary>
    public class World
    {
        public delegate void WorldStep(float timestep);

        public class WorldEvents
        {
            // Post&Prestep
            public event WorldStep PreStep;
            public event WorldStep PostStep;

            // Add&Remove
            public event Action<GameObject> AddedRigidBody;
            public event Action<GameObject> RemovedRigidBody;
            public event Action<Constraint> AddedConstraint;
            public event Action<Constraint> RemovedConstraint;
            //public event Action<SoftBody> AddedSoftBody;
            //public event Action<SoftBody> RemovedSoftBody;

            // Collision
            public event Action<GameObject, GameObject, Vector3> BodiesBeginCollide;
            public event Action<GameObject, GameObject> BodiesEndCollide;
            public event Action<Contact> ContactCreated;

            // Deactivation
            public event Action<GameObject> DeactivatedBody;
            public event Action<GameObject> ActivatedBody;

            internal WorldEvents() { }

            #region Raise Events

            internal void RaiseWorldPreStep(float timestep)
            {
                if (PreStep != null) PreStep(timestep);
            }

            internal void RaiseWorldPostStep(float timestep)
            {
                if (PostStep != null) PostStep(timestep);
            }

            internal void RaiseAddedRigidBody(GameObject body)
            {
                if (AddedRigidBody != null) AddedRigidBody(body);
            }

            internal void RaiseRemovedRigidBody(GameObject body)
            {
                if (RemovedRigidBody != null) RemovedRigidBody(body);
            }

            internal void RaiseAddedConstraint(Constraint constraint)
            {
                if (AddedConstraint != null) AddedConstraint(constraint);
            }

            internal void RaiseRemovedConstraint(Constraint constraint)
            {
                if (RemovedConstraint != null) RemovedConstraint(constraint);
            }

            internal void RaiseBodiesBeginCollide(GameObject body1, GameObject body2, Vector3 normal)
            {
                if (BodiesBeginCollide != null) BodiesBeginCollide(body1, body2, normal);
            }

            internal void RaiseBodiesEndCollide(GameObject body1, GameObject body2)
            {
                if (BodiesEndCollide != null) BodiesEndCollide(body1, body2);
            }

            internal void RaiseActivatedBody(GameObject body)
            {
                if (ActivatedBody != null) ActivatedBody(body);
            }

            internal void RaiseDeactivatedBody(GameObject body)
            {
                if (DeactivatedBody != null) DeactivatedBody(body);
            }

            internal void RaiseContactCreated(Contact contact)
            {
                if (ContactCreated != null) ContactCreated(contact);
            }

            #endregion
        }

        private ContactSettings contactSettings = new ContactSettings();

        private float inactiveAngularThresholdSq = 0.1f;
        private float inactiveLinearThresholdSq = 0.1f;
        private float deactivationTime = 2f;

        private float angularDamping = 0.85f;
        private float linearDamping = 0.85f;

        private int contactIterations = 10;
        private int smallIterations = 4;
        private float timestep = 0.0f;

        private Spectrum.Framework.Physics.Collision.IslandManager islands = new IslandManager();

        public HashSet<GameObject> Collidables { get; private set; }
        public HashSet<Constraint> Constraints { get; private set; }

        private WorldEvents events = new WorldEvents();
        public WorldEvents Events { get { return events; } }

        private ThreadManager threadManager = ThreadManager.Instance;

        /// <summary>
        /// Holds a list of <see cref="Arbiter"/>. All currently
        /// active arbiter in the <see cref="World"/> are stored in this map.
        /// </summary>
        public ArbiterMap ArbiterMap { get { return arbiterMap; } }
        private ArbiterMap arbiterMap;

        private Queue<Arbiter> removedArbiterQueue = new Queue<Arbiter>();
        private Queue<Arbiter> addedArbiterQueue = new Queue<Arbiter>();

        private Vector3 gravity = new Vector3(0, -9.81f, 0);

        public ContactSettings ContactSettings { get { return contactSettings; } }

        private Action<object> arbiterCallback;
        private Action<object> integrateCallback;

        private CollisionDetectedHandler collisionDetectionHandler;

        /// <summary>
        /// Create a new instance of the <see cref="World"/> class.
        /// </summary>
        /// <param name="collision">The collisionSystem which is used to detect
        /// collisions. See for example: <see cref="CollisionSystemSAP"/>
        /// or <see cref="CollisionSystemBrute"/>.
        /// </param>
        public World(CollisionSystem collision)
        {
            if (collision == null)
                throw new ArgumentNullException("The CollisionSystem can't be null.", "collision");

            arbiterCallback = new Action<object>(ArbiterCallback);
            integrateCallback = new Action<object>(IntegrateCallback);

            // Create the readonly wrappers
            this.Collidables = new HashSet<GameObject>();
            this.Constraints = new HashSet<Constraint>();
            //this.SoftBodies = new ReadOnlyHashset<SoftBody>(softbodies);

            this.CollisionSystem = collision;

            collisionDetectionHandler = new CollisionDetectedHandler(CollisionDetected);

            this.CollisionSystem.CollisionDetected += collisionDetectionHandler;

            this.arbiterMap = new ArbiterMap();

            AllowDeactivation = true;
        }

        /// <summary>
        /// Gets the <see cref="CollisionSystem"/> used
        /// to detect collisions.
        /// </summary>
        public CollisionSystem CollisionSystem { set; get; }

        /// <summary>
        /// In Jitter many objects get added to stacks after they were used.
        /// If a new object is needed the old object gets removed from the stack
        /// and is reused. This saves some time and also garbage collections.
        /// Calling this method removes all cached objects from all
        /// stacks.
        /// </summary>
        public void ResetResourcePools()
        {
            IslandManager.Pool.ResetResourcePool();
            Arbiter.Pool.ResetResourcePool();
            Contact.Pool.ResetResourcePool();
        }

        /// <summary>
        /// Removes all objects from the world and removes all memory cached objects.
        /// </summary>
        public void Clear()
        {
            // remove bodies from collision system
            foreach (GameObject body in Collidables)
            {
                CollisionSystem.RemoveEntity(body);

                if (body.island != null)
                {
                    body.island.ClearLists();
                    body.island = null;
                }

                body.connections.Clear();
                body.arbiters.Clear();
                body.constraints.Clear();

                events.RaiseRemovedRigidBody(body);
            }

            //foreach (SoftBody body in softbodies)
            //{
            //    CollisionSystem.RemoveEntity(body);
            //}

            // remove bodies from the world
            Collidables.Clear();

            // remove constraints
            foreach (Constraint constraint in Constraints)
            {
                events.RaiseRemovedConstraint(constraint);
            }
            Constraints.Clear();

            //softbodies.Clear();

            // remove all islands
            islands.RemoveAll();

            // delete the arbiters
            arbiterMap.Clear();

            ResetResourcePools();
        }

        /// <summary>
        /// Gets or sets the gravity in this <see cref="World"/>. The default gravity
        /// is (0,-9.81,0)
        /// </summary>
        public Vector3 Gravity { get { return gravity; } set { gravity = value; } }

        /// <summary>
        /// Global sets or gets if a body is able to be temporarily deactivated by the engine to
        /// safe computation time. Use <see cref="SetInactivityThreshold"/> to set parameters
        /// of the deactivation process.
        /// </summary>
        public bool AllowDeactivation { get; set; }

        /// <summary>
        /// Every computation <see cref="Step"/> the angular and linear velocity 
        /// of a <see cref="RigidBody"/> gets multiplied by this value.
        /// </summary>
        /// <param name="angularDamping">The factor multiplied with the angular velocity.
        /// The default value is 0.85.</param>
        /// <param name="linearDamping">The factor multiplied with the linear velocity.
        /// The default value is 0.85</param>
        public void SetDampingFactors(float angularDamping, float linearDamping)
        {
            if (angularDamping < 0.0f || angularDamping > 1.0f)
                throw new ArgumentException("Angular damping factor has to be between 0.0 and 1.0", "angularDamping");

            if (linearDamping < 0.0f || linearDamping > 1.0f)
                throw new ArgumentException("Linear damping factor has to be between 0.0 and 1.0", "linearDamping");

            this.angularDamping = angularDamping;
            this.linearDamping = linearDamping;
        }

        /// <summary>
        /// Sets parameters for the <see cref="RigidBody"/> deactivation process.
        /// If the bodies angular velocity is less than the angular velocity threshold
        /// and its linear velocity is lower then the linear velocity threshold for a 
        /// specific time the body gets deactivated. A body can be reactivated by setting
        /// <see cref="RigidBody.IsActive"/> to true. A body gets also automatically
        /// reactivated if another moving object hits it or the <see cref="CollisionIsland"/>
        /// the object is in gets activated.
        /// </summary>
        /// <param name="angularVelocity">The threshold value for the angular velocity. The default value
        /// is 0.1.</param>
        /// <param name="linearVelocity">The threshold value for the linear velocity. The default value
        /// is 0.1</param>
        /// <param name="time">The threshold value for the time in seconds. The default value is 2.</param>
        public void SetInactivityThreshold(float angularVelocity, float linearVelocity, float time)
        {
            if (angularVelocity < 0.0f) throw new ArgumentException("Angular velocity threshold has to " +
                 "be larger than zero", "angularVelocity");

            if (linearVelocity < 0.0f) throw new ArgumentException("Linear velocity threshold has to " +
                "be larger than zero", "linearVelocity");

            if (time < 0.0f) throw new ArgumentException("Deactivation time threshold has to " +
                "be larger than zero", "time");

            this.inactiveAngularThresholdSq = angularVelocity * angularVelocity;
            this.inactiveLinearThresholdSq = linearVelocity * linearVelocity;
            this.deactivationTime = time;
        }

        /// <summary>
        /// Jitter uses an iterativ approach to solve collisions and contacts. You can set the number of
        /// iterations Jitter should do. In general the more iterations the more stable a simulation gets
        /// but also costs computation time.
        /// </summary>
        /// <param name="iterations">The number of contact iterations. Default value 10.</param>
        /// <param name="smallIterations">The number of contact iteration used for smaller (two and three constraint) systems. Default value 4.</param>
        /// <remarks>The number of iterations for collision and contact should be between 3 - 30.
        /// More iterations means more stability and also a longer calculation time.</remarks>
        public void SetIterations(int iterations, int smallIterations)
        {
            if (iterations < 1) throw new ArgumentException("The number of collision " +
                 "iterations has to be larger than zero", "iterations");

            if (smallIterations < 1) throw new ArgumentException("The number of collision " +
                "iterations has to be larger than zero", "smallIterations");

            this.contactIterations = iterations;
            this.smallIterations = smallIterations;
        }

        /// <summary>
        /// Removes a <see cref="RigidBody"/> from the world.
        /// </summary>
        /// <param name="body">The body which should be removed.</param>
        /// <returns>Returns false if the body could not be removed from the world.</returns>
        public bool RemoveBody(GameObject body)
        {
            return RemoveBody(body, false);
        }

        private bool RemoveBody(GameObject body, bool removeMassPoints)
        {
            // remove the body from the world list
            if (!Collidables.Remove(body)) return false;

            // Remove all connected constraints and arbiters
            foreach (Arbiter arbiter in body.arbiters)
            {
                arbiterMap.Remove(arbiter);
                events.RaiseBodiesEndCollide(arbiter.body1, arbiter.body2);
            }

            foreach (Constraint constraint in body.constraints)
            {
                Constraints.Remove(constraint);
                events.RaiseRemovedConstraint(constraint);
            }

            // remove the body from the collision system
            CollisionSystem.RemoveEntity(body);

            // remove the body from the island manager
            islands.RemoveBody(body);

            events.RaiseRemovedRigidBody(body);

            return true;
        }


        /// <summary>
        /// Adds a <see cref="RigidBody"/> to the world.
        /// </summary>
        /// <param name="body">The body which should be added.</param>
        public void AddBody(GameObject body)
        {
            if (body == null) throw new ArgumentNullException("body", "body can't be null.");
            if (Collidables.Contains(body)) throw new ArgumentException("The body was already added to the world.", "body");

            events.RaiseAddedRigidBody(body);

            this.CollisionSystem.AddEntity(body);

            Collidables.Add(body);
        }

        /// <summary>
        /// Add a <see cref="Constraint"/> to the world. Fast, O(1).
        /// </summary>
        /// <param name="constraint">The constraint which should be added.</param>
        /// <returns>True if the constraint was successfully removed.</returns>
        public bool RemoveConstraint(Constraint constraint)
        {
            if (!Constraints.Remove(constraint)) return false;
            events.RaiseRemovedConstraint(constraint);

            islands.ConstraintRemoved(constraint);

            return true;
        }

        /// <summary>
        /// Add a <see cref="Constraint"/> to the world.
        /// </summary>
        /// <param name="constraint">The constraint which should be removed.</param>
        public void AddConstraint(Constraint constraint)
        {
            if (Constraints.Contains(constraint))
                throw new ArgumentException("The constraint was already added to the world.", "constraint");

            Constraints.Add(constraint);

            islands.ConstraintCreated(constraint);

            events.RaiseAddedConstraint(constraint);
        }

        private float currentLinearDampFactor = 1.0f;
        private float currentAngularDampFactor = 1.0f;

#if(!WINDOWS_PHONE)
        Stopwatch sw = new Stopwatch();

        public enum DebugType
        {
            CollisionDetect, BuildIslands, HandleArbiter, UpdateContacts,
            PreStep, DeactivateBodies, IntegrateForces, Integrate, PostStep, ClothUpdate, Num
        }

        /// <summary>
        /// Time in ms for every part of the <see cref="Step"/> method.
        /// </summary>
        /// <example>int time = DebugTimes[(int)DebugType.CollisionDetect] gives
        /// the amount of time spent on collision detection during the last <see cref="Step"/>.
        /// </example>
        private double[] debugTimes = new double[(int)DebugType.Num];
        public double[] DebugTimes { get { return debugTimes; } }
#endif

        /// <summary>
        /// Integrates the whole world a timestep further in time.
        /// </summary>
        /// <param name="timestep">The timestep in seconds. 
        /// It should be small as possible to keep the simulation stable.
        /// The physics simulation shouldn't run slower than 60fps.
        /// (timestep=1/60).</param>
        /// <param name="multithread">If true the engine uses several threads to
        /// integrate the world. This is faster on multicore CPUs.</param>
        public void Step(float timestep, bool multithread)
        {
            this.timestep = timestep;

            // yeah! nothing to do!
            if (timestep == 0.0f) return;

            // throw exception if the timestep is smaller zero.
            if (timestep < 0.0f) throw new ArgumentException("The timestep can't be negative.", "timestep");

            GJKCollide.Timestep = timestep;

            // Calculate this
            currentAngularDampFactor = (float)Math.Pow(angularDamping, timestep);
            currentLinearDampFactor = (float)Math.Pow(linearDamping, timestep);

            sw.Reset(); sw.Start();
            events.RaiseWorldPreStep(timestep);
            foreach (GameObject body in Collidables) body.PreStep(timestep);

            sw.Stop(); debugTimes[(int)DebugType.PreStep] = sw.Elapsed.TotalMilliseconds;

            sw.Reset(); sw.Start();
            UpdateContacts();
            sw.Stop(); debugTimes[(int)DebugType.UpdateContacts] = sw.Elapsed.TotalMilliseconds;

            sw.Reset(); sw.Start();
            double ms = 0;
            while (removedArbiterQueue.Count > 0) islands.ArbiterRemoved(removedArbiterQueue.Dequeue());
            sw.Stop(); ms = sw.Elapsed.TotalMilliseconds;

            sw.Reset(); sw.Start();
            CollisionSystem.Detect(multithread);
            sw.Stop(); debugTimes[(int)DebugType.CollisionDetect] = sw.Elapsed.TotalMilliseconds;

            sw.Reset(); sw.Start();

            while (addedArbiterQueue.Count > 0) islands.ArbiterCreated(addedArbiterQueue.Dequeue());

            sw.Stop(); debugTimes[(int)DebugType.BuildIslands] = sw.Elapsed.TotalMilliseconds + ms;

            sw.Reset(); sw.Start();
            CheckDeactivation();
            sw.Stop(); debugTimes[(int)DebugType.DeactivateBodies] = sw.Elapsed.TotalMilliseconds;

            sw.Reset(); sw.Start();
            IntegrateForces();
            sw.Stop(); debugTimes[(int)DebugType.IntegrateForces] = sw.Elapsed.TotalMilliseconds;

            sw.Reset(); sw.Start();
            HandleArbiter(contactIterations, multithread);
            sw.Stop(); debugTimes[(int)DebugType.HandleArbiter] = sw.Elapsed.TotalMilliseconds;

            sw.Reset(); sw.Start();
            Integrate(multithread);
            sw.Stop(); debugTimes[(int)DebugType.Integrate] = sw.Elapsed.TotalMilliseconds;

            sw.Reset(); sw.Start();
            foreach (GameObject body in Collidables) body.PostStep(timestep);
            events.RaiseWorldPostStep(timestep);
            sw.Stop(); debugTimes[(int)DebugType.PostStep] = sw.Elapsed.TotalMilliseconds;
        }

        private float accumulatedTime = 0.0f;

        /// <summary>
        /// Integrates the whole world several fixed timestep further in time.
        /// </summary>
        /// <param name="totalTime">The time to integrate.</param>
        /// <param name="timestep">The timestep in seconds. 
        /// It should be small as possible to keep the simulation stable.
        /// The physics simulation shouldn't run slower than 60fps.
        /// (timestep=1/60).</param>
        /// <param name="multithread">If true the engine uses several threads to
        /// integrate the world. This is faster on multicore CPUs.</param>
        /// <param name="maxSteps">The maximum number of substeps. After that Jitter gives up
        /// to keep up with the given totalTime.</param>
        public void Step(float totalTime, bool multithread, float timestep, int maxSteps)
        {
            int counter = 0;
            accumulatedTime += totalTime;

            while (accumulatedTime > timestep)
            {
                Step(timestep, multithread);

                accumulatedTime -= timestep;
                counter++;

                if (counter > maxSteps)
                {
                    // okay, okay... we can't keep up
                    accumulatedTime = 0.0f;
                    break;
                }
            }

        }

        private void UpdateArbiterContacts(Arbiter arbiter)
        {
            if (arbiter.contactList.Count == 0)
            {
                lock (removedArbiterStack) { removedArbiterStack.Push(arbiter); }
                return;
            }

            for (int i = arbiter.contactList.Count - 1; i >= 0; i--)
            {
                Contact c = arbiter.contactList[i];
                c.UpdatePosition();

                if (c.penetration < -contactSettings.breakThreshold || (c.p1 - c.p2).LengthSquared() > contactSettings.slipThresholdSquared)
                {
                    Contact.Pool.GiveBack(c);
                    arbiter.contactList.RemoveAt(i);
                    continue;
                }

            }
        }

        private Stack<Arbiter> removedArbiterStack = new Stack<Arbiter>();

        private void UpdateContacts()
        {
            foreach (Arbiter arbiter in arbiterMap.Arbiters)
            {
                UpdateArbiterContacts(arbiter);
            }

            while (removedArbiterStack.Count > 0)
            {
                Arbiter arbiter = removedArbiterStack.Pop();
                Arbiter.Pool.GiveBack(arbiter);
                arbiterMap.Remove(arbiter);

                removedArbiterQueue.Enqueue(arbiter);
                events.RaiseBodiesEndCollide(arbiter.body1, arbiter.body2);
            }

        }

        #region private void ArbiterCallback(object obj)
        private void ArbiterCallback(object obj)
        {
            CollisionIsland island = obj as CollisionIsland;

            int thisIterations;
            if (island.Bodies.Count + island.Constraints.Count > 3) thisIterations = contactIterations;
            else thisIterations = smallIterations;

            for (int i = -1; i < thisIterations; i++)
            {
                // Contact and Collision
                foreach (Arbiter arbiter in island.arbiter)
                {
                    int contactCount = arbiter.contactList.Count;
                    for (int e = 0; e < contactCount; e++)
                    {
                        if (i == -1) arbiter.contactList[e].PrepareForIteration(timestep);
                        else arbiter.contactList[e].Iterate();
                    }
                }

                //  Constraints
                foreach (Constraint c in island.constraints)
                {
                    if (c.body1 != null && !c.body1.IsActive && c.body2 != null && !c.body2.IsActive)
                        continue;

                    if (i == -1) c.PrepareForIteration(timestep);
                    else c.Iterate();
                }

            }
        }
        #endregion

        private void HandleArbiter(int iterations, bool multiThreaded)
        {
            if (multiThreaded)
            {
                for (int i = 0; i < islands.Count; i++)
                {
                    if (islands[i].IsActive()) threadManager.AddTask(arbiterCallback, islands[i]);
                }

                threadManager.Execute();
            }
            else
            {
                for (int i = 0; i < islands.Count; i++)
                {
                    if (islands[i].IsActive()) arbiterCallback(islands[i]);
                }

            }
        }

        private void IntegrateForces()
        {
            foreach (GameObject body in Collidables)
            {
                if (!body.IsStatic && body.IsActive)// && !(body as Character != null && (body as Character).IsOnGround))
                {
                    Vector3 temp;
                    Vector3.Multiply(ref body.force, body.inverseMass * timestep, out temp);
                    Vector3.Add(ref temp, ref body.linearVelocity, out body.linearVelocity);

                    Vector3.Multiply(ref body.torque, timestep, out temp);
                    Vector3.Transform(ref temp, ref body.invInertiaWorld, out temp);
                    Vector3.Add(ref temp, ref body.angularVelocity, out body.angularVelocity);

                    if (body.affectedByGravity)
                    {
                        Vector3.Multiply(ref gravity, timestep, out temp);
                        Vector3.Add(ref body.linearVelocity, ref temp, out body.linearVelocity);
                    }
                }

                body.force = new Vector3();
                body.torque = new Vector3();

            }
        }

        #region private void IntegrateCallback(object obj)
        private void IntegrateCallback(object obj)
        {
            GameObject body = obj as GameObject;

            Vector3 temp;
            Vector3.Multiply(ref body.linearVelocity, timestep, out temp);
            Vector3.Add(ref temp, ref body.position, out body.position);


            //exponential map
            Vector3 axis;
            float angle = body.angularVelocity.Length();

            if (angle < 0.001f)
            {
                // use Taylor's expansions of sync function
                // axis = body.angularVelocity * (0.5f * timestep - (timestep * timestep * timestep) * (0.020833333333f) * angle * angle);
                Vector3.Multiply(ref body.angularVelocity, (0.5f * timestep - (timestep * timestep * timestep) * (0.020833333333f) * angle * angle), out axis);
            }
            else
            {
                // sync(fAngle) = sin(c*fAngle)/t
                Vector3.Multiply(ref body.angularVelocity, ((float)Math.Sin(0.5f * angle * timestep) / angle), out axis);
            }

            Quaternion dorn = new Quaternion(axis.X, axis.Y, axis.Z, (float)Math.Cos(angle * timestep * 0.5f));
            Quaternion ornA; Quaternion.CreateFromRotationMatrix(ref body.orientation, out ornA);

            Quaternion.Multiply(ref dorn, ref ornA, out dorn);

            dorn.Normalize(); Matrix.CreateFromQuaternion(ref dorn, out body.orientation);


            if ((body.Damping & GameObject.DampingType.Linear) != 0)
                Vector3.Multiply(ref body.linearVelocity, currentLinearDampFactor, out body.linearVelocity);

            if ((body.Damping & GameObject.DampingType.Angular) != 0)
                Vector3.Multiply(ref body.angularVelocity, currentAngularDampFactor, out body.angularVelocity);

            body.PhysicsUpdate();


            if (CollisionSystem.EnableSpeculativeContacts || body.EnableSpeculativeContacts)
                body.SweptExpandBoundingBox(timestep);
        }
        #endregion


        private void Integrate(bool multithread)
        {
            if (multithread)
            {
                foreach (GameObject body in Collidables)
                {
                    if (body.IsStatic || !body.IsActive) continue;
                    threadManager.AddTask(integrateCallback, body);
                }

                threadManager.Execute();
            }
            else
            {
                foreach (GameObject body in Collidables)
                {
                    if (body.IsStatic || !body.IsActive) continue;
                    integrateCallback(body);
                }
            }
        }

        private void CollisionDetected(GameObject body1, GameObject body2, Vector3 point, Vector3 normal, float penetration)
        {
            Arbiter arbiter = null;
            if (body1 == null || body2 == null) { return; }

            lock (arbiterMap)
            {
                arbiterMap.LookUpArbiter(body1, body2, out arbiter);
                if (arbiter == null)
                {
                    arbiter = Arbiter.Pool.GetNew();
                    arbiter.body1 = body1; arbiter.body2 = body2;
                    arbiter.system = CollisionSystem;
                    arbiterMap.Add(new ArbiterKey(body1, body2), arbiter);

                    addedArbiterQueue.Enqueue(arbiter);

                    events.RaiseBodiesBeginCollide(body1, body2, normal);
                }
            }

            Contact contact = null;

            if (arbiter.body1 == body1)
            {
                Vector3.Negate(ref normal, out normal);
                contact = arbiter.AddContact(point, point, normal, penetration, contactSettings);
            }
            else
            {
                contact = arbiter.AddContact(point, point, normal, penetration, contactSettings);
            }

            if (contact != null) events.RaiseContactCreated(contact);

        }

        private void CheckDeactivation()
        {
            // A body deactivation DOESN'T kill the contacts - they are stored in
            // the arbitermap within the arbiters. So, waking up ist STABLE - old
            // contacts are reused. Also the collisionislands build every frame (based 
            // on the contacts) keep the same.

            foreach (CollisionIsland island in islands)
            {
                bool deactivateIsland = true;

                // global allowdeactivation
                if (!this.AllowDeactivation) deactivateIsland = false;
                else
                {
                    foreach (GameObject body in island.bodies)
                    {
                        // body allowdeactivation
                        if (body.AllowDeactivation && (body.angularVelocity.LengthSquared() < inactiveAngularThresholdSq &&
                        (body.linearVelocity.LengthSquared() < inactiveLinearThresholdSq)))
                        {
                            body.inactiveTime += timestep;
                            if (body.inactiveTime < deactivationTime)
                                deactivateIsland = false;
                        }
                        else
                        {
                            body.inactiveTime = 0.0f;
                            deactivateIsland = false;
                        }
                    }
                }

                foreach (GameObject body in island.bodies)
                {
                    if (body.IsActive == deactivateIsland)
                    {
                        if (body.IsActive)
                        {
                            body.IsActive = false;
                            events.RaiseDeactivatedBody(body);
                        }
                        else
                        {
                            body.IsActive = true;
                            events.RaiseActivatedBody(body);
                        }
                    }

                }
            }
        }

    }
}
