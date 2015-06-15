using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Spectrum.Framework.Input
{
    public class InputLayout
    {
        public static Dictionary<string, InputLayout> Profiles = new Dictionary<string, InputLayout>();
        public static InputLayout Default;

        public Dictionary<string, KeyBinding> KeyBindings = new Dictionary<string, KeyBinding>();
        public Dictionary<string, Axis1> Axes1 = new Dictionary<string, Axis1>();

        public static void Init()
        {
            Default = new InputLayout("Default");
            Default.KeyBindings = new Dictionary<string, KeyBinding>()
                {
                    {"MenuLeft", new KeyBinding(Keys.Left)},
                    {"MenuRight", new KeyBinding(Keys.Right)},
                    {"MenuUp", new KeyBinding(Keys.Up)},
                    {"MenuDown", new KeyBinding(Keys.Down)},
                    {"MenuCycleF", new KeyBinding(Keys.Tab)},
                    {"MenuCycleB", new KeyBinding(Keys.Tab, keyModifier: Keys.LeftShift)},
                    {"GoBack", new KeyBinding(Keys.Escape)},
                    {"Continue", new KeyBinding(Keys.Enter)},
                };
        }

        public InputLayout(string name)
        {
            if (Profiles.ContainsKey(name)) throw new ArgumentException("An input profile with that name already exists");
            Profiles[name] = this;
        }
    }
}
