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
    [Flags]
    public enum KeyPressType
    {
        None,
        Press,
        Release,
        Hold,
    }
    public class InputState
    {
        public static float MouseSensitivity = 0.003f;
        public static SpectrumMouse SpecMouse = new SpectrumMouse();
        public static InputState Current { get; private set; } = new InputState();

        public float DT;
        public Microsoft.Xna.Framework.Input.KeyboardState KeyboardState;
        public CursorState CursorState;
        public Gamepad[] Gamepads = new Gamepad[4];
        public VRHMD VRHMD = new VRHMD();
        public VRController[] VRControllers = new VRController[] { new VRController(VRHand.Left), new VRController(VRHand.Right) };
        public VRController VRFromHand(VRHand hand) => VRControllers[hand == VRHand.Right ? 1 : 0];
        public InputState Last;
        public bool DisableCursorState { get; private set; }
        public DefaultDict<KeyBind, bool> consumedKeys = new DefaultDict<KeyBind, bool>();

        public InputState(bool disableCursorState = false)
        {
            DisableCursorState = disableCursorState;
            KeyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            if (!DisableCursorState)
                CursorState = SpecMouse.GetCurrentState();
            Last = null;
            for (int i = 0; i < 4; i++)
            {
                Gamepads[i] = new Gamepad(new SharpDX.XInput.Controller((SharpDX.XInput.UserIndex)i));
            }
        }

        #region Public Methods

        public void Update(float dt)
        {
            if (Last == null)
                Last = new InputState();
            Last.DT = DT;
            DT = dt;
            Last.KeyboardState = KeyboardState;
            KeyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            Last.CursorState = CursorState;
            if (!DisableCursorState)
                CursorState = SpecMouse.GetCurrentState(CursorState);
            for (int i = 0; i < 4; i++)
            {
                Last.Gamepads[i] = Gamepads[i];
                Gamepads[i].Update();
            }
            if (SpecVR.Running)
            {
                Last.VRHMD = VRHMD;
                VRHMD.Update();
                for (int i = 0; i < 2; i++)
                {
                    Last.VRControllers[i] = VRControllers[i];
                    VRControllers[i].Update();
                }
            }
            if (!DisableCursorState)
                RawMouse.Update();
            consumedKeys.SetFrom(consumedKeys.Where(kvp => kvp.Value && IsKeyDown(kvp.Key, false)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }
        public void ConsumeInput(KeyBind key, bool hold)
            => consumedKeys[key] = hold;
        public bool IsConsumed(KeyBind key)
            => consumedKeys.ContainsKey(key);
        public bool IsKeyDown(string bindingName, PlayerInformation playerInfo = null)
        {
            InputLayout layout = (playerInfo ?? PlayerInformation.Default).Layout;
            if (!layout.KeyBindings.TryGetValue(bindingName, out KeyBinding binding))
            {
                DebugPrinter.PrintOnce("Binding not found " + bindingName);
                return false;
            }
            return IsKeyDown(binding);
        }
        public bool IsKeyDown(KeyBinding binding, bool consultConsumed = true)
        {
            return binding.Options.Any(button => IsKeyDown(button, consultConsumed));
        }
        public bool IsKeyDown(KeyBind button, bool consultConsumed = true)
        {
            if (consultConsumed && consumedKeys.ContainsKey(button))
                return false;
            if (!button.modifiers.All(modifier => IsKeyDown(modifier)))
                return false;
            if ((button.key != null && KeyboardState.IsKeyDown(button.key.Value)) ||
                (button.mouseButton != null && IsMouseDown(button.mouseButton.Value)))
                return true;
            // TODO: Use a field in KeyBind to specify the gamepad index
            //foreach (int gamepadIndex in playerInfo.UsedGamepads)
            //{
            //    if (button.button != null && Gamepads[gamepadIndex].IsButtonPressed(button.button)) { return true; }
            //}
            if (SpecVR.Running && button.vrButton != null)
                if (VRControllers.Any(controller => controller.IsButtonPressed(button.vrButton.Value)))
                    return true;
            return false;
        }
        public bool IsNewKeyPress(string bindingName, PlayerInformation playerInfo = null)
            => IsKeyDown(bindingName, playerInfo) && !Last.IsKeyDown(bindingName, playerInfo);
        public bool IsNewKeyPress(KeyBinding binding, bool consultConsumed = true)
            => IsKeyDown(binding, consultConsumed) && !Last.IsKeyDown(binding, consultConsumed);
        public bool IsNewKeyPress(KeyBind button, bool consultConsumed = true)
            => (!consultConsumed || !consumedKeys.ContainsKey(button)) && IsKeyDown(button, false) && !Last.IsKeyDown(button, false);
        public bool IsNewKeyRelease(string bindingName, PlayerInformation playerInfo = null)
            => !IsKeyDown(bindingName, playerInfo) && Last.IsKeyDown(bindingName, playerInfo);
        public bool IsNewKeyRelease(KeyBinding binding, bool consultConsumed = true)
            => !IsKeyDown(binding, consultConsumed) && Last.IsKeyDown(binding, consultConsumed);
        public bool IsNewKeyRelease(KeyBind button, bool consultConsumed = true)
            => (!consultConsumed || !consumedKeys.ContainsKey(button)) && !IsKeyDown(button, false) && Last.IsKeyDown(button, false);
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
            if (limitToOne && output.LengthSquared > 1)
                output.Normalize();
            return output;
        }
        public Point MousePosition => CursorState.P;
        public bool IsMouseDown(int button)
        {
            if (button >= (CursorState?.buttons?.Length ?? 0))
                return false;
            if (button >= 0)
                return CursorState.buttons[button];
            if (button == -1)
                return MouseScrollY > 0;
            if (button == -2)
                return MouseScrollY < 0;
            if (button == -3)
                return MouseScrollX > 0;
            if (button == -4)
                return MouseScrollX < 0;
            return false;
        }
        public bool IsNewMousePress(int button)
            => button < (CursorState?.buttons?.Length ?? 0) && IsMouseDown(button) && !Last.IsMouseDown(button);
        public bool IsNewMouseRelease(int button)
            => button < (CursorState?.buttons?.Length ?? 0) && !IsMouseDown(button) && Last.IsMouseDown(button);
        public int MouseScrollY => CursorState?.ScrollY ?? 0;
        public int MouseScrollX => CursorState?.ScrollX ?? 0;


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
                        if (IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl))
                        {
                            while (count < currentString.Length - 1 && currentString[currentString.Length - 1 - count] != ' ')
                                count++;
                        }
                        count++;
                        position -= count;
                        currentString = currentString.Remove(position, count);
                    }
                    if (key == Keys.Right && position < currentString.Length)
                        position++;
                    if (key == Keys.Left && position > 0)
                        position--;

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
