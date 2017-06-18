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
            set { orientation = value; }
        }
        public Matrix invOrientation;
        public Matrix InvOrientation { get { return invOrientation; } }
        public Vector3 position;
        [Replicate]
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
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

        public int marker = 0;
        public Shape Shape { get; set; }
        public void ShapeFromModelBounds()
        {
            JBBox box = ModelBounds;
            box.Transform(ref ModelTransform);
            Shape = new BoxShape((box.Max - box.Min), box.Center);
        }
        public virtual void OnCollide(GameObject other, Vector3 point, Vector3 normal, float penetration) { }
        public virtual void OnEndCollide(GameObject other) { }


        /// <summary>
        /// The current oriention of the body.
        /// </summary>
        public Matrix World
        {
            get { return ModelTransform * orientation * Matrix.CreateTranslation(position); }
        }


        public bool EnableSpeculativeContacts { get; set; }

        public List<GameObject> connections = new List<GameObject>();
        public HashSet<Arbiter> arbiters = new HashSet<Arbiter>();
        public HashSet<Constraint> constraints = new HashSet<Constraint>();

        #endregion

        private List<RenderTask> _tasks = null;
        private SpecModel _parts = null;
        public SpecModel Model
        {
            get { return _parts; }
            set
            {
                _parts = value;
                _tasks = Model?.Select((part) => new RenderTask(part, TypeName) { world = World }).ToList();
            }
        }
        public JBBox ModelBounds
        {
            get
            {
                JBBox output = JBBox.SmallBox;
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

        public GameObject()
            : base()
        {
            this.Model = null;
            IsActive = true;
            AnimationPlayer = new AnimationPlayer(this);
            orientation = Matrix.Identity;
            inertia = new Matrix();
            invInertia = this.invInertiaWorld = new Matrix();
            invOrientation = this.orientation = Matrix.Identity;
            inverseMass = 1.0f;
            material = new Material();
        }

        public override void Initialize()
        {
            base.Initialize();
            ReplicationData.SetInterpolator("Position", (w, current, target) => Vector3.Lerp((Vector3)current, (Vector3)target, w));
            ReplicationData.SetInterpolator("Orientation", (w, current, target) => Matrix.CreateFromQuaternion(Quaternion.Slerp(Quaternion.CreateFromRotationMatrix((Matrix)current),
                Quaternion.CreateFromRotationMatrix((Matrix)target), w)));
        }

        #region Physics Functions
        public void PhysicsUpdate(float timestep)
        {
            if (Shape != null)
            {
                //Set mass properties
                this.inertia = Shape.inertia;
                Matrix.Invert(ref inertia, out invInertia);
                this.inverseMass = 1.0f / Shape.mass;

                // Given: Orientation, Inertia
                Matrix.Transpose(ref orientation, out invOrientation);
                Shape.GetBoundingBox(ref orientation, out boundingBox);
                Vector3.Add(ref boundingBox.Min, ref this.position, out boundingBox.Min);
                boundingBox.Min = Vector3.Min(boundingBox.Min, boundingBox.Min + linearVelocity * timestep);
                Vector3.Add(ref boundingBox.Max, ref this.position, out boundingBox.Max);
                boundingBox.Max = Vector3.Max(boundingBox.Max, boundingBox.Max + linearVelocity * timestep);
            }


            if (!IsStatic)
            {
                Matrix.Multiply(ref invOrientation, ref invInertia, out invInertiaWorld);
                Matrix.Multiply(ref invInertiaWorld, ref orientation, out invInertiaWorld);
            }
        }
        public virtual void PreStep(float step)
        {
            PhysicsUpdate(step);
        }
        public virtual void PostStep(float step) { }
        #endregion

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
                    output += "\n" + arbiter.ContactList[0].Penetration.ToString("0.00") + " " + arbiter.ContactList[0].Normal;
                }
            }
            return output;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            AnimationPlayer.Update(gameTime.DT());
            Emitter.Position = position;
            Emitter.Up = Vector3.Up;
            Emitter.Forward = Vector3.Forward;
            if (Model != null) { Model.Update(gameTime); }
            Emitter.Update();
        }
        private Matrix _lastWorld = Matrix.Identity;
        public virtual List<RenderTask> GetRenderTasks(RenderPhaseInfo phase)
        {
            if (_tasks != null && _lastWorld != World)
            {
                _lastWorld = World;
                for (int i = 0; i < _tasks.Count; i++)
                {
                    _tasks[i].world = World;
                }
            }
            return _tasks;
        }
        public override void Dispose()
        {
            DebugPrinter.undisplay(this);
            base.Dispose();
        }

        public void DebugDraw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (Shape != null)
            {
                JBBox boundingBox;
                Shape.GetBoundingBox(ref orientation, out boundingBox);
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
