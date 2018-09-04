using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Spectrum.Framework.VR
{
    public struct VRBinding
    {
        public VRHand Hand;
        public VRButton Button;
        public VRPressType PressType;
        public VRBinding(VRButton button, VRHand hand = VRHand.Left | VRHand.Right, VRPressType pressType = VRPressType.Pressed)
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
    public struct VRController
    {
        public VRHand Hand { get; private set; }
        public ulong PressedButtons;
        public ulong TouchedButtons;
        public VRControllerState_t State;
        public Vector2[] Axis;
        public Vector2 Axis0Direction;
        public double AxisAngle(int axis) { return Math.Atan2(Axis[axis].Y, Axis[axis].X); }
        public Vector3 Direction { get => Vector3.Transform(Vector3.Forward, Rotation); }
        public Vector3 Position;
        public Vector3 PositionDelta;
        public Quaternion Rotation;
        public Quaternion RotationDelta;
        public Quaternion Pointing;
        public VRController(VRHand hand)
        {
            Hand = hand;
            PressedButtons = 0;
            TouchedButtons = 0;
            State = new VRControllerState_t();
            Axis = new Vector2[5];
            Axis0Direction = Vector2.Zero;
            Position = PositionDelta = Vector3.Zero;
            Rotation = RotationDelta = Quaternion.Identity;
            Pointing = Quaternion.Identity;
        }
        public void Update()
        {
            int index = Hand == VRHand.Left ? SpecVR.LeftHandIndex : SpecVR.RightHandIndex;
            if(index != -1)
                OpenVR.System.GetControllerState((uint)index, ref State, 64);
            PressedButtons = State.ulButtonPressed;
            TouchedButtons = State.ulButtonTouched;
            Axis[0].X = State.rAxis0.x;
            Axis[0].Y = State.rAxis0.y;
            Axis[1].X = State.rAxis1.x;
            Axis[1].Y = State.rAxis1.y;
            Axis0Direction = Axis[0];
            Axis0Direction.Normalize();
            var rotation = (Hand == VRHand.Left ? SpecVR.LeftHand : SpecVR.RightHand).ToQuaternion();
            RotationDelta = rotation * Quaternion.Inverse(Rotation);
            Rotation = rotation;
            Pointing = Quaternion.CreateFromAxisAngle(Vector3.Right, -(float)Math.PI / 3f).Concat(Rotation);
            var position = (Hand == VRHand.Left ? SpecVR.LeftHand : SpecVR.RightHand).Translation;
            PositionDelta = position - Position;
            Position = position;
        }
        private bool CheckFlag(bool touched, VRButton check)
        {
            var flags = touched ? TouchedButtons : PressedButtons;
            switch (check)
            {
                case VRButton.BetterTrigger:
                    return touched ? State.rAxis1.x > 0.1 : State.rAxis1.x >= 1;
                case VRButton.DPad_Up:
                    return CheckFlag(touched, VRButton.SteamVR_Touchpad) && Vector2.Dot(Axis0Direction, Vector2.UnitY) > Math.Cos(Math.PI / 4);
                case VRButton.DPad_Down:
                    return CheckFlag(touched, VRButton.SteamVR_Touchpad) && Vector2.Dot(Axis0Direction, -Vector2.UnitY) > Math.Cos(Math.PI / 4);
                case VRButton.DPad_Left:
                    return CheckFlag(touched, VRButton.SteamVR_Touchpad) && Vector2.Dot(Axis0Direction, -Vector2.UnitX) > Math.Cos(Math.PI / 4);
                case VRButton.DPad_Right:
                    return CheckFlag(touched, VRButton.SteamVR_Touchpad) && Vector2.Dot(Axis0Direction, Vector2.UnitX) > Math.Cos(Math.PI / 4);
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
        public bool IsButtonPressed(VRBinding binding)
        {
            return binding.Hand.HasFlag(Hand) && 
                (
                    (binding.PressType.HasFlag(VRPressType.Pressed) && CheckFlag(false, binding.Button))
                    || (binding.PressType.HasFlag(VRPressType.Touched) && CheckFlag(true, binding.Button))
                );
        }
    }
}
