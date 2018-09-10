using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Input
{
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
        Max = 64,
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
}
