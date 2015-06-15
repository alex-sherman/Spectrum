using Microsoft.Xna.Framework;
using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Input
{
    public enum GamepadAxisType
    {
        LeftStickVertical,
        LeftStickHorizontal,
        RightStickVertical,
        RightStickHorizontal,
        LeftTrigger,
        RightTrigger
    }
    public enum GamepadButton
    {
        None = 0,
        DPadUp = 1,
        DPadDown = 2,
        DPadLeft = 4,
        DPadRight = 8,
        Start = 16,
        Back = 32,
        LeftThumb = 64,
        RightThumb = 128,
        LeftShoulder = 256,
        RightShoulder = 512,
        A = 4096,
        B = 8192,
        X = 16384,
        Y = 32768,
        LeftTrigger = -1,
        RightTrigger = -2
    }

    public struct Gamepad
    {
        private const float MAX_STICK_VALUE = 32767;
        private Controller controller;
        private SharpDX.XInput.Gamepad state;
        public Gamepad(Controller controller)
        {
            this.controller = controller;
            state = default(SharpDX.XInput.Gamepad);

        }
        public void Update()
        {
            if (controller.IsConnected)
                state = controller.GetState().Gamepad;
        }
        private float axisConvert(short value, short deadzone)
        {
            if (value > -deadzone && value < deadzone) return 0;
            return Math.Max(Math.Sign(value) * ((Math.Abs((float)value) - deadzone) / (MAX_STICK_VALUE - deadzone)), -1);
        }
        public float Axis(GamepadAxisType axis)
        {
            switch (axis)
            {
                case GamepadAxisType.LeftStickVertical:
                    return axisConvert(state.LeftThumbY, SharpDX.XInput.Gamepad.LeftThumbDeadZone);
                case GamepadAxisType.LeftStickHorizontal:
                    return axisConvert(state.LeftThumbX, SharpDX.XInput.Gamepad.LeftThumbDeadZone);
                case GamepadAxisType.RightStickVertical:
                    return axisConvert(state.RightThumbY, SharpDX.XInput.Gamepad.RightThumbDeadZone);
                case GamepadAxisType.RightStickHorizontal:
                    return axisConvert(state.RightThumbX, SharpDX.XInput.Gamepad.RightThumbDeadZone);
                case GamepadAxisType.LeftTrigger:
                    return state.LeftTrigger / 255.0f;
                case GamepadAxisType.RightTrigger:
                    return state.RightTrigger / 255.0f;
                default:
                    break;                    
            }
            return 0;
        }

        public bool IsButtonPressed(GamepadButton button)
        {
            switch (button)
            {
                case GamepadButton.LeftTrigger:
                    return state.LeftTrigger > SharpDX.XInput.Gamepad.TriggerThreshold;
                case GamepadButton.RightTrigger:
                    return state.RightTrigger > SharpDX.XInput.Gamepad.TriggerThreshold;
                default:
                    return (state.Buttons & (SharpDX.XInput.GamepadButtonFlags)(int)button) != 0;
            }
        }
    }
}
