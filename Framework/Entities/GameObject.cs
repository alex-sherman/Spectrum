﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spectrum.Framework;
using Spectrum.Framework.Physics;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Graphics;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Network;
using Spectrum.Framework.Physics.Dynamics;
using Spectrum.Framework.Physics.Collision.Shapes;
using Spectrum.Framework.Physics.Collision;
using Spectrum.Framework.Physics.LinearMath;
using Spectrum.Framework.Physics.Dynamics.Constraints;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Audio;
using Spectrum.Framework.Content;
using Spectrum.Framework.Graphics.Animation;

namespace Spectrum.Framework.Entities
{
    public enum BodyType
    {
        Dynamic,
        Static,
        Ignore
    }
    public class GameObject : Entity, IDebug, IEquatable<GameObject>, IComparable<GameObject>, ICollidable, IAnimationSource
    {
        #region Events
        public event Action<GameObject, Vector3, Vector3, float> OnCollide;
        public event Action<GameObject> OnEndCollide;
        #endregion

        #region Physics Fields/Properties
        [Flags]
        public enum DampingType { None = 0x00, Angular = 0x01, Linear = 0x02 }

        public Matrix inertia;
        public Matrix invInertia;

        public Matrix invInertiaWorld;
        public Quaternion orientation;
        [Replicate]
        public Quaternion Orientation
        {
            get { return orientation; }
            set { _dirtyPhysics = true; orientation = value; }
        }
        public Quaternion invOrientation;
        public Quaternion InvOrientation { get { return invOrientation; } }
        public Vector3 position;
        [Replicate]
        public Vector3 Position
        {
            get { return position; }
            set { _dirtyPhysics = true; position = value; }
        }
        public object PositionInterpolator(float w, object value)
        {
            return null;
        }
        [Replicate]
        public Vector3 Velocity
        {
            get { return linearVelocity; }
            set { linearVelocity = value; }
        }
        public Vector3 linearVelocity;
        public Vector3 angularVelocity;
        public bool IgnoreRotation = false;

        public Material material;

        public JBBox boundingBox;
        public JBBox BoundingBox { get { return boundingBox; } }

        public float inactiveTime = 0.0f;

        public bool IsActive;
        public bool AllowDeactivation;

        /// <summary>
        /// Static objects cannot move, but may still collide
        /// </summary>
        public bool IsStatic;

        /// <summary>
        /// Ignore objects aren't affected by physics at all
        /// </summary>
        public bool Ignore;

        /// <summary>
        /// Disables collision handling for the object, the OnCollide event is still called
        /// </summary>
        public bool NoCollide;

        public bool affectedByGravity = true;

        private DampingType damping = DampingType.Angular | DampingType.Linear;
        public DampingType Damping { get { return damping; } set { damping = value; } }

        internal Vector3 sweptDirection = Vector3.Zero;
        public Vector3 SweptDirection { get { return sweptDirection; } }
        public void SweptExpandBoundingBox(float timestep)
        {
            sweptDirection = linearVelocity * timestep;

            if (sweptDirection.X < 0.0f)
            {
                boundingBox.Min.X += sweptDirection.X;
            }
            else
            {
                boundingBox.Max.X += sweptDirection.X;
            }

            if (sweptDirection.Y < 0.0f)
            {
                boundingBox.Min.Y += sweptDirection.Y;
            }
            else
            {
                boundingBox.Max.Y += sweptDirection.Y;
            }

            if (sweptDirection.Z < 0.0f)
            {
                boundingBox.Min.Z += sweptDirection.Z;
            }
            else
            {
                boundingBox.Max.Z += sweptDirection.Z;
            }
        }

        public CollisionIsland island;
        public float inverseMass;

        public Vector3 force, torque;

        public int internalIndex = 0;

        public int marker = 0;
        private Shape shape;
        public Shape Shape { get => shape; set { _dirtyPhysics = true; shape = value; } }
        public void ShapeFromModelBounds()
        {
            if (Model != null)
            {
                JBBox box = ModelBounds;
                box.Transform(ref ModelTransform);
                Shape = new BoxShape((box.Max - box.Min), box.Center);
            }
        }
        public virtual void Collide(GameObject other, Vector3 point, Vector3 normal, float penetration) => OnCollide?.Invoke(other, point, normal, penetration);
        public virtual void EndCollide(GameObject other) => OnEndCollide?.Invoke(other);




        public bool EnableSpeculativeContacts { get; set; }

        public List<GameObject> connections = new List<GameObject>();
        public HashSet<Arbiter> arbiters = new HashSet<Arbiter>();
        public HashSet<Constraint> constraints = new HashSet<Constraint>();

        #endregion

        #region DrawingStuff
        /// <summary>
        /// Uses fixed render tasks which are only updated on creation/destruction and DrawEnabled changes.
        /// Fixed render tasks are much faster during draw calls but cannot have changing shader properties.
        /// If those updates happen frequently it will degrade performance of other fixed render tasks.
        /// </summary>
        public bool UseFixedRender = false;
        public RenderCallKey FixedRenderKey;

        /// <summary>
        /// The world matrix for the purposes of drawing the game object
        /// </summary>
        public Matrix World => ModelTransform * Matrix.CreateFromQuaternion(orientation) * Matrix.CreateTranslation(position);
        public MaterialData Material;
        public Texture2D Texture { get => Material?.DiffuseTexture; set => (Material ?? (Material = new MaterialData())).DiffuseTexture = value; }
        public bool DisableInstancing;
        public bool DisableDepthBuffer;
        public SpecModel Model;
        public JBBox ModelBounds
        {
            get
            {
                JBBox output = new JBBox(Vector3.Zero, Vector3.Zero);
                if (Model == null) return output;
                foreach (var part in Model)
                {
                    output.AddPoint(part.Bounds.Min);
                    output.AddPoint(part.Bounds.Max);
                }
                return output;
            }
        }

        [Replicate]
        public Matrix ModelTransform = Matrix.Identity;
        // TODO: Should be a component
        public AnimationPlayer AnimationPlayer;
        public AnimationClip GetAnimation(string name)
        {
            return Model?.Animations[name] ?? Animations?[name];
        }
        public SkinningData GetSkinningData()
        {
            return Model?.SkinningData;
        }
        public AnimationData Animations;
        #endregion

        public GameObject()
            : base()
        {
            Model = null;
            IsActive = true;
            inertia = new Matrix();
            invInertia = invInertiaWorld = new Matrix();
            invOrientation = orientation = Quaternion.Identity;
            inverseMass = 1.0f;
            material = new Material();
        }

        public override void Initialize()
        {
            base.Initialize();
            if (!IsStatic)
            {
                ReplicationData.SetInterpolator<Vector3>("Position", (w, current, target) => Vector3.Lerp(current, target, w));
                ReplicationData.SetInterpolator<Quaternion>("Orientation", (w, current, target) => Quaternion.Slerp(current, target, w));
            }
            PhysicsUpdate(0);
            if (UseFixedRender)
                foreach (var part in Model)
                    // TODO: Support disable instance here
                    Manager.RegisterDraw(part, World, Material, disableDepthBuffer: DisableDepthBuffer);/*, disableInstancing: DisableInstancing);*/
        }

        #region Physics Functions
        private bool _dirtyPhysics = true;
        public void PhysicsUpdate(float timestep)
        {
            if (!IsStatic || _dirtyPhysics)
            {
                _dirtyPhysics = false;
                //TODO: This might be useful to cache on the object if its needed elsewhere
                // or it should just get removed and figure out how to do everything with quaternions
                Matrix orientationMat = Matrix.CreateFromQuaternion(orientation);
                Quaternion.Inverse(ref orientation, out invOrientation);
                Matrix invOrientationMat = Matrix.CreateFromQuaternion(invOrientation);

                if (Shape != null)
                {
                    //Set mass properties
                    inertia = Shape.inertia;
                    Matrix.Invert(ref inertia, out invInertia);
                    inverseMass = 1.0f / Shape.mass;

                    // Given: Orientation, Inertia
                    Shape.GetBoundingBox(ref orientationMat, out boundingBox);
                    Vector3.Add(ref boundingBox.Min, ref position, out boundingBox.Min);
                    boundingBox.Min = Vector3.Min(boundingBox.Min, boundingBox.Min + linearVelocity * timestep);
                    Vector3.Add(ref boundingBox.Max, ref position, out boundingBox.Max);
                    boundingBox.Max = Vector3.Max(boundingBox.Max, boundingBox.Max + linearVelocity * timestep);
                }
                Matrix.Multiply(ref invOrientationMat, ref invInertia, out invInertiaWorld);
                Matrix.Multiply(ref invInertiaWorld, ref orientationMat, out invInertiaWorld);
            }
        }
        public virtual void PreStep(float step) { }
        public virtual void PostStep(float step) { }
        #endregion
        // TODO: Should be a component
        protected SoundEmitter SoundEmitter;

        public bool Equals(GameObject other)
        {
            return (other.ID == this.ID);
        }

        public int CompareTo(GameObject other)
        {
            return other.ID.CompareTo(other.ID);
        }

        public virtual string Debug()
        {
            string output = this.GetType().ToString() + ": " + Position.X.ToString("000.00") + " " + Position.Y.ToString("000.00") + " " + Position.Z.ToString("000.00");
            foreach (Arbiter arbiter in arbiters)
            {
                if (arbiter.ContactList.Count > 0)
                {
                    output += "\n" + arbiter.ContactList[0].Penetration.ToString("0.00") + " " + arbiter.ContactList[0].Normal;
                }
            }
            return output;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            AnimationPlayer?.Update(gameTime.DT());
            if (SoundEmitter != null)
                SoundEmitter.Update(this);
            if (Model != null) { Model.Update(gameTime); }
        }
        public override void Draw(float gameTime)
        {
            base.Draw(gameTime);
            if (!UseFixedRender && Model != null)
            {
                foreach (var part in Model)
                {
                    Manager.DrawPart(part, World, Material, disableDepthBuffer: DisableDepthBuffer, disableInstancing: DisableInstancing);
                }
            }
        }
        public override void Destroy()
        {
            DebugPrinter.undisplay(this);
            if (UseFixedRender)
                Manager.UnregisterDraw(FixedRenderKey);
            base.Destroy();
        }

        public void DebugDraw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (Shape != null)
            {
                JBBox boundingBox;
                Matrix orientationMat = Matrix.CreateFromQuaternion(orientation);
                Shape.GetBoundingBox(ref orientationMat, out boundingBox);
                Vector3.Add(ref boundingBox.Min, ref position, out boundingBox.Min);
                Vector3.Add(ref boundingBox.Max, ref position, out boundingBox.Max);
                GraphicsEngine.DrawJBBox(boundingBox, Color.Black, spriteBatch);
                GraphicsEngine.DrawCircle(position, 3, Color.Red, spriteBatch);
                GraphicsEngine.DrawLine(position, position + Velocity * 1 / 60f * 10, Color.Blue, spriteBatch);
                foreach (var arbiter in this.arbiters)
                {
                    foreach (var contact in arbiter.contactList)
                    {
                        GraphicsEngine.DrawCircle(contact.Position1, 3, Color.Yellow, spriteBatch);
                        GraphicsEngine.DrawCircle(contact.Position2, 3, Color.HotPink, spriteBatch);
                        GraphicsEngine.DrawLine(contact.Position1, contact.Position1 - contact.normal * contact.Penetration, contact.Penetration < 0 ? Color.Red : Color.Blue, spriteBatch);
                        GraphicsEngine.DrawLine(contact.Position1, contact.Position1 + contact.normal * contact.accumulatedNormalImpulse, Color.Green, spriteBatch);
                        GraphicsEngine.DrawLine(contact.Position1, contact.Position1 + contact.tangent * contact.accumulatedTangentImpulse, Color.Red, spriteBatch);
                    }
                }
            }
        }
    }
}
