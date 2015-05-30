using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Input
{
    public class KeyBinding
    {
        public static Dictionary<string, KeyBinding> KeyBindings = new Dictionary<string, KeyBinding>()
        {
            {"MenuLeft", new KeyBinding(Keys.Left)},
            {"MenuRight", new KeyBinding(Keys.Right)},
            {"MenuUp", new KeyBinding(Keys.Up)},
            {"MenuDown", new KeyBinding(Keys.Down)},
            {"MenuCycleF", new KeyBinding(Keys.Tab)},
            {"MenuCycleB", new KeyBinding(Keys.Tab, modifier: Keys.LeftShift)},
            {"GoBack", new KeyBinding(Keys.Escape)},
            {"Continue", new KeyBinding(Keys.Enter)},
        };


        public Keys? key1 { get; set; }
        public Keys? key2 { get; set; }
        public Keys? modifier { get; set; }
        public int? mouseButton { get; set; }
        public KeyBinding(Keys? key1 = null, Keys? key2 = null, int? mouseButton = null, Keys? modifier = null)
        {
            this.key1 = key1;
            this.key2 = key2;
            this.modifier = modifier;
            this.mouseButton = mouseButton;
        }
    }
}
