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
            if (OpenVR.IsHmdPresent())
            {
                EVRInitError error = EVRInitError.Init_Internal;
                OpenVR.Init(ref error);
                Running = error == EVRInitError.None;
                if (!Running)
                    OpenVR.Shutdown();
            }
            return Running;
        }
        public static bool Running { get; private set; }
        public static int HMDIndex { get; private set; } = -1;
        public static Matrix HeadPose { get; private set; } = Matrix.Identity;
        public static int LeftHandIndex { get; private set; } = -1;
        public static Matrix LeftHand { get; private set; } = Matrix.Identity;
        public static int RightHandIndex { get; private set; } = -1;
        public static Matrix RightHand { get; private set; } = Matrix.Identity;
        public static void Update(GameTime time)
        {
            TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            TrackedDevicePose_t[] poses2 = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            OpenVR.Compositor.WaitGetPoses(poses, poses2);
            LeftHandIndex = -1;
            RightHandIndex = -1;
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                var deviceClass = OpenVR.System.GetTrackedDeviceClass(i);
                switch (deviceClass)
                {
                    case ETrackedDeviceClass.HMD:
                        HMDIndex = (int)i;
                        break;
                    case ETrackedDeviceClass.Controller:
                        var hand = OpenVR.System.GetControllerRoleForTrackedDeviceIndex(i);
                        switch (hand)
                        {
                            case ETrackedControllerRole.LeftHand:
                                LeftHandIndex = (int)i;
                                break;
                            case ETrackedControllerRole.RightHand:
                                RightHandIndex = (int)i;
                                break;
                        }
                        break;
                }
            }
            if (HMDIndex != -1)
                HeadPose = poses[HMDIndex].mDeviceToAbsoluteTracking;
            if (LeftHandIndex != -1)
                LeftHand = poses[LeftHandIndex].mDeviceToAbsoluteTracking;
            if (RightHandIndex != -1)
                RightHand = poses[RightHandIndex].mDeviceToAbsoluteTracking;
        }
        public static Vector3 HeadPosition(Vector3? basis = null, Vector3? orientedOffset = null)
        {
            var derp = Vector3.Transform((orientedOffset ?? Vector3.Zero), HeadPose.ToQuaternion());
            return (basis ?? Vector3.Zero) + HeadPose.Translation + derp;
        }
    }
}
