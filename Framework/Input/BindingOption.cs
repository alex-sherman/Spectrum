using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.VR;
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
        public VRButtonBinding? vrButton;
        public BindingOption(Keys? key = null, Keys? keyModifier = null, int? mouseButton = null,
            GamepadButton? button = null, GamepadButton? buttonModifier = null,
            VRButtonBinding? vrButton = null)
        {
            this.key = key;
            this.keyModifier = keyModifier;
            this.mouseButton = mouseButton;
            this.button = button;
            this.buttonModifier = buttonModifier;
            this.vrButton = vrButton;
        }
    }
    public class KeyBinding
    {
        public List<BindingOption> Options;
        public KeyBinding(Keys? key = null, Keys? keyModifier = null, int? mouseButton = null, GamepadButton? button = null, GamepadButton? buttonModifier = null)
            : this(new BindingOption(key, keyModifier, mouseButton, button, buttonModifier)) { }

        public KeyBinding(params BindingOption[] options)
        {
            Options = options.ToList();
        }
        public void Add(BindingOption option)
        {
            Options.Add(option);
        }
    }
}
