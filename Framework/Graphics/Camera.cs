using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spectrum.Framework;
using Spectrum.Framework.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Spectrum.Framework.Graphics
{

    public class Camera
    {
        public Matrix Projection;
        CullMode culling = CullMode.None;
        Color clearColor = Color.CornflowerBlue;
        float aspectRatio;
        private Vector3 _position;
        public Quaternion Rotation;
        /// TODO: Retrieve these values from rotation?
        public float Yaw
        {
            get => Rotation.Yaw();
            set { Rotation = Quaternion.CreateFromYawPitchRoll(value, Pitch, Roll); }
        }
        public double PitchLimit = 0.001f;
        public float Pitch
        {
            get => Rotation.Pitch();
            set
            {
                Rotation = Quaternion.CreateFromYawPitchRoll(Yaw, (float)Math.Min(Math.Max(value, PitchLimit - Math.PI / 2), Math.PI / 2 - PitchLimit), Roll);
            }
        }
        public float Roll => Rotation.Roll();

        public Color ClearColor
        {
            get { return clearColor; }
            set { clearColor = value; }
        }

        public CullMode Culling
        {
            get { return culling; }
            set { culling = value; }
        }

        public float AspectRatio
        {
            get { return aspectRatio; }
            set { aspectRatio = value; }
        }

        public virtual Matrix ReflectionProjection
        {
            get
            {
                return Settings.reflectionProjection;
            }
        }

        public virtual Matrix View
        {
            get
            {
                return Matrix.CreateLookAt(
                    Position,
                    Vector3.Transform(Vector3.Forward, Rotation) + Position,
                    Vector3.Transform(Vector3.Up, Rotation));
            }
        }
        public virtual Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        public virtual Matrix ReflectionView
        {
            get
            {
                Vector3 refCP = Position;
                refCP.Y = -refCP.Y + 2 * Water.waterHeight;

                Vector3 refTP = Vector3.Transform(Vector3.Forward, Rotation) + Position;
                refTP.Y = -refTP.Y + 2 * Water.waterHeight;

                Vector3 cameraRight = Vector3.Transform(new Vector3(1, 0, 0), Rotation);
                Vector3 invUpVector = Vector3.Cross(cameraRight, refTP - refCP);

                return Matrix.CreateLookAt(refCP, refTP, invUpVector);
            }
        }

        public virtual void UpdateFromVector2(Vector2 desiredRotation)
        {
            Rotation = Quaternion.CreateFromYawPitchRoll(Yaw - desiredRotation.X, (float)Math.Min(Math.Max(Pitch - desiredRotation.Y, PitchLimit - Math.PI / 2), Math.PI / 2 - PitchLimit), 0);
        }

        public Ray GetMouseRay(Point screenCoords)
        {
            Vector3 nearsource = new Vector3(screenCoords.X, screenCoords.Y, 0f);
            Vector3 farsource = new Vector3(screenCoords.X, screenCoords.Y, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            Vector3 nearPoint = SpectrumGame.Game.GraphicsDevice.Viewport.Unproject(nearsource, Projection, View, world);

            Vector3 farPoint = SpectrumGame.Game.GraphicsDevice.Viewport.Unproject(farsource, Projection, View, world);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            return new Ray(nearPoint, direction);
        }

        public Vector3 Forward => Vector3.Transform(Vector3.Forward, Rotation);
    }
}
