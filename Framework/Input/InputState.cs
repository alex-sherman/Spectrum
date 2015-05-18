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
using System;
using System.Collections.Generic;
#endregion

namespace Spectrum.Framework.Input
{
    /// <summary>
    /// Helper for reading input from keyboard, gamepad, and touch input. This class 
    /// tracks both the current and previous state of the input devices, and implements 
    /// query methods for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
        public static float MouseSensitivity = 0.003f;

        #region Fields

        public KeyboardState KeyboardState;
        public MouseState MouseState;
        private InputState LastInputState;

        #endregion

        #region Initialization


        public InputState()
            : this(new KeyboardState(), new MouseState(), new InputState(new KeyboardState(), new MouseState(), null)) { }

        private InputState(KeyboardState keyboardState, MouseState mouseState, InputState lastInputState)
        {
            KeyboardState = keyboardState;
            MouseState = mouseState;
            LastInputState = lastInputState;
        }


        #endregion

        #region Public Methods

        public void Update()
        {
            LastInputState.KeyboardState = KeyboardState;
            KeyboardState = Keyboard.GetState();
            LastInputState.MouseState = MouseState;
            MouseState = Mouse.GetState();
        }
        public bool IsKeyDown(string bindingName)
        {
            KeyBinding binding;
            if (!KeyBinding.KeyBindings.TryGetValue(bindingName, out binding)) throw new KeyNotFoundException("Binding not found");
            return IsKeyDown(binding);

        }
        public bool IsKeyDown(KeyBinding binding)
        {
            if (binding.modifier != null && !IsKeyDown((Keys)binding.modifier)) { return false; }
            if (binding.key1 != null) { if (IsKeyDown((Keys)binding.key1)) return true; }
            if (binding.key2 != null) { if (IsKeyDown((Keys)binding.key2)) return true; }
            if (binding.mouseButton != null) { if (IsMouseDown((int)binding.mouseButton)) return true; }
            return false;
        }
        public bool IsKeyDown(Keys key)
        {
            return KeyboardState.IsKeyDown(key);
        }

        public bool IsNewKeyPress(string bindingName)
        {
            KeyBinding binding;
            if (!KeyBinding.KeyBindings.TryGetValue(bindingName, out binding)) throw new KeyNotFoundException("Binding not found");
            return IsNewKeyPress(binding);

        }
        public bool IsNewKeyPress(KeyBinding binding)
        {
            return IsKeyDown(binding) && !LastInputState.IsKeyDown(binding);
        }
        public bool IsNewKeyPress(Keys key)
        {
            return (IsKeyDown(key) &&
                    !LastInputState.IsKeyDown(key));
        }

        public bool IsNewKeyRelease(string bindingName)
        {
            KeyBinding binding;
            if (!KeyBinding.KeyBindings.TryGetValue(bindingName, out binding)) throw new KeyNotFoundException("Binding not found");
            return IsNewKeyRelease(binding);

        }
        public bool IsNewKeyRelease(KeyBinding binding)
        {
            return !IsKeyDown(binding) && LastInputState.IsKeyDown(binding);
        }
        public bool IsNewKeyRelease(Keys key)
        {
            return (!IsKeyDown(key) &&
                    LastInputState.IsKeyDown(key));
        }


        public bool IsMouseDown(int button)
        {
            switch (button)
            {
                case 0:
                    return MouseState.LeftButton == ButtonState.Pressed;
                case 1:
                    return MouseState.MiddleButton == ButtonState.Pressed;
                case 2:
                    return MouseState.RightButton == ButtonState.Pressed;
                case 3:
                    return MouseState.XButton1 == ButtonState.Pressed;
                case 4:
                    return MouseState.XButton2 == ButtonState.Pressed;
                default:
                    return false;
            }
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
            return MouseState.ScrollWheelValue - LastInputState.MouseState.ScrollWheelValue;
        }


        #endregion
    }
}
