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
        CullMode culling = CullMode.None;
        Color clearColor = Color.CornflowerBlue;
        float nearPlane = 1f;
        float farPlane = 10000;
        float aspectRatio;
        private Vector3 _position;
        protected Quaternion _rotation;
        public Quaternion Rotation { get { return _rotation; } }
        private float _yaw, _pitch, _roll;
        public float Yaw { get { return _yaw; } set { _yaw = value; _rotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, _roll); } }
        public float Pitch { get { return _pitch; } set { _pitch = value; _rotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, _roll); } }
        public float Roll { get { return _roll; } set { _roll = value; _rotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, _roll); } }

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

        public float NearPlane
        {
            get { return nearPlane; }
            set { nearPlane = value; }
        }

        public float FarPlane
        {
            get { return farPlane; }
            set { farPlane = value; }
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
                    Vector3.Transform(Vector3.Forward, _rotation) + Position,
                    Vector3.Transform(Vector3.Up, _rotation));
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

                Vector3 refTP = Vector3.Transform(Vector3.Forward, _rotation) + Position;
                refTP.Y = -refTP.Y + 2 * Water.waterHeight;

                Vector3 cameraRight = Vector3.Transform(new Vector3(1, 0, 0), _rotation);
                Vector3 invUpVector = Vector3.Cross(cameraRight, refTP - refCP);

                return Matrix.CreateLookAt(refCP, refTP, invUpVector);
            }
        }
    }
}
