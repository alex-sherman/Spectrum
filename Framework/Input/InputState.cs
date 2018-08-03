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
        private static SpectrumMouse SpecMouse = new SpectrumMouse();
        public static InputState Current { get; private set; } = new InputState();

        public Microsoft.Xna.Framework.Input.KeyboardState KeyboardState;
        public SpectrumMouseState MouseState;
        public Gamepad[] Gamepads = new Gamepad[4];
        public VRHMD VRHMD = new VRHMD();
        public VRController[] VRControllers = new VRController[] { new VRController(VRHand.Left), new VRController(VRHand.Right) };
        private InputState LastInputState;

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

        #region Public Methods

        public void Update()
        {
            if (LastInputState == null)
                LastInputState = new InputState();
            LastInputState.KeyboardState = KeyboardState;
            KeyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            LastInputState.MouseState = MouseState;
            MouseState = SpecMouse.GetCurrentState(MouseState);
            for (int i = 0; i < 4; i++)
            {
                LastInputState.Gamepads[i] = Gamepads[i];
                Gamepads[i].Update();
            }
            if (SpecVR.Running)
            {
                LastInputState.VRHMD = VRHMD;
                VRHMD.Update();
                for (int i = 0; i < 2; i++)
                {
                    LastInputState.VRControllers[i] = VRControllers[i];
                    VRControllers[i].Update();
                }
            }
            RawMouse.Update();
        }
        public bool IsKeyDown(string bindingName, PlayerInformation playerInfo = null, bool ignoreModifiers = false)
        {
            InputLayout layout = (playerInfo ?? PlayerInformation.Default).Layout;
            if (!layout.KeyBindings.TryGetValue(bindingName, out KeyBinding binding))
            {
                DebugPrinter.PrintOnce("Binding not found " + bindingName);
                return false;
            }
            return binding.Options.Any(button => IsKeyDown(button, playerInfo, ignoreModifiers));
        }
        public bool IsKeyDown(KeyBind button, PlayerInformation playerInfo = null, bool ignoreModifiers = false)
        {
            playerInfo = playerInfo ?? PlayerInformation.Default;
            if (!ignoreModifiers && !button.modifiers.All(modifier => IsKeyDown(modifier, playerInfo)))
                return false;
            if (playerInfo.UsesKeyboard)
            {
                if ((button.key != null && IsKeyDown(button.key.Value)) ||
                    (button.mouseButton != null && IsMouseDown(button.mouseButton.Value)))
                    return true;
            }
            foreach (int gamepadIndex in playerInfo.UsedGamepads)
            {
                if (button.button != null && IsButtonDown((GamepadButton)button.button, gamepadIndex)) { return true; }
            }
            if (SpecVR.Running && button.vrButton != null)
            {
                if (IsButtonDown(button.vrButton.Value)) { return true; }
            }
            return false;
        }
        public bool IsKeyDown(Keys key) => KeyboardState.IsKeyDown(key);
        private bool IsButtonDown(GamepadButton button, int gamepadIndex)
            => Gamepads[gamepadIndex].IsButtonPressed(button);
        private bool IsButtonDown(VRButtonBinding button)
            => VRControllers.Any(controller => controller.IsButtonPressed(button));
        public bool IsNewKeyPress(string bindingName, PlayerInformation playerInfo = null)
            => IsKeyDown(bindingName, playerInfo) && !LastInputState.IsKeyDown(bindingName, playerInfo);
        public bool IsNewKeyPress(Keys key)
            => IsKeyDown(key) && !LastInputState.IsKeyDown(key);
        public bool IsNewKeyPress(KeyBind button)
            => IsKeyDown(button) && !LastInputState.IsKeyDown(button);
        public bool IsNewKeyRelease(string bindingName, PlayerInformation playerInfo = null)
            => !IsKeyDown(bindingName, playerInfo) && LastInputState.IsKeyDown(bindingName, playerInfo);
        public bool IsNewKeyRelease(Keys key)
            => !IsKeyDown(key) && LastInputState.IsKeyDown(key);
        public float GetAxis1D(string axisName, PlayerInformation playerInfo = null)
        {
            playerInfo = playerInfo ?? PlayerInformation.Default;
            InputLayout layout = playerInfo.Layout;
            if (!layout.Axes1.TryGetValue(axisName, out Axis1 axis)) throw new KeyNotFoundException("Axis not found");
            return axis.Value(this, playerInfo);
        }

        public Vector2 GetAxis2D(string horizontal, string vertical, bool limitToOne = false, PlayerInformation playerInfo = null)
        {
            Vector2 output = new Vector2(GetAxis1D(horizontal, playerInfo), GetAxis1D(vertical, playerInfo));
            if (limitToOne && output.LengthSquared() > 1)
                output.Normalize();
            return output;
        }
        public Point MousePosition { get { return new Point(MouseState.X, MouseState.Y); } }
        public bool IsMouseDown(int button)
            => button >= MouseState.buttons.Length ? false : MouseState.buttons[button];
        public bool IsNewMousePress(int button)
            => IsMouseDown(button) && !LastInputState.IsMouseDown(button);
        public bool IsNewMouseRelease(int button)
            => !IsMouseDown(button) && LastInputState.IsMouseDown(button);
        public int MouseWheelDistance => MouseState.Scroll;


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
            position = Math.Max(Math.Min(position, currentString.Length), 0);
            Keys[] pressedKeys = KeyboardState.GetPressedKeys();
            foreach (Keys key in pressedKeys)
            {
                if (IsNewKeyPress(key))
                {
                    if (key == Keys.Back && position > 0)
                    {
                        int count = 0;
                        if(IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl))
                        {
                            while (count < currentString.Length - 1 && currentString[currentString.Length - 1 - count] != ' ')
                                count++;
                        }
                        count++;
                        position -= count;
                        currentString = currentString.Remove(position, count);
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
