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
    }
    [Flags]
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
    }
    [Flags]
    public enum VRHand
    {
        Left = 1,
        Right = 2
    }
    public struct VRController
    {
        public VRHand Hand { get; private set; }
        private ulong buttons;
        public VRController(VRHand hand)
        {
            Hand = hand;
            buttons = 0;
        }
        public void Update()
        {
            VRControllerState_t state = new VRControllerState_t();
            OpenVR.System.GetControllerState(3, ref state, 64);
            buttons = state.ulButtonPressed;
        }
        public bool IsButtonPressed(VRButton button)
        {
            return (buttons & ((1ul) << (int)button)) != 0;
        }
    }
}
