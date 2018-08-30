using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.VR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Input
{
    public struct KeyBind
    {
        public Keys? key;
        public int? mouseButton;
        public GamepadButton? button;
        public VRBinding? vrButton;
        public KeyBind[] modifiers;
        public KeyBind(Keys key, params KeyBind[] modifiers) : this(modifiers) { this.key = key; }
        public KeyBind(int mouseButton, params KeyBind[] modifiers) : this(modifiers) { this.mouseButton = mouseButton; }
        public KeyBind(GamepadButton button, params KeyBind[] modifiers) : this(modifiers) { this.button = button; }
        public KeyBind(VRBinding vrButton, params KeyBind[] modifiers) : this(modifiers) { this.vrButton = vrButton; }
        private KeyBind(params KeyBind[] modifiers)
        {
            key = null; mouseButton = null; button = null; vrButton = null;
            this.modifiers = modifiers;
        }
        public static implicit operator KeyBind(Keys key) => new KeyBind(key);
        public static implicit operator KeyBind(GamepadButton key) => new KeyBind(key);
        public static implicit operator KeyBind(VRBinding key) => new KeyBind(key);
        public static KeyBind operator +(KeyBind a, KeyBind b)
            => new KeyBind()
            {
                key = a.key,
                mouseButton = a.mouseButton,
                button = a.button,
                vrButton = a.vrButton,
                modifiers = a.modifiers.Union(b).ToArray()
            };

        public override bool Equals(object obj)
        {
            if (!(obj is KeyBind))
            {
                return false;
            }

            var bind = (KeyBind)obj;
            return EqualityComparer<Keys?>.Default.Equals(key, bind.key) &&
                   EqualityComparer<int?>.Default.Equals(mouseButton, bind.mouseButton) &&
                   EqualityComparer<GamepadButton?>.Default.Equals(button, bind.button) &&
                   EqualityComparer<VRBinding?>.Default.Equals(vrButton, bind.vrButton) &&
                   modifiers.Length == bind.modifiers.Length && modifiers.Zip(bind.modifiers, (a, b) => a.Equals(b)).All(b => b);
        }

        public override int GetHashCode()
        {
            var hashCode = -92915133;
            hashCode = hashCode * -1521134295 + EqualityComparer<Keys?>.Default.GetHashCode(key);
            hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(mouseButton);
            hashCode = hashCode * -1521134295 + EqualityComparer<GamepadButton?>.Default.GetHashCode(button);
            hashCode = hashCode * -1521134295 + EqualityComparer<VRBinding?>.Default.GetHashCode(vrButton);
            return hashCode;
        }
    }
    public class KeyBinding
    {
        public List<KeyBind> Options;

        public KeyBinding(params KeyBind[] options)
        {
            Options = options.ToList();
        }
        public KeyBinding(Keys key, Keys? keyModifier = null)
            : this(new KeyBind(key, modifiers: keyModifier.HasValue ? new KeyBind[] { new KeyBind(keyModifier.Value) } : new KeyBind[0])) { }
        public KeyBinding(int mouseButton)
            : this(new KeyBind(mouseButton)) { }
        public KeyBinding(GamepadButton button, GamepadButton? buttonModifier = null)
            : this(new KeyBind(button: button, modifiers: buttonModifier.HasValue ? new KeyBind[] { new KeyBind(buttonModifier.Value) } : new KeyBind[0])) { }
        public void Add(KeyBind option)
        {
            Options.Add(option);
        }
    }
}
