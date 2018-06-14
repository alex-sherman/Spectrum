using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Spectrum.Framework.VR
{
    public enum VRAxisType
    {
        LeftStickVertical,
        LeftStickHorizontal,
        RightStickVertical,
        RightStickHorizontal,
        LeftTrigger,
        RightTrigger
    }
    public struct VRButtonBinding
    {
        public VRHand Hand;
        public VRButton Button;
        public VRPressType PressType;
        public VRButtonBinding(VRButton button, VRHand hand = VRHand.Left | VRHand.Right, VRPressType pressType = VRPressType.Pressed)
        {
            Hand = hand;
            Button = button;
            PressType = pressType;
        }
    }
    
    public enum VRButton
    {
        System = 0,
        ApplicationMenu = 1,
        Grip = 2,
        DPad_Left = 3,
        DPad_Up = 4,
        DPad_Right = 5,
        DPad_Down = 6,
        A = 7,
        ProximitySensor = 31,
        Axis0 = 32,
        Axis1 = 33,
        Axis2 = 34,
        Axis3 = 35,
        Axis4 = 36,
        SteamVR_Touchpad = 32,
        SteamVR_Trigger = 33,
        Dashboard_Back = 2,
        Max = 64,
        BetterTrigger = 65,
    }
    [Flags]
    public enum VRHand
    {
        Left = 1,
        Right = 2
    }
    [Flags]
    public enum VRPressType
    {
        Pressed = 1,
        Touched = 2
    }
    public struct VRController
    {
        public VRHand Hand { get; private set; }
        public ulong pressedButtons;
        public ulong touchedButtons;
        public VRControllerState_t state;
        public Vector2 touchpad;
        public Vector2 touchpadDirection;
        public double touchpadAngle;
        public Vector3 direction;
        public Vector3 position;
        public Quaternion rotation;
        public VRController(VRHand hand)
        {
            Hand = hand;
            pressedButtons = 0;
            touchedButtons = 0;
            state = new VRControllerState_t();
            touchpad = Vector2.Zero;
            touchpadDirection = Vector2.Zero;
            touchpadAngle = 0;
            direction = Vector3.Forward;
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
        }
        public void Update()
        {
            int index = Hand == VRHand.Left ? SpecVR.LeftHandIndex : SpecVR.RightHandIndex;
            if(index != -1)
                OpenVR.System.GetControllerState((uint)index, ref state, 64);
            pressedButtons = state.ulButtonPressed;
            touchedButtons = state.ulButtonTouched;
            touchpad.X = state.rAxis0.x;
            touchpad.Y = state.rAxis0.y;
            touchpadDirection = touchpad;
            touchpadDirection.Normalize();
            touchpadAngle = Math.Atan2(touchpadDirection.Y, touchpadDirection.X);
            rotation = (Hand == VRHand.Left ? SpecVR.LeftHand : SpecVR.RightHand).ToQuaternion();
            direction = Vector3.Transform(Vector3.Forward, rotation);
            position = (Hand == VRHand.Left ? SpecVR.LeftHand : SpecVR.RightHand).Translation;
        }
        private bool CheckFlag(bool touched, VRButton check)
        {
            var flags = touched ? touchedButtons : pressedButtons;
            switch (check)
            {
                case VRButton.BetterTrigger:
                    return touched ? state.rAxis1.x > 0.1 : state.rAxis1.x > 0.95;
                case VRButton.DPad_Up:
                    return CheckFlag(touched, VRButton.SteamVR_Touchpad) && Vector2.Dot(touchpadDirection, Vector2.UnitY) > Math.Cos(Math.PI / 4);
                case VRButton.DPad_Down:
                    return CheckFlag(touched, VRButton.SteamVR_Touchpad) && Vector2.Dot(touchpadDirection, -Vector2.UnitY) > Math.Cos(Math.PI / 4);
                case VRButton.DPad_Left:
                    return CheckFlag(touched, VRButton.SteamVR_Touchpad) && Vector2.Dot(touchpadDirection, Vector2.UnitX) > Math.Cos(Math.PI / 4);
                case VRButton.DPad_Right:
                    return CheckFlag(touched, VRButton.SteamVR_Touchpad) && Vector2.Dot(touchpadDirection, -Vector2.UnitX) > Math.Cos(Math.PI / 4);
                case VRButton.Axis0:
                case VRButton.Axis1:
                case VRButton.Axis2:
                case VRButton.Axis3:
                case VRButton.Grip:
                case VRButton.System:
                case VRButton.ApplicationMenu:
                    return (flags & ((1ul) << (int)check)) != 0;
                default:
                    return false;
            }
        }
        public bool IsButtonPressed(VRButtonBinding binding)
        {
            return binding.Hand.HasFlag(Hand) && 
                (
                    (binding.PressType.HasFlag(VRPressType.Pressed) && CheckFlag(false, binding.Button))
                    || (binding.PressType.HasFlag(VRPressType.Touched) && CheckFlag(true, binding.Button))
                );
        }
    }
}
