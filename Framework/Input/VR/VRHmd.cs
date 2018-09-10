using Microsoft.Xna.Framework;
using Spectrum.Framework.VR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Input
{
    public struct VRHMD
    {
        public Vector3 Position;
        public Vector3 PositionDelta;
        public Vector3 Direction;
        public Quaternion Rotation;
        public Quaternion RotationDelta;
        public void Update()
        {
            var position = SpecVR.HeadPose.Translation;
            PositionDelta = position - Position;
            Position = position;
            var rotation = SpecVR.HeadPose.ToQuaternion();
            RotationDelta = rotation * Quaternion.Inverse(Rotation);
            Rotation = rotation;
        }
    }
}
