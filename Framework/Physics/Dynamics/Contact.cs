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
using Spectrum.Framework.Physics.Dynamics.Constraints;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Physics.Collision;
using Spectrum.Framework.Entities;
using System.Diagnostics;
#endregion

namespace Spectrum.Framework.Physics.Dynamics
{

    public enum RigidBodyIndex
    {
        RigidBody1, RigidBody2
    }

    #region public class ContactSettings
    public class ContactSettings
    {
        public enum MaterialCoefficientMixingType { TakeMaximum, TakeMinimum, UseAverage }

        internal float maximumBias = 10.0f;
        internal float bias = 0.25f;
        internal float minVelocity = 0.001f;
        internal float allowedPenetration = 0.01f;
        internal float breakThreshold = 0.01f;
        internal float slipThresholdSquared = 0.01f;

        internal MaterialCoefficientMixingType materialMode = MaterialCoefficientMixingType.TakeMinimum;

        public float MaximumBias { get { return maximumBias; } set { maximumBias = value; } }

        public float BiasFactor { get { return bias; } set { bias = value; } }

        public float MinimumVelocity { get { return minVelocity; } set { minVelocity = value; } }

        public float AllowedPenetration { get { return allowedPenetration; } set { allowedPenetration = value; } }

        public float BreakThreshold { get { return breakThreshold; } set { breakThreshold = value; } }

        public MaterialCoefficientMixingType MaterialCoefficientMixing { get { return materialMode; } set { materialMode = value; } }
    }
    #endregion


    /// <summary>
    /// </summary>
    public class Contact
    {
        private CollisionSystem system;
        private ContactSettings settings;

        internal GameObject body1, body2;

        internal Vector3 normal, tangent;

        internal Vector3 realRelPos1, realRelPos2;
        internal Vector3 relativePos1, relativePos2;
        internal Vector3 p1, p2;

        internal float accumulatedNormalImpulse = 0.0f;
        internal float accumulatedTangentImpulse = 0.0f;

        internal float slip = 0.0f;
        internal float initialPen = 0.0f;

        private float staticFriction, dynamicFriction;
        private float friction = 0.0f;

        private float massNormal = 0.0f, massTangent = 0.0f;
        private float restitutionBias = 0.0f;

        private bool newContact = false;

        private bool treatBody1AsStatic = false;
        private bool treatBody2AsStatic = false;
        private bool noCollide = false;

        float lostSpeculativeBounce = 0.0f;
        float speculativeVelocity = 0.0f;

        /// <summary>
        /// A contact resource pool.
        /// </summary>
        public static readonly ResourcePool<Contact> Pool =
            new ResourcePool<Contact>();

        private float lastTimeStep = float.PositiveInfinity;

        #region Properties
        public float Restitution { get; set; }

        public float StaticFriction
        {
            get { return staticFriction; }
            set { staticFriction = value; }
        }

        public float DynamicFriction
        {
            get { return dynamicFriction; }
            set { dynamicFriction = value; }
        }

        /// <summary>
        /// The first body involved in the contact.
        /// </summary>
        public GameObject Body1 { get { return body1; } }

        /// <summary>
        /// The second body involved in the contact.
        /// </summary>
        public GameObject Body2 { get { return body2; } }

        /// <summary>
        /// The penetration of the contact.
        /// </summary>
        public float Penetration { get; private set; }

        /// <summary>
        /// The collision position in world space of body1.
        /// </summary>
        public Vector3 Position1 { get { return p1; } }

        /// <summary>
        /// The collision position in world space of body2.
        /// </summary>
        public Vector3 Position2 { get { return p2; } }

        /// <summary>
        /// The contact tangent.
        /// </summary>
        public Vector3 Tangent { get { return tangent; } }

        /// <summary>
        /// The contact normal.
        /// </summary>
        public Vector3 Normal { get { return normal; } }
        #endregion

        public void NewIterate(float contactCount)
        {
            accumulatedNormalImpulse = 0;
            accumulatedTangentImpulse = -dynamicFriction;
            if (noCollide) return;
            float e = Restitution;
            Vector3 dv = body2.angularVelocity.Cross(relativePos2) + body2.linearVelocity;
            dv -= body1.angularVelocity.Cross(relativePos1) + body1.linearVelocity;
            float dvNormalScalar = Vector3.Dot(dv, normal);
            Vector3 dvNormal = normal * dvNormalScalar;
            Vector3 dvTangent = dv - dvNormal;
            tangent = dvTangent;
            if (tangent.LengthSquared > 0)
                tangent.Normalize();

            massNormal = InertiaInDirection(normal);
            massTangent = InertiaInDirection(tangent);
            accumulatedNormalImpulse = -(e + 1) * massNormal * dvNormalScalar;
            // TODO: Tangent force should be a function of the normal force or something and not just fixed
            Vector3 J = normal * accumulatedNormalImpulse + accumulatedTangentImpulse * tangent;
            ApplyImpulse(J / contactCount);
            if (Penetration > settings.allowedPenetration)
            {
                ApplyPush((Penetration - settings.allowedPenetration) * normal / contactCount);
            }
        }

        /// <summary>
        /// The points in world space gets recalculated by transforming the
        /// local coordinates. Also new penetration depth is estimated.
        /// </summary>
        public void UpdatePosition()
        {
            p1 = body1.orientation * realRelPos1 + body1.position;

            p2 = body2.orientation * realRelPos2 + body2.position;

            Vector3 dist = p1 - p2;
            Penetration = dist.Dot(normal);
            slip = (p1 - p2 - Penetration * normal).LengthSquared;
        }

        public void ApplyImpulse(Vector3 impulse)
        {
            Debug.Assert(!(float.IsNaN(impulse.X) || float.IsNaN(impulse.Y) || float.IsNaN(impulse.Z)));
            Vector3 rotationImpulse = impulse;
            if (!treatBody1AsStatic && !treatBody2AsStatic)
            {
                impulse /= 2;
                if (!body1.IgnoreRotation && !body2.IgnoreRotation)
                    rotationImpulse /= 2;

            }

            if (!treatBody1AsStatic)
            {
                body1.linearVelocity.X -= (impulse.X * body1.inverseMass);
                body1.linearVelocity.Y -= (impulse.Y * body1.inverseMass);
                body1.linearVelocity.Z -= (impulse.Z * body1.inverseMass);

                if (!body1.IgnoreRotation)
                {
                    float num0, num1, num2;
                    num0 = relativePos1.Y * rotationImpulse.Z - relativePos1.Z * rotationImpulse.Y;
                    num1 = relativePos1.Z * rotationImpulse.X - relativePos1.X * rotationImpulse.Z;
                    num2 = relativePos1.X * rotationImpulse.Y - relativePos1.Y * rotationImpulse.X;

                    float num3 =
                        (((num0 * body1.invInertiaWorld.M11) +
                        (num1 * body1.invInertiaWorld.M21)) +
                        (num2 * body1.invInertiaWorld.M31));
                    float num4 =
                        (((num0 * body1.invInertiaWorld.M12) +
                        (num1 * body1.invInertiaWorld.M22)) +
                        (num2 * body1.invInertiaWorld.M32));
                    float num5 =
                        (((num0 * body1.invInertiaWorld.M13) +
                        (num1 * body1.invInertiaWorld.M23)) +
                        (num2 * body1.invInertiaWorld.M33));

                    body1.angularVelocity.X -= num3;
                    body1.angularVelocity.Y -= num4;
                    body1.angularVelocity.Z -= num5;
                }
            }

            if (!treatBody2AsStatic)
            {

                body2.linearVelocity.X += (impulse.X * body2.inverseMass);
                body2.linearVelocity.Y += (impulse.Y * body2.inverseMass);
                body2.linearVelocity.Z += (impulse.Z * body2.inverseMass);

                if (!body2.IgnoreRotation)
                {
                    float num0, num1, num2;
                    num0 = relativePos2.Y * rotationImpulse.Z - relativePos2.Z * rotationImpulse.Y;
                    num1 = relativePos2.Z * rotationImpulse.X - relativePos2.X * rotationImpulse.Z;
                    num2 = relativePos2.X * rotationImpulse.Y - relativePos2.Y * rotationImpulse.X;

                    float num3 =
                        (((num0 * body2.invInertiaWorld.M11) +
                        (num1 * body2.invInertiaWorld.M21)) +
                        (num2 * body2.invInertiaWorld.M31));
                    float num4 =
                        (((num0 * body2.invInertiaWorld.M12) +
                        (num1 * body2.invInertiaWorld.M22)) +
                        (num2 * body2.invInertiaWorld.M32));
                    float num5 =
                        (((num0 * body2.invInertiaWorld.M13) +
                        (num1 * body2.invInertiaWorld.M23)) +
                        (num2 * body2.invInertiaWorld.M33));

                    body2.angularVelocity.X += num3;
                    body2.angularVelocity.Y += num4;
                    body2.angularVelocity.Z += num5;
                }
            }
            Debug.Assert(!(float.IsNaN(body1.linearVelocity.X) || float.IsNaN(body1.linearVelocity.Y) || float.IsNaN(body1.linearVelocity.Z)));
            Debug.Assert(!(float.IsNaN(body2.linearVelocity.X) || float.IsNaN(body2.linearVelocity.Y) || float.IsNaN(body2.linearVelocity.Z)));
        }

        public void ApplyPush(Vector3 push)
        {
            if (push.LengthSquared == 0)
                return;
            if (!treatBody1AsStatic && !treatBody2AsStatic)
                push /= 2;
            if (!treatBody1AsStatic)
            {
                body1.position -= push;
                //if (!body1.IgnoreRotation)
                //{
                //    Vector3 axis = Vector3.Cross(push, relativePos1);
                //    double angle = Math.Asin(axis.Length() / push.Length() / relativePos1.Length());
                //    Debug.Assert(!double.IsNaN(angle));
                //    if (angle != 0)
                //    {
                //        axis.Normalize();
                //        body1.orientation = Quaternion.Concatenate(body1.orientation, Quaternion.CreateFromAxisAngle(axis, -(float)angle/2));
                //    }
                //}
            }
            if (!treatBody2AsStatic)
            {
                body2.position += push;
                //if (!body2.IgnoreRotation)
                //{
                //    Vector3 axis = Vector3.Cross(push, relativePos2);
                //    double angle = Math.Asin(axis.Length() / push.Length() / relativePos2.Length());
                //    Debug.Assert(!double.IsNaN(angle));
                //    if (angle != 0)
                //    {
                //        axis.Normalize();
                //        body2.orientation = Quaternion.Concatenate(body2.orientation, Quaternion.CreateFromAxisAngle(axis, -(float)angle / 2));
                //    }
                //}
            }
        }

        private float InertiaInDirection(Vector3 direction)
        {
            float kNormal = 0.0f;

            Vector3 rantra = Vector3.Zero;
            if (!treatBody1AsStatic)
            {
                //TODO: This is completely wrong and should use the inertia provided by the shape
                kNormal += body1.inverseMass;
                if (!body1.IgnoreRotation)
                {
                    rantra = relativePos1.Cross(direction);
                    rantra = body1.invInertiaWorld * rantra;
                    rantra = rantra.Cross(relativePos1);
                    kNormal += rantra.X * direction.X + rantra.Y * direction.Y + rantra.Z * direction.Z;
                }

            }

            Vector3 rbntrb = Vector3.Zero;
            if (!treatBody2AsStatic)
            {
                kNormal += body2.inverseMass;

                if (!body2.IgnoreRotation)
                {
                    rbntrb = relativePos2.Cross(direction);
                    rbntrb = body2.invInertiaWorld * rbntrb;
                    rbntrb = rbntrb.Cross(relativePos2);
                    kNormal += rbntrb.X * direction.X + rbntrb.Y * direction.Y + rbntrb.Z * direction.Z;
                }
            }

            return 1.0f / kNormal;
        }

        /// <summary>
        /// Initializes a contact.
        /// </summary>
        /// <param name="body1">The first body.</param>
        /// <param name="body2">The second body.</param>
        /// <param name="point1">The collision point in worldspace</param>
        /// <param name="point2">The collision point in worldspace</param>
        /// <param name="n">The normal pointing to body2.</param>
        /// <param name="penetration">The estimated penetration depth.</param>
        public void Initialize(CollisionSystem system, GameObject body1, GameObject body2, ref Vector3 point, ref Vector3 n,
            float penetration, bool newContact, ContactSettings settings, bool noCollide)
        {
            this.noCollide = noCollide;
            this.system = system;
            this.body1 = body1; this.body2 = body2;
            this.normal = n; normal.Normalize();
            this.p1 = point + penetration * normal / 2; this.p2 = point - penetration * normal / 2;
            //this.p1 = point1;
            //this.p2 = point2;

            this.newContact = newContact;

            relativePos1 = p1 - body1.position;
            relativePos2 = p2 - body2.position;

            realRelPos1 = body1.invOrientation * relativePos1;
            realRelPos2 = body2.invOrientation * relativePos2;

            initialPen = penetration;
            Penetration = penetration;

            // Material Properties
            if (newContact)
            {
                treatBody1AsStatic = body1.isStatic;
                treatBody2AsStatic = body2.isStatic;

                accumulatedNormalImpulse = 0.0f;
                accumulatedTangentImpulse = 0.0f;

                lostSpeculativeBounce = 0.0f;

                switch (settings.MaterialCoefficientMixing)
                {
                    case ContactSettings.MaterialCoefficientMixingType.TakeMaximum:
                        staticFriction = JMath.Max(body1.material.staticFriction, body2.material.staticFriction);
                        dynamicFriction = JMath.Max(body1.material.kineticFriction, body2.material.kineticFriction);
                        Restitution = JMath.Max(body1.material.restitution, body2.material.restitution);
                        break;
                    case ContactSettings.MaterialCoefficientMixingType.TakeMinimum:
                        staticFriction = JMath.Min(body1.material.staticFriction, body2.material.staticFriction);
                        dynamicFriction = JMath.Min(body1.material.kineticFriction, body2.material.kineticFriction);
                        Restitution = JMath.Min(body1.material.restitution, body2.material.restitution);
                        break;
                    case ContactSettings.MaterialCoefficientMixingType.UseAverage:
                        staticFriction = (body1.material.staticFriction + body2.material.staticFriction) / 2.0f;
                        dynamicFriction = (body1.material.kineticFriction + body2.material.kineticFriction) / 2.0f;
                        Restitution = (body1.material.restitution + body2.material.restitution) / 2.0f;
                        break;
                }

            }
            this.settings = settings;
        }
    }
}
