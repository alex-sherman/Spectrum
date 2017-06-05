using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Spectrum.Framework.VR
{
    public static class SpecVR
    {
        public static bool Init()
        {
            EVRInitError error = EVRInitError.Init_Internal;
            OpenVR.Init(ref error);
            Running = error == EVRInitError.None;
            return Running;
        }
        public static bool Running { get; private set; }
        public static Matrix HeadPose { get; private set; }
        public static Matrix LeftHand { get; private set; }
        public static Matrix RightHand { get; private set; }
        public static void Update(GameTime time)
        {
            TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            TrackedDevicePose_t[] poses2 = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            OpenVR.Compositor.WaitGetPoses(poses, poses2);
            //TODO: Why invert?
            HeadPose = Matrix.Invert(poses[0].mDeviceToAbsoluteTracking);
            LeftHand = poses[3].mDeviceToAbsoluteTracking;
        }
    }
}
