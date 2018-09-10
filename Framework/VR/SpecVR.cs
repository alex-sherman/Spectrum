using Microsoft.Xna.Framework;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Spectrum.Framework.VR
{
    public enum VRHardwareType
    {
        Vive,
        Oculus,
        Unknown = 63,
    }
    public static class SpecVR
    {
        public static VRHardwareType HardwareType;
        public static bool Init()
        {
            if (OpenVR.IsHmdPresent())
            {
                EVRInitError error = EVRInitError.Init_Internal;
                OpenVR.Init(ref error);
                Running = error == EVRInitError.None;
                if (!Running)
                    OpenVR.Shutdown();
                else
                {
                    Listen(EVREventType.VREvent_HideKeyboard, (_) => IsKeyboardVisible = false);
                    Listen(EVREventType.VREvent_KeyboardClosed, (_) => IsKeyboardVisible = false);
                    Listen(EVREventType.VREvent_KeyboardDone, (_) => IsKeyboardVisible = false);
                    Listen(EVREventType.VREvent_ShowKeyboard, (_) => IsKeyboardVisible = true);
                    StringBuilder stringBuilder = new StringBuilder();
                    ETrackedPropertyError tError = ETrackedPropertyError.TrackedProp_Success;
                    OpenVR.System.GetStringTrackedDeviceProperty(0, ETrackedDeviceProperty.Prop_TrackingSystemName_String, stringBuilder, 64, ref tError);
                    switch (stringBuilder.ToString())
                    {
                        case "oculus":
                            HardwareType = VRHardwareType.Oculus;
                            break;
                        case "lighthouse":
                            HardwareType = VRHardwareType.Vive;
                            break;
                        default:
                            HardwareType = VRHardwareType.Unknown;
                            break;
                    }
                    switch (HardwareType)
                    {
                        case VRHardwareType.Vive:
                            VRController.ButtonHysteresis[VRButton.SteamVR_Trigger] = new Hysteresis()
                            {
                                Axis = 1,
                                Enter = 1,
                                Exit = 0.95f,
                            };
                            break;
                        case VRHardwareType.Oculus:
                            VRController.ButtonHysteresis[VRButton.Grip] = new Hysteresis()
                            {
                                Axis = 2,
                                Enter = 0.5f,
                                Exit = 0.4f,
                            };
                            break;
                    }
                }
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
        public static void Update()
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
        public static void PollEvents()
        {
            var size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));
            VREvent_t retEvent = new VREvent_t();
            // The limit of 100 came from a bug on other Alex's system, it seems like events still make their way in even with the limit
            while(OpenVR.System.PollNextEvent(ref retEvent, size))
            //for (int i = 0; i < 100 && OpenVR.System.PollNextEvent(ref retEvent, size); i++)
            {
                if (listeners.TryGetValue((EVREventType)retEvent.eventType, out List<Action<VREvent_t>> handlers))
                    foreach (var handler in handlers)
                        handler(retEvent);
            }
        }
        static DefaultDict<EVREventType, List<Action<VREvent_t>>> listeners =
            new DefaultDict<EVREventType, List<Action<VREvent_t>>>(() => new List<Action<VREvent_t>>(), true);
        public static void Listen(EVREventType eventType, Action<VREvent_t> handler)
        {
            listeners[eventType].Add(handler);
        }
        public static Vector3 HeadPosition(Vector3? basis = null, Vector3? orientedOffset = null)
        {
            var derp = Vector3.Transform((orientedOffset ?? Vector3.Zero), HeadPose.ToQuaternion());
            return (basis ?? Vector3.Zero) + HeadPose.Translation + derp;
        }
        static StringBuilder textBuilder = new StringBuilder(1024);
        public static bool IsKeyboardVisible { get; private set; }
        public static void ShowKeyboard(string existingText = "")
        {
            textBuilder.Clear();
            textBuilder.Append(existingText);
            OpenVR.Overlay.ShowKeyboard(0, 0, "Prefab Name", 64, textBuilder.ToString(), false, 0);
        }
        public static void HideKeyboard() => OpenVR.Overlay.HideKeyboard();
        public static string GetKeyBoardText()
        {
            OpenVR.Overlay.GetKeyboardText(textBuilder, 1024);

            return textBuilder.ToString();
        }
    }
}
