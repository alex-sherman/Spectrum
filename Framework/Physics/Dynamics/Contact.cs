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
        internal float slipThresholdSquared = 1f;

        internal MaterialCoefficientMixingType materialMode = MaterialCoefficientMixingType.UseAverage;

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

        /// <summary>
        /// Solves the contact iteratively.
        /// </summary>
        public void Iterate()
        {
            if (noCollide) return;
            if (treatBody1AsStatic && treatBody2AsStatic) return;

            float dvx, dvy, dvz;

            dvx = body2.linearVelocity.X - body1.linearVelocity.X;
            dvy = body2.linearVelocity.Y - body1.linearVelocity.Y;
            dvz = body2.linearVelocity.Z - body1.linearVelocity.Z;

            dvx = dvx - (body1.angularVelocity.Y * relativePos1.Z) + (body1.angularVelocity.Z * relativePos1.Y);
            dvy = dvy - (body1.angularVelocity.Z * relativePos1.X) + (body1.angularVelocity.X * relativePos1.Z);
            dvz = dvz - (body1.angularVelocity.X * relativePos1.Y) + (body1.angularVelocity.Y * relativePos1.X);

            dvx = dvx + (body2.angularVelocity.Y * relativePos2.Z) - (body2.angularVelocity.Z * relativePos2.Y);
            dvy = dvy + (body2.angularVelocity.Z * relativePos2.X) - (body2.angularVelocity.X * relativePos2.Z);
            dvz = dvz + (body2.angularVelocity.X * relativePos2.Y) - (body2.angularVelocity.Y * relativePos2.X);

            // this gets us some performance
            if (dvx * dvx + dvy * dvy + dvz * dvz < settings.minVelocity * settings.minVelocity)
            { return; }

            float vn = normal.X * dvx + normal.Y * dvy + normal.Z * dvz;
            float normalImpulse = massNormal * (-vn + restitutionBias + speculativeVelocity);

            float oldNormalImpulse = accumulatedNormalImpulse;
            accumulatedNormalImpulse = oldNormalImpulse + normalImpulse;
            if (accumulatedNormalImpulse < 0.0f) accumulatedNormalImpulse = 0.0f;
            normalImpulse = accumulatedNormalImpulse - oldNormalImpulse;

            float vt = dvx * tangent.X + dvy * tangent.Y + dvz * tangent.Z;
            float maxTangentImpulse = friction * accumulatedNormalImpulse;
            float tangentImpulse = massTangent * (-vt);

            float oldTangentImpulse = accumulatedTangentImpulse;
            accumulatedTangentImpulse = oldTangentImpulse + tangentImpulse;
            if (accumulatedTangentImpulse < -maxTangentImpulse) accumulatedTangentImpulse = -maxTangentImpulse;
            else if (accumulatedTangentImpulse > maxTangentImpulse) accumulatedTangentImpulse = maxTangentImpulse;

            tangentImpulse = accumulatedTangentImpulse - oldTangentImpulse;

            // Apply contact impulse
            Vector3 impulse;
            impulse.X = normal.X * normalImpulse + tangent.X * tangentImpulse;
            impulse.Y = normal.Y * normalImpulse + tangent.Y * tangentImpulse;
            impulse.Z = normal.Z * normalImpulse + tangent.Z * tangentImpulse;

            ApplyImpulse(impulse);
        }

        public void NewIterate()
        {
            if (Vector3.Dot(body2.linearVelocity - body1.linearVelocity, normal) > 0)
                return;
            if (Penetration < 0) return;
            float e = 0.5f;
            //matrix IaInverse = Ia.inverse();
            //vector angularVelChangea = normal.copy(); // start calculating the change in abgular rotation of a
            //angularVelChangea.cross(ra);
            //IaInverse.transform(angularVelChangea);
            //vector vaLinDueToR = angularVelChangea.copy().cross(ra);  // calculate the linear velocity of collision point on a due to rotation of a
            //double scalar = 1 / ma + vaLinDueToR.dot(normal);
            float scalar = body1.IsStatic ? 0 : body1.inverseMass;
            //matrix IbInverse = Ib.inverse();
            //vector angularVelChangeb = normal.copy(); // start calculating the change in abgular rotation of b
            //angularVelChangeb.cross(rb);
            //IbInverse.transform(angularVelChangeb);
            //vector vbLinDueToR = angularVelChangeb.copy().cross(rb);  // calculate the linear velocity of collision point on b due to rotation of b
            //scalar += 1 / mb + vbLinDueToR.dot(normal);
            scalar += body2.IsStatic ? 0 : body2.inverseMass;
            float Jmod = (e + 1) * (body1.linearVelocity - body2.linearVelocity).Length() / scalar;
            Vector3 J = normal * (Jmod);

            if (!treatBody1AsStatic)
            {
                body1.linearVelocity -= J * body1.inverseMass;
                body1.position -= normal * Penetration / 2;
            }
            if (!treatBody2AsStatic)
            {
                body2.linearVelocity += J * body2.inverseMass;
                body2.position += normal * Penetration / 2;
            }

            //vaf = vai - J.mul(1 / ma);
            //vbf = vbi - J.mul(1 / mb);
            //waf = wai - angularVelChangea;
            //wbf = wbi - angularVelChangeb;
        }

        public float AppliedNormalImpulse { get { return accumulatedNormalImpulse; } }
        public float AppliedTangentImpulse { get { return accumulatedTangentImpulse; } }

        /// <summary>
        /// The points in wolrd space gets recalculated by transforming the
        /// local coordinates. Also new penetration depth is estimated.
        /// </summary>
        public void UpdatePosition()
        {
            Vector3.Transform(ref realRelPos1, ref body1.orientation, out p1);
            Vector3.Add(ref p1, ref body1.position, out p1);

            Vector3.Transform(ref realRelPos2, ref body2.orientation, out p2);
            Vector3.Add(ref p2, ref body2.position, out p2);

            Vector3 dist; Vector3.Subtract(ref p1, ref p2, out dist);
            Penetration = Vector3.Dot(dist, normal);
        }

        public void ApplyImpulse(Vector3 impulse)
        {
            if (!treatBody1AsStatic)
            {
                body1.linearVelocity.X -= (impulse.X * body1.inverseMass);
                body1.linearVelocity.Y -= (impulse.Y * body1.inverseMass);
                body1.linearVelocity.Z -= (impulse.Z * body1.inverseMass);

                if (!body1.IgnoreRotation)
                {
                    float num0, num1, num2;
                    num0 = relativePos1.Y * impulse.Z - relativePos1.Z * impulse.Y;
                    num1 = relativePos1.Z * impulse.X - relativePos1.X * impulse.Z;
                    num2 = relativePos1.X * impulse.Y - relativePos1.Y * impulse.X;

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
                    num0 = relativePos2.Y * impulse.Z - relativePos2.Z * impulse.Y;
                    num1 = relativePos2.Z * impulse.X - relativePos2.X * impulse.Z;
                    num2 = relativePos2.X * impulse.Y - relativePos2.Y * impulse.X;

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
        }

        /// <summary>
        /// PrepareForIteration has to be called before <see cref="Iterate"/>.
        /// </summary>
        /// <param name="timestep">The timestep of the simulation.</param>
        public void PrepareForIteration(float timestep)
        {
            if (noCollide) return;
            float dvx, dvy, dvz;

            dvx = (body2.angularVelocity.Y * relativePos2.Z) - (body2.angularVelocity.Z * relativePos2.Y) + body2.linearVelocity.X;
            dvy = (body2.angularVelocity.Z * relativePos2.X) - (body2.angularVelocity.X * relativePos2.Z) + body2.linearVelocity.Y;
            dvz = (body2.angularVelocity.X * relativePos2.Y) - (body2.angularVelocity.Y * relativePos2.X) + body2.linearVelocity.Z;

            dvx = dvx - (body1.angularVelocity.Y * relativePos1.Z) + (body1.angularVelocity.Z * relativePos1.Y) - body1.linearVelocity.X;
            dvy = dvy - (body1.angularVelocity.Z * relativePos1.X) + (body1.angularVelocity.X * relativePos1.Z) - body1.linearVelocity.Y;
            dvz = dvz - (body1.angularVelocity.X * relativePos1.Y) + (body1.angularVelocity.Y * relativePos1.X) - body1.linearVelocity.Z;

            float kNormal = 0.0f;

            Vector3 rantra = Vector3.Zero;
            if (!treatBody1AsStatic)
            {
                kNormal += body1.inverseMass;

                // Vector3.Cross(ref relativePos1, ref normal, out rantra);
                rantra.X = (relativePos1.Y * normal.Z) - (relativePos1.Z * normal.Y);
                rantra.Y = (relativePos1.Z * normal.X) - (relativePos1.X * normal.Z);
                rantra.Z = (relativePos1.X * normal.Y) - (relativePos1.Y * normal.X);

                // Vector3.Transform(ref rantra, ref body1.invInertiaWorld, out rantra);
                float num0 = ((rantra.X * body1.invInertiaWorld.M11) + (rantra.Y * body1.invInertiaWorld.M21)) + (rantra.Z * body1.invInertiaWorld.M31);
                float num1 = ((rantra.X * body1.invInertiaWorld.M12) + (rantra.Y * body1.invInertiaWorld.M22)) + (rantra.Z * body1.invInertiaWorld.M32);
                float num2 = ((rantra.X * body1.invInertiaWorld.M13) + (rantra.Y * body1.invInertiaWorld.M23)) + (rantra.Z * body1.invInertiaWorld.M33);

                rantra.X = num0; rantra.Y = num1; rantra.Z = num2;

                //Vector3.Cross(ref rantra, ref relativePos1, out rantra);
                num0 = (rantra.Y * relativePos1.Z) - (rantra.Z * relativePos1.Y);
                num1 = (rantra.Z * relativePos1.X) - (rantra.X * relativePos1.Z);
                num2 = (rantra.X * relativePos1.Y) - (rantra.Y * relativePos1.X);

                rantra.X = num0; rantra.Y = num1; rantra.Z = num2;

            }

            Vector3 rbntrb = Vector3.Zero;
            if (!treatBody2AsStatic)
            {
                kNormal += body2.inverseMass;

                // Vector3.Cross(ref relativePos1, ref normal, out rantra);
                rbntrb.X = (relativePos2.Y * normal.Z) - (relativePos2.Z * normal.Y);
                rbntrb.Y = (relativePos2.Z * normal.X) - (relativePos2.X * normal.Z);
                rbntrb.Z = (relativePos2.X * normal.Y) - (relativePos2.Y * normal.X);

                // Vector3.Transform(ref rantra, ref body1.invInertiaWorld, out rantra);
                float num0 = ((rbntrb.X * body2.invInertiaWorld.M11) + (rbntrb.Y * body2.invInertiaWorld.M21)) + (rbntrb.Z * body2.invInertiaWorld.M31);
                float num1 = ((rbntrb.X * body2.invInertiaWorld.M12) + (rbntrb.Y * body2.invInertiaWorld.M22)) + (rbntrb.Z * body2.invInertiaWorld.M32);
                float num2 = ((rbntrb.X * body2.invInertiaWorld.M13) + (rbntrb.Y * body2.invInertiaWorld.M23)) + (rbntrb.Z * body2.invInertiaWorld.M33);

                rbntrb.X = num0; rbntrb.Y = num1; rbntrb.Z = num2;

                //Vector3.Cross(ref rantra, ref relativePos1, out rantra);
                num0 = (rbntrb.Y * relativePos2.Z) - (rbntrb.Z * relativePos2.Y);
                num1 = (rbntrb.Z * relativePos2.X) - (rbntrb.X * relativePos2.Z);
                num2 = (rbntrb.X * relativePos2.Y) - (rbntrb.Y * relativePos2.X);

                rbntrb.X = num0; rbntrb.Y = num1; rbntrb.Z = num2;

            }

            if (!treatBody1AsStatic) kNormal += rantra.X * normal.X + rantra.Y * normal.Y + rantra.Z * normal.Z;
            if (!treatBody2AsStatic) kNormal += rbntrb.X * normal.X + rbntrb.Y * normal.Y + rbntrb.Z * normal.Z;

            massNormal = 1.0f / kNormal;

            float num = dvx * normal.X + dvy * normal.Y + dvz * normal.Z;

            tangent.X = dvx - normal.X * num;
            tangent.Y = dvy - normal.Y * num;
            tangent.Z = dvz - normal.Z * num;

            num = tangent.X * tangent.X + tangent.Y * tangent.Y + tangent.Z * tangent.Z;

            if (num != 0.0f)
            {
                num = (float)Math.Sqrt(num);
                tangent.X /= num;
                tangent.Y /= num;
                tangent.Z /= num;
            }

            float kTangent = 0.0f;

            if (treatBody1AsStatic) rantra = new Vector3();
            else
            {
                kTangent += body1.inverseMass;

                // Vector3.Cross(ref relativePos1, ref normal, out rantra);
                rantra.X = (relativePos1.Y * tangent.Z) - (relativePos1.Z * tangent.Y);
                rantra.Y = (relativePos1.Z * tangent.X) - (relativePos1.X * tangent.Z);
                rantra.Z = (relativePos1.X * tangent.Y) - (relativePos1.Y * tangent.X);

                // Vector3.Transform(ref rantra, ref body1.invInertiaWorld, out rantra);
                float num0 = ((rantra.X * body1.invInertiaWorld.M11) + (rantra.Y * body1.invInertiaWorld.M21)) + (rantra.Z * body1.invInertiaWorld.M31);
                float num1 = ((rantra.X * body1.invInertiaWorld.M12) + (rantra.Y * body1.invInertiaWorld.M22)) + (rantra.Z * body1.invInertiaWorld.M32);
                float num2 = ((rantra.X * body1.invInertiaWorld.M13) + (rantra.Y * body1.invInertiaWorld.M23)) + (rantra.Z * body1.invInertiaWorld.M33);

                rantra.X = num0; rantra.Y = num1; rantra.Z = num2;

                //Vector3.Cross(ref rantra, ref relativePos1, out rantra);
                num0 = (rantra.Y * relativePos1.Z) - (rantra.Z * relativePos1.Y);
                num1 = (rantra.Z * relativePos1.X) - (rantra.X * relativePos1.Z);
                num2 = (rantra.X * relativePos1.Y) - (rantra.Y * relativePos1.X);

                rantra.X = num0; rantra.Y = num1; rantra.Z = num2;


            }

            if (treatBody2AsStatic) rbntrb = new Vector3();
            else
            {
                kTangent += body2.inverseMass;

                // Vector3.Cross(ref relativePos1, ref normal, out rantra);
                rbntrb.X = (relativePos2.Y * tangent.Z) - (relativePos2.Z * tangent.Y);
                rbntrb.Y = (relativePos2.Z * tangent.X) - (relativePos2.X * tangent.Z);
                rbntrb.Z = (relativePos2.X * tangent.Y) - (relativePos2.Y * tangent.X);

                // Vector3.Transform(ref rantra, ref body1.invInertiaWorld, out rantra);
                float num0 = ((rbntrb.X * body2.invInertiaWorld.M11) + (rbntrb.Y * body2.invInertiaWorld.M21)) + (rbntrb.Z * body2.invInertiaWorld.M31);
                float num1 = ((rbntrb.X * body2.invInertiaWorld.M12) + (rbntrb.Y * body2.invInertiaWorld.M22)) + (rbntrb.Z * body2.invInertiaWorld.M32);
                float num2 = ((rbntrb.X * body2.invInertiaWorld.M13) + (rbntrb.Y * body2.invInertiaWorld.M23)) + (rbntrb.Z * body2.invInertiaWorld.M33);

                rbntrb.X = num0; rbntrb.Y = num1; rbntrb.Z = num2;

                //Vector3.Cross(ref rantra, ref relativePos1, out rantra);
                num0 = (rbntrb.Y * relativePos2.Z) - (rbntrb.Z * relativePos2.Y);
                num1 = (rbntrb.Z * relativePos2.X) - (rbntrb.X * relativePos2.Z);
                num2 = (rbntrb.X * relativePos2.Y) - (rbntrb.Y * relativePos2.X);

                rbntrb.X = num0; rbntrb.Y = num1; rbntrb.Z = num2;

            }

            if (!treatBody1AsStatic) kTangent += Vector3.Dot(rantra, tangent);
            if (!treatBody2AsStatic) kTangent += Vector3.Dot(rbntrb, tangent);
            massTangent = 1.0f / kTangent;

            restitutionBias = lostSpeculativeBounce;

            speculativeVelocity = 0.0f;

            float relNormalVel = normal.X * dvx + normal.Y * dvy + normal.Z * dvz; //Vector3.Dot(ref normal, ref dv);

            if (Penetration > settings.allowedPenetration)
            {
                restitutionBias = settings.bias * (1.0f / timestep) * JMath.Max(0.0f, Penetration - settings.allowedPenetration);
                restitutionBias = JMath.Clamp(restitutionBias, 0.0f, settings.maximumBias);
                //  body1IsMassPoint = body2IsMassPoint = false;
            }


            float timeStepRatio = timestep / lastTimeStep;
            accumulatedNormalImpulse *= timeStepRatio;
            accumulatedTangentImpulse *= timeStepRatio;

            {
                // Static/Dynamic friction
                float relTangentVel = -(tangent.X * dvx + tangent.Y * dvy + tangent.Z * dvz);
                float tangentImpulse = massTangent * relTangentVel;
                float maxTangentImpulse = -staticFriction * accumulatedNormalImpulse;

                if (tangentImpulse < maxTangentImpulse) friction = dynamicFriction;
                else friction = staticFriction;
            }

            Vector3 impulse;

            // Simultaneos solving and restitution is simply not possible
            // so fake it a bit by just applying restitution impulse when there
            // is a new contact.
            if (relNormalVel < -1.0f && newContact)
            {
                restitutionBias = Math.Max(-Restitution * relNormalVel, restitutionBias);
            }

            // Speculative Contacts!
            // if the penetration is negative (which means the bodies are not already in contact, but they will
            // be in the future) we store the current bounce bias in the variable 'lostSpeculativeBounce'
            // and apply it the next frame, when the speculative contact was already solved.
            if (Penetration < -settings.allowedPenetration)
            {
                speculativeVelocity = Penetration / timestep;

                lostSpeculativeBounce = restitutionBias;
                restitutionBias = 0.0f;
            }
            else
            {
                lostSpeculativeBounce = 0.0f;
            }

            impulse.X = normal.X * accumulatedNormalImpulse + tangent.X * accumulatedTangentImpulse;
            impulse.Y = normal.Y * accumulatedNormalImpulse + tangent.Y * accumulatedTangentImpulse;
            impulse.Z = normal.Z * accumulatedNormalImpulse + tangent.Z * accumulatedTangentImpulse;

            ApplyImpulse(impulse);

            lastTimeStep = timestep;

            newContact = false;
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
        public void Initialize(CollisionSystem system, GameObject body1, GameObject body2, ref Vector3 point1, ref Vector3 point2, ref Vector3 n,
            float penetration, bool newContact, ContactSettings settings)
        {
            this.system = system;
            this.body1 = body1; this.body2 = body2;
            this.normal = n; normal.Normalize();
            this.p1 = point1; this.p2 = point2;

            this.newContact = newContact;

            Vector3.Subtract(ref p1, ref body1.position, out relativePos1);
            Vector3.Subtract(ref p2, ref body2.position, out relativePos2);
            Vector3.Transform(ref relativePos1, ref body1.invOrientation, out realRelPos1);
            Vector3.Transform(ref relativePos2, ref body2.invOrientation, out realRelPos2);

            this.initialPen = penetration;
            this.Penetration = penetration;

            // Material Properties
            if (newContact)
            {
                noCollide = body1.NoCollide || body2.NoCollide;
                treatBody1AsStatic = body1.IsStatic;
                treatBody2AsStatic = body2.IsStatic;

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
