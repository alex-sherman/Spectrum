#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SharpDX.DirectInput;
using Spectrum.Framework.VR;
using System;
using System.Linq;
using System.Collections.Generic;
#endregion

namespace Spectrum.Framework.Input
{
    public class InputState
    {
        public static float MouseSensitivity = 0.003f;
        private static DirectInput di = new DirectInput();
        private static SpectrumMouse SpecMouse = new SpectrumMouse(di);
        public static InputState Current { get; private set; } = new InputState();

        #region Fields
        public Microsoft.Xna.Framework.Input.KeyboardState KeyboardState;
        public SpectrumMouseState MouseState;
        public Gamepad[] Gamepads = new Gamepad[4];
        public VRController[] VRControllers = new VRController[] { new VRController(VRHand.Left), new VRController(VRHand.Right) };
        private InputState LastInputState;
        #endregion

        #region Initialization



        public InputState()
        {
            KeyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            MouseState = SpecMouse.GetCurrentState();
            LastInputState = null;
            for (int i = 0; i < 4; i++)
            {
                Gamepads[i] = new Gamepad(new SharpDX.XInput.Controller((SharpDX.XInput.UserIndex)i));
            }

        }


        #endregion

        #region Public Methods

        public void Update()
        {
            if (LastInputState == null)
                LastInputState = new InputState();
            LastInputState.KeyboardState = KeyboardState;
            KeyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            LastInputState.MouseState = MouseState;
            MouseState = SpecMouse.GetCurrentState();
            for (int i = 0; i < 4; i++)
            {
                LastInputState.Gamepads[i] = Gamepads[i];
                Gamepads[i].Update();
            }
            if (SpecVR.Running)
            {
                for (int i = 0; i < 2; i++)
                {
                    LastInputState.VRControllers[i] = VRControllers[i];
                    VRControllers[i].Update();
                }
            }
        }
        public bool IsKeyDown(string bindingName)
        {
            return IsKeyDown(bindingName, PlayerInformation.Default);
        }
        public bool IsKeyDown(string bindingName, PlayerInformation playerInfo)
        {
            InputLayout layout = playerInfo.Layout;
            KeyBinding binding;
            if (!layout.KeyBindings.TryGetValue(bindingName, out binding)) throw new KeyNotFoundException("Binding not found");
            foreach (var bindingInfo in binding.Options)
            {
                if (playerInfo.UsesKeyboard)
                {
                    if (bindingInfo.keyModifier != null && !IsKeyDown(bindingInfo.keyModifier.Value)) { continue; }
                    if (bindingInfo.key != null) { if (IsKeyDown(bindingInfo.key.Value)) return true; }
                    if (bindingInfo.mouseButton != null) { if (IsMouseDown(bindingInfo.mouseButton.Value)) return true; }
                }
                foreach (int gamepadIndex in playerInfo.UsedGamepads)
                {
                    if (bindingInfo.buttonModifier != null && !IsButtonDown((GamepadButton)bindingInfo.buttonModifier, gamepadIndex)) { continue; }
                    if (bindingInfo.button != null && IsButtonDown((GamepadButton)bindingInfo.button, gamepadIndex)) { return true; }
                }
                if(SpecVR.Running && bindingInfo.vrButton != null)
                {
                    if (IsButtonDown(bindingInfo.vrButton.Value)) { return true; }
                }
            }
            return false;
        }
        public bool IsKeyDown(Keys key)
        {
            return KeyboardState.IsKeyDown(key);
        }
        private bool IsButtonDown(GamepadButton button, int gamepadIndex)
        {
            return Gamepads[gamepadIndex].IsButtonPressed(button);
        }
        private bool IsButtonDown(VRButtonBinding button)
        {
            return VRControllers.Any(controller => controller.IsButtonPressed(button));
        }

        public bool IsNewKeyPress(string bindingName)
        {
            return IsNewKeyPress(bindingName, PlayerInformation.Default);
        }
        public bool IsNewKeyPress(string bindingName, PlayerInformation playerInfo)
        {
            return IsKeyDown(bindingName, playerInfo) && !LastInputState.IsKeyDown(bindingName, playerInfo);

        }
        public bool IsNewKeyPress(Keys key)
        {
            return (IsKeyDown(key) &&
                    !LastInputState.IsKeyDown(key));
        }

        public bool IsNewKeyRelease(string bindingName)
        {
            return IsNewKeyRelease(bindingName, PlayerInformation.Default);
        }
        public bool IsNewKeyRelease(string bindingName, PlayerInformation playerInfo)
        {
            return !IsKeyDown(bindingName, playerInfo) && LastInputState.IsKeyDown(bindingName, playerInfo);

        }
        public bool IsNewKeyRelease(Keys key)
        {
            return (!IsKeyDown(key) &&
                    LastInputState.IsKeyDown(key));
        }

        public float GetAxis1D(string axisName)
        {
            return GetAxis1D(axisName, PlayerInformation.Default);
        }
        public float GetAxis1D(string axisName, PlayerInformation playerInfo)
        {
            InputLayout layout = playerInfo.Layout;
            Axis1 axis;
            if (!layout.Axes1.TryGetValue(axisName, out axis)) throw new KeyNotFoundException("Axis not found");
            return axis.Value(this, playerInfo);
        }

        public Vector2 GetAxis2D(string horizontal, string vertical, bool limitToOne = false)
        {
            return GetAxis2D(horizontal, vertical, PlayerInformation.Default, limitToOne);
        }
        public Vector2 GetAxis2D(string horizontal, string vertical, PlayerInformation playerInfo, bool limitToOne)
        {
            Vector2 output = new Vector2(GetAxis1D(horizontal, playerInfo), GetAxis1D(vertical, playerInfo));
            if (limitToOne && output.LengthSquared() > 1)
                output.Normalize();
            return output;
        }
        public Point MousePosition { get { return new Point(MouseState.X, MouseState.Y); } }
        public bool IsMouseDown(int button)
        {
            return button >= MouseState.buttons.Length ? false : MouseState.buttons[button];
        }
        public bool IsNewMousePress(int button)
        {
            return IsMouseDown(button) && !LastInputState.IsMouseDown(button);
        }
        public bool IsNewMouseRelease(int button)
        {
            return !IsMouseDown(button) && LastInputState.IsMouseDown(button);
        }
        public int MouseWheelDistance()
        {
            return MouseState.Scroll;
        }


        #endregion

        /// <summary>
        /// Helps take keyboard input for a text box or something.
        /// Should be called in HandleInput
        /// </summary>
        /// <param name="currentString">The string being modified</param>
        /// <param name="input">Input from the HandleInput call</param>
        /// <returns>Modified string</returns>
        public void TakeKeyboardInput(ref int position, ref string currentString)
        {
            Keys[] pressedKeys = KeyboardState.GetPressedKeys();
            foreach (Keys key in pressedKeys)
            {

                if (IsNewKeyPress(key))
                {

                    if (key == Keys.Back && position > 0)
                    {
                        position--;
                        currentString = currentString = currentString.Remove(position, 1);
                    }
                    char typedChar = GetChar(key, IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift));
                    if (typedChar != (char)0)
                    {
                        currentString = currentString.Insert(position, "" + typedChar);
                        position++;
                    }
                }
            }
        }
        private static char GetChar(Keys key, bool shiftHeld)
        {
            if (key == Keys.Space) return ' ';
            if (key >= Keys.A && key <= Keys.Z)
            {
                if (shiftHeld)
                {
                    return key.ToString()[0];
                }
                else
                {
                    return key.ToString().ToLower()[0];
                }
            }
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                if (shiftHeld)
                {
                    if (key == Keys.D2) return '@';
                    else if (key == Keys.D0) return ')';
                    else if (key == Keys.D6) return '^';
                    else if (key == Keys.D8) return '*';
                    else
                    {
                        if (key > Keys.D5) { return (char)(key - Keys.D0 + 31); }
                        else return (char)(key - Keys.D0 + 32);
                    }
                }
                else return (key - Keys.D0).ToString()[0];
            }
            if (key >= Keys.NumPad0 && key <= Keys.NumPad9) return (key - Keys.NumPad0).ToString()[0];
            if (key == Keys.OemPeriod || key == Keys.Decimal) return '.';
            if (key == Keys.OemQuestion) return '/';
            return (char)0;
        }
    }
}
