using System;
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

namespace Spectrum.Framework.Entities
{
    public enum BodyType
    {
        Dynamic,
        Static,
        Ignore
    }
    public class GameObject : Entity, IDebug, IEquatable<GameObject>, IComparable<GameObject>, ICollidable
    {
        //This decides whether the game object will despawn if it goes out of range of the heightmap
        protected bool required = false;
        private bool _canMove = true;
        public virtual bool CanMove
        {
            get { return _canMove; }
            protected set { _canMove = value; }
        }

        #region Physics Fields/Properties
        [Flags]
        public enum DampingType { None = 0x00, Angular = 0x01, Linear = 0x02 }

        public Matrix inertia;
        public Matrix invInertia;

        public Matrix invInertiaWorld;
        public Matrix orientation;
        [Replicate]
        public Matrix Orientation
        {
            get { return orientation; }
            set { orientation = value; PhysicsUpdate(); }
        }
        public Matrix invOrientation;
        public Matrix InvOrientation { get { return invOrientation; } }
        public Vector3 position;
        [Replicate]
        public Vector3 Position
        {
            get { return position; }
            set { position = value; PhysicsUpdate(); }
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

        private JBBox boundingBox;
        public JBBox BoundingBox { get { return boundingBox; } }

        public float inactiveTime = 0.0f;

        public bool IsActive;
        public bool AllowDeactivation;

        /// <summary>
        /// Static objects cannot move, but may still collide
        /// </summary>
        public bool IsStatic { get; protected set; }

        /// <summary>
        /// Ignore objects aren't affected by physics at all
        /// </summary>
        public bool Ignore { get; protected set; }

        /// <summary>
        /// Disables collision handling for the object, the OnCollide event is still called
        /// </summary>
        public bool NoCollide { get; protected set; }

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

        private ShapeUpdatedHandler updatedHandler;

        public int marker = 0;
        private Shape shape;
        public Shape Shape
        {
            get { return shape; }
            set
            {
                // deregister update event
                if (shape != null) shape.ShapeUpdated -= updatedHandler;

                // register new event
                shape = value;
                shape.ShapeUpdated += updatedHandler;
                PhysicsUpdate();
            }
        }
        public virtual void OnCollide(GameObject other, Vector3 normal) { }
        public virtual void OnEndCollide(GameObject other) { }


        /// <summary>
        /// The current oriention of the body.
        /// </summary>

        public Matrix World
        {
            get { return orientation * Matrix.CreateTranslation(position); }
        }


        public bool EnableSpeculativeContacts { get; set; }

        public List<GameObject> connections = new List<GameObject>();
        public HashSet<Arbiter> arbiters = new HashSet<Arbiter>();
        public HashSet<Constraint> constraints = new HashSet<Constraint>();

        #endregion

        public List<DrawablePart> Parts;
        public SpecModel Model { get { return Parts as SpecModel; } }

        public GameObject()
            : base()
        {
            this.Parts = new List<DrawablePart>();
            IsActive = true;

            updatedHandler = new ShapeUpdatedHandler(PhysicsUpdate);

            orientation = Matrix.Identity;
            inertia = new Matrix();
            invInertia = this.invInertiaWorld = new Matrix();
            invOrientation = this.orientation = Matrix.Identity;
            inverseMass = 1.0f;
            material = new Material();
            Shape = new BoxShape(1, 1, 1);
            PhysicsUpdate();
        }

        #region Physics Functions
        public void PhysicsUpdate()
        {
            //Set mass properties
            this.inertia = Shape.inertia;
            Matrix.Invert(ref inertia, out invInertia);
            this.inverseMass = 1.0f / Shape.mass;

            // Given: Orientation, Inertia
            Matrix.Transpose(ref orientation, out invOrientation);
            Shape.GetBoundingBox(ref orientation, out boundingBox);
            Vector3.Add(ref boundingBox.Min, ref this.position, out boundingBox.Min);
            Vector3.Add(ref boundingBox.Max, ref this.position, out boundingBox.Max);


            if (!IsStatic)
            {
                Matrix.Multiply(ref invOrientation, ref invInertia, out invInertiaWorld);
                Matrix.Multiply(ref invInertiaWorld, ref orientation, out invInertiaWorld);
            }
            this.Replicate();
        }
        public virtual void PreStep(float step) { }
        public virtual void PostStep(float step) { }
        #endregion

        protected List<SoundEffect> Sounds = new List<SoundEffect>();
        protected Emitter Emitter = new Emitter();
        public void PlaySound(SoundEffect sound)
        {
            Emitter.RegisterSoundEffect(sound);
            Emitter.Update();
            sound.Play();
        }

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
                    output += "\n" + arbiter.ContactList[0].Penetration.ToString("0.00") + " " + arbiter.ContactList[0].slip.ToString("0.00") + " " + arbiter.ContactList[0].Normal;
                }
            }
            return output;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Emitter.Position = position;
            Emitter.Up = Vector3.Up;
            Emitter.Forward = Vector3.Forward;
            if (Model != null) { Model.Update(gameTime); }
            Emitter.Update();
        }
        public override void Dispose()
        {
            DebugPrinter.undisplay(this);
            base.Dispose();
        }

        public void DebugDraw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            JBBox boundingBox;
            shape.GetBoundingBox(ref orientation, out boundingBox);
            Vector3.Add(ref boundingBox.Min, ref position, out boundingBox.Min);
            Vector3.Add(ref boundingBox.Max, ref position, out boundingBox.Max);
            GraphicsEngine.DrawJBBox(boundingBox, Color.Black, spriteBatch);
        }
    }
}
