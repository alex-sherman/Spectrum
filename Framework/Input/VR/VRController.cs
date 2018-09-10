using Microsoft.Xna.Framework;
using Spectrum.Framework.VR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Spectrum.Framework.Input
{
    public struct Hysteresis
    {
        public int Axis;
        public bool UseY;
        public float Exit;
        public float Enter;
        public bool IsPressed(VRController controller, bool lastPressed)
        {
            var axis = controller.Axis[Axis];
            float currentValue = UseY ? axis.Y : axis.X;
            return currentValue >= Enter || (lastPressed && currentValue > Exit);
        }
    }
    public struct VRController
    {
        public static Dictionary<VRButton, Hysteresis> ButtonHysteresis = new Dictionary<VRButton, Hysteresis>();
        public readonly VRHand Hand;
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
            if (index != -1)
                OpenVR.System.GetControllerState((uint)index, ref State, 64);
            var lastPressed = PressedButtons;
            PressedButtons = State.ulButtonPressed;
            foreach (var hysteresis in ButtonHysteresis)
                SetFlagRaw(ref PressedButtons, hysteresis.Key, hysteresis.Value.IsPressed(this, CheckFlagRaw(lastPressed, hysteresis.Key)));
            TouchedButtons = State.ulButtonTouched;
            Axis[0].X = State.rAxis0.x;
            Axis[0].Y = State.rAxis0.y;
            Axis[1].X = State.rAxis1.x;
            Axis[1].Y = State.rAxis1.y;
            Axis[2].X = State.rAxis2.x;
            Axis[2].Y = State.rAxis2.y;
            Axis0Direction = Axis[0];
            Axis0Direction.Normalize();
            var rotation = (Hand == VRHand.Left ? SpecVR.LeftHand : SpecVR.RightHand).ToQuaternion();
            RotationDelta = Quaternion.Inverse(Rotation) * rotation;
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
                case VRButton.DPad_Up:
                    return CheckFlag(touched, VRButton.SteamVR_Touchpad) && Vector2.Dot(Axis0Direction, Vector2.UnitY) > Math.Cos(Math.PI / 4);
                case VRButton.DPad_Down:
                    return CheckFlag(touched, VRButton.SteamVR_Touchpad) && Vector2.Dot(Axis0Direction, -Vector2.UnitY) > Math.Cos(Math.PI / 4);
                case VRButton.DPad_Left:
                    return CheckFlag(touched, VRButton.SteamVR_Touchpad) && Vector2.Dot(Axis0Direction, -Vector2.UnitX) > Math.Cos(Math.PI / 4);
                case VRButton.DPad_Right:
                    return CheckFlag(touched, VRButton.SteamVR_Touchpad) && Vector2.Dot(Axis0Direction, Vector2.UnitX) > Math.Cos(Math.PI / 4);
                default:
                    return CheckFlagRaw(flags, check);
            }
        }
        private bool CheckFlagRaw(ulong flags, VRButton check) => (flags & ((1ul) << (int)check)) != 0;
        private void SetFlagRaw(ref ulong flags, VRButton check, bool value)
        {
            ulong singleBit = ((1ul) << (int)check);
            if (value)
                flags |= singleBit;
            else
                flags &= (~singleBit);
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
