using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Input
{
    public struct BindingOption
    {
        public Keys? key;
        public Keys? keyModifier;
        public int? mouseButton;
        public GamepadButton? button;
        public GamepadButton? buttonModifier;
        public BindingOption(Keys? key = null, Keys? keyModifier = null, int? mouseButton = null, GamepadButton? button = null, GamepadButton? buttonModifier = null)
        {
            this.key = key;
            this.keyModifier = keyModifier;
            this.mouseButton = mouseButton;
            this.button = button;
            this.buttonModifier = buttonModifier;
        }
    }
    public struct KeyBinding
    {
        public List<BindingOption> Options;
        public KeyBinding(Keys? key = null, Keys? keyModifier = null, int? mouseButton = null, GamepadButton? button = null, GamepadButton? buttonModifier = null)
            : this(new BindingOption(key, keyModifier, mouseButton, button, buttonModifier)) { }

        public KeyBinding(BindingOption info)
        {
            Options = new List<BindingOption>();
            Options.Add(info);
        }
    }
}
