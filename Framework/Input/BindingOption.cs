using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.VR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Input
{
    public struct Button
    {
        public Keys? key;
        public int? mouseButton;
        public GamepadButton? button;
        public VRButtonBinding? vrButton;
        public Button[] modifiers;
        public Button(Keys key, params Button[] modifiers) : this(modifiers) { this.key = key; }
        public Button(int mouseButton, params Button[] modifiers) : this(modifiers) { this.mouseButton = mouseButton; }
        public Button(GamepadButton button, params Button[] modifiers) : this(modifiers) { this.button = button; }
        public Button(VRButtonBinding vrButton, params Button[] modifiers) : this(modifiers) { this.vrButton = vrButton; }
        private Button(params Button[] modifiers)
        {
            key = null; mouseButton = null; button = null;  vrButton = null;
            this.modifiers = modifiers;
        }
    }
    public class KeyBinding
    {
        public List<Button> Options;

        public KeyBinding(params Button[] options)
        {
            Options = options.ToList();
        }
        public KeyBinding(Keys key, Keys? keyModifier = null)
            : this(new Button(key, modifiers: keyModifier.HasValue ? new Button[] { new Button(keyModifier.Value) } : new Button[0])) { }
        public KeyBinding(int mouseButton)
            : this(new Button(mouseButton)) { }
        public KeyBinding(GamepadButton button, GamepadButton? buttonModifier = null)
            : this(new Button(button: button, modifiers: buttonModifier.HasValue ? new Button[] { new Button(buttonModifier.Value) } : new Button[0])) { }
        public void Add(Button option)
        {
            Options.Add(option);
        }
    }
}
