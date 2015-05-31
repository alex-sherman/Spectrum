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
        private static SpectrumMouse SpecMouse = new SpectrumMouse();

        #region Fields

        public KeyboardState KeyboardState;
        public SpectrumMouseState MouseState;
        private InputState LastInputState;


        #endregion

        #region Initialization



        public InputState()
        {
            KeyboardState = Keyboard.GetState();
            MouseState = SpecMouse.GetCurrentState();
            LastInputState = null;
        }


        #endregion

        #region Public Methods

        public void Update()
        {
            if (LastInputState == null)
                LastInputState = new InputState();
            LastInputState.KeyboardState = KeyboardState;
            KeyboardState = Keyboard.GetState();
            LastInputState.MouseState = MouseState;
            MouseState = SpecMouse.GetCurrentState();
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
            return MouseState.buttons[button];
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
    }
}
