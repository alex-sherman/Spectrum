using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Spectrum.Framework.Audio;
using Spectrum.Framework.Content;
using Spectrum.Framework.Graphics.Animation;
using Replicate;
using TwoMGFX;
using Spectrum.Framework.Physics;

namespace Spectrum.Framework.Entities
{
    public class GameObject : Entity, IDebug, IEquatable<GameObject>, IComparable<GameObject>, ICollidable, IAnimationSource, ITransform
    {
        #region Events
        /// <summary>
        /// Hit game object, position, normal, penetration
        /// </summary>
        public event Action<GameObject, Vector3, Vector3, float> OnCollide;
        public event Action<GameObject> OnEndCollide;
        #endregion

        #region Physics Fields/Properties
        [Flags]
        public enum DampingType { None = 0x00, Angular = 0x01, Linear = 0x02 }

        public Matrix inertia;
        public Matrix invInertia;

        public Matrix inertiaWorld;
        public Matrix invInertiaWorld;
        public Vector3 inertiaOrigin;
        public Quaternion orientation;
        [Replicate]
        public Quaternion Orientation
        {
            get { return orientation; }
            set { dirtyPhysics = true; orientation = value; }
        }
        public Quaternion invOrientation;
        public Quaternion InvOrientation { get { return invOrientation; } }
        public Vector3 position;
        [Replicate]
        public Vector3 Position
        {
            get { return position; }
            set { dirtyPhysics = true; position = value; }
        }
        Vector3 ITransform.Scale => Vector3.One;
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

        public bool IsActive = true;
        public bool AllowDeactivation;

        /// <summary>
        /// Static objects cannot move, but may still collide
        /// </summary>
        public bool IsStatic
        {
            get => isStatic;
            set
            {
                isStatic = value;
                if (value) IslandManager.MakeBodyStatic(this);
            }
        }
        internal bool isStatic;

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

        internal CollisionIsland island;
        internal float inverseMass;

        public Vector3 force, torque;

        public int internalIndex = 0;

        public int marker = 0;
        private Shape shape;
        public Shape Shape
        {
            get => shape;
            set
            {
                shape = value;
                if (shape != null)
                {
                    //Set mass properties
                    inertia = Shape.inertia;
                    invInertia = inertia.Invert();
                    inverseMass = 1.0f / Shape.mass;
                }
                else
                    inertiaOrigin = Vector3.Zero;
                dirtyPhysics = true;
            }
        }
        public void ShapeFromModelBounds(bool separateParts = false)
        {
            if (Model != null)
            {
                if (!separateParts)
                    Shape = new BoxShape(ModelBounds);
                else
                {
                    var shapes = Model.MeshParts.Values.Select<DrawablePart, Shape>(p =>
                    {
                        var box = p.Bounds;
                        var t = ModelTransform.LocalWorld;
                        box.Transform(ref t);
                        return new BoxShape(box);
                    }).ToList();
                    Shape = new ListMultishape(shapes);
                }
            }
        }
        public virtual bool Collide(GameObject other, Vector3 point, Vector3 normal, float penetration)
        {
            OnCollide?.Invoke(other, point, normal, penetration);
            return true;
        }
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
        bool _useFixedRender = false;
        public virtual bool UseFixedRender
        {
            get => _useFixedRender;
            set
            {
                if (IsInitialized)
                {
                    if (value && !UseFixedRender)
                        RegisterDraws();
                    if (!value && UseFixedRender)
                        UnregisterDraws();
                }
                _useFixedRender = value;
            }
        }
        public virtual IEnumerable<RenderCallKey> GetFixedRenderCalls()
        {
            return Model?.MeshParts.Values.Select(part => Batch3D.Current.RegisterDraw(part, World, Material, options: DrawOptions));
        }
        public override void Reload()
        {
            base.Reload();
            if (UseFixedRender)
            {
                UnregisterDraws();
                RegisterDraws();
            }
        }
        public virtual void RegisterDraws()
        {
            // TODO: Support disable instance here
            if (FixedRenderKeys != null)
                UnregisterDraws();
            FixedRenderKeys = GetFixedRenderCalls()?.ToList();
        }
        public virtual void UnregisterDraws()
        {
            if (FixedRenderKeys != null)
                foreach (var renderKey in FixedRenderKeys)
                    Batch3D.Current.UnregisterDraw(renderKey);
            FixedRenderKeys = null;
        }
        private List<RenderCallKey> FixedRenderKeys = null;

        /// <summary>
        /// The world matrix for the purposes of drawing the game object
        /// </summary>
        public Matrix World => orientation.ToMatrix() * Matrix.CreateTranslation(position);
        [Replicate]
        public MaterialData Material;
        [Replicate]
        public Texture2D Texture { get => Material?.DiffuseTexture; set => (Material ?? (Material = new MaterialData())).DiffuseTexture = value; }
        public Batch3D.DrawOptions DrawOptions;
        [Replicate]
        public SpecModel Model;
        public JBBox ModelBounds
        {
            get
            {
                if (Model == null)
                    return new JBBox();
                var output = Model.Bounds;
                var t = ModelTransform.LocalWorld;
                output.Transform(ref t);
                return output;
            }
        }

        [Replicate]
        public Transform ModelTransform;
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
            inertia = new Matrix();
            invInertia = invInertiaWorld = new Matrix();
            invOrientation = orientation = Quaternion.Identity;
            inverseMass = 1.0f;
            material = new Material();
            ModelTransform = new Transform() { Parent = this };
        }

        public override void Initialize()
        {
            base.Initialize();
            if (!isStatic && ReplicationData != null)
            {
                ReplicationData.SetInterpolator<Vector3>("Position", (w, current, target) => Vector3.Lerp(current, target, w));
                //ReplicationData.SetInterpolator<Quaternion>("Orientation", (w, current, target) => Quaternion.Slerp(current, target, w));
            }
            if (UseFixedRender)
                RegisterDraws();
            PhysicsUpdate(0);
        }

        #region Physics Functions
        protected bool dirtyPhysics = true;
        public void PhysicsUpdate(float timestep)
        {
            if (!isStatic || dirtyPhysics)
            {
                dirtyPhysics = false;
                //TODO: This might be useful to cache on the object if its needed elsewhere
                // or it should just get removed and figure out how to do everything with quaternions
                Matrix orientationMat = orientation.ToMatrix();
                invOrientation = orientation.Inverse();
                Matrix invOrientationMat = invOrientation.ToMatrix();

                if (Shape != null)
                {
                    // Given: Orientation, Inertia
                    Shape.GetBoundingBox(ref orientationMat, out boundingBox);
                    boundingBox.Min += position;
                    boundingBox.Min = Vector3.Min(boundingBox.Min, boundingBox.Min + linearVelocity * timestep);
                    boundingBox.Max += position;
                    boundingBox.Max = Vector3.Max(boundingBox.Max, boundingBox.Max + linearVelocity * timestep);
                    inertiaOrigin = orientation * Shape.geomCen;
                }
                invInertiaWorld = invOrientationMat * invInertia * orientationMat;
                inertiaWorld = inertia * orientationMat;
            }
        }
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
        public override void Update(float dt)
        {
            base.Update(dt);
            AnimationPlayer?.Update(dt);
            if (SoundEmitter != null)
                SoundEmitter.Update(this);
            if (Model != null) { Model.Update(dt); }
        }
        public override void Draw(float dt)
        {
            base.Draw(dt);
            if (!UseFixedRender && Model != null)
            {
                foreach (var part in Model)
                {
                    Batch3D.Current.DrawPart(part, ModelTransform.LocalWorld * World, Material, options: DrawOptions);
                }
            }
        }
        public override void Destroy()
        {
            DebugPrinter.undisplay(this);
            UnregisterDraws();
            base.Destroy();
        }

        public void DebugDraw(float gameTime)
        {
            if (Shape != null)
            {
                JBBox boundingBox;
                Matrix orientationMat = orientation.ToMatrix();
                Shape.GetBoundingBox(ref orientationMat, out boundingBox);
                boundingBox.Min += position;
                boundingBox.Max += position;
                Batch3D.Current.DrawJBBox(boundingBox, Color.Black);
                //GraphicsEngine.DrawCircle(position, 3, Color.Red, spriteBatch);
                if (!isStatic)
                {
                    Batch3D.Current.DrawLine(position, position + Velocity * 1 / 60f * 10, Color.Blue);
                    Batch3D.Current.DrawLine(position, position + angularVelocity, Color.Red);
                    foreach (var arbiter in arbiters.Where(arb => !arb.Body1.NoCollide && !arb.Body2.NoCollide))
                    {
                        foreach (var contact in arbiter.ContactList)
                        {
                            var body1 = contact.body1 == this;
                            var position = body1 ? contact.Position1 : contact.Position2;
                            var normal = contact.normal * (body1 ? 1 : -1);
                            //GraphicsEngine.DrawCircle(myPosition, 3, Color.Yellow, SpectrumGame.Game.Root.SpriteBatch);
                            //GraphicsEngine.DrawCircle(otherPosition, 3, Color.HotPink, SpectrumGame.Game.Root.SpriteBatch);
                            Batch3D.Current.DrawLine(position, position - normal * contact.Penetration, "orange");
                            Batch3D.Current.DrawLine(position, position - normal * contact.accumulatedNormalImpulse, Color.Green);
                            Batch3D.Current.DrawLine(position, position + (body1 ? contact.lastAngularBody1 : contact.lastAngularBody2) * 100, "pink");
                        }
                    }
                }
            }
        }
    }
}
