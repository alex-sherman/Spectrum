using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spectrum.Framework.Entities;

namespace Spectrum.Framework.Graphics
{
    public interface ICamera : ITransform
    {
        Matrix View { get; }
        Matrix Projection { get; set; }
        Matrix Transform { get; set; }
        void UpdateProjection(int width, int height);
    }
    public static class CameraExtensions
    {
        public static Vector2 Unproject(this ICamera camera, Rectangle bounds, Point point)
        {
            Vector2 vector = new Vector2
            {
                X = 2f * (point.X - bounds.X) / bounds.Width - 1,
                Y = 1 - 2f * (point.Y - bounds.Y) / bounds.Height,
            };
            Matrix inverse = (Matrix.Identity * camera.View * camera.Projection).Invert();
            var result = inverse * vector;
            float W = vector.X * inverse.M14 + vector.Y * inverse.M24 + inverse.M44;
            return result / W;
        }
    }
    public class Camera : ICamera
    {
        public Matrix Projection { get; set; } = Matrix.Identity;

        public void UpdateProjection(int width, int height)
        {
            Projection = Matrix.CreatePerspectiveFOV(
                (float)Math.PI / 3.0f,
                (float)width / height,
                0.1f, 10000);
        }
        public virtual Vector3 Position { get; set; }
        Vector3 ITransform.Position => Position;
        public Quaternion Orientation;
        Quaternion ITransform.Orientation => Orientation;
        Vector3 ITransform.Scale => Vector3.One;
        public Matrix Transform { get; set; } = Matrix.Identity;
        /// TODO: Retrieve these values from rotation?
        public float Yaw
        {
            get => Orientation.Yaw();
            set { Orientation = Quaternion.CreateFromYawPitchRoll(value, Pitch, Roll); }
        }
        public double PitchLimit = 0.001f;
        public float Pitch
        {
            get => Orientation.Pitch();
            set
            {
                Orientation = Quaternion.CreateFromYawPitchRoll(Yaw, (float)Math.Min(Math.Max(value, PitchLimit - Math.PI / 2), Math.PI / 2 - PitchLimit), Roll);
            }
        }
        public float Roll => Orientation.Roll();

        public virtual Matrix View
        {
            get
            {
                return Transform * Matrix.CreateLookAt(
                    Position,
                    Orientation * Vector3.Forward + Position,
                    Orientation * Vector3.Up);
            }
        }

        public virtual Matrix ReflectionView
        {
            get
            {
                Vector3 refCP = Position;
                refCP.Y = -refCP.Y + 2 * Water.waterHeight;

                Vector3 refTP = Orientation * Vector3.Forward + Position;
                refTP.Y = -refTP.Y + 2 * Water.waterHeight;

                Vector3 cameraRight = Orientation * Vector3.Right;
                Vector3 invUpVector = cameraRight.Cross(refTP - refCP);

                return Matrix.CreateLookAt(refCP, refTP, invUpVector);
            }
        }

        public virtual void UpdateFromVector2(Vector2 desiredRotation)
        {
            Orientation = Quaternion.CreateFromYawPitchRoll(Yaw - desiredRotation.X, (float)Math.Min(Math.Max(Pitch - desiredRotation.Y, PitchLimit - Math.PI / 2), Math.PI / 2 - PitchLimit), 0);
        }

        public Ray GetMouseRay(Point screenCoords)
        {
            Vector3 nearsource = new Vector3(screenCoords.X, screenCoords.Y, 0f);
            Vector3 farsource = new Vector3(screenCoords.X, screenCoords.Y, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            Vector3 nearPoint = SpectrumGame.Game.GraphicsDevice.Viewport.Unproject(nearsource, Projection, View, world);

            Vector3 farPoint = SpectrumGame.Game.GraphicsDevice.Viewport.Unproject(farsource, Projection, View, world);

            Vector3 direction = (farPoint - nearPoint).Normal();
            return new Ray(nearPoint, direction);
        }

        public Vector3 Forward => Orientation * Vector3.Forward;
    }
    public class Camera2D : ICamera
    {
        public Vector2 Position;

        public Matrix View => Transform * Matrix.CreateTranslation(-((ICamera)this).Position);
        public Matrix Projection { get; set; } = Matrix.Identity;
        public Matrix Transform { get; set; } = Matrix.Identity;
        Vector3 ITransform.Position => new Vector3(Position.X, Position.Y, 1);
        Quaternion ITransform.Orientation => Quaternion.Identity;
        Vector3 ITransform.Scale => Vector3.One;
        public void UpdateProjection(int width, int height)
        {
            var scale = height / 1700f;
            Projection = Matrix.CreateScale(scale / width, scale / height , -1);
        }
    }
}
