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

        public DefaultDict<string, KeyBinding> KeyBindings = new DefaultDict<string, KeyBinding>(() => new KeyBinding(), true);
        public Dictionary<string, Axis1> Axes1 = new Dictionary<string, Axis1>();

        public static void Init()
        {
            Default = new InputLayout("Default");
            Default.Add("MenuLeft", new KeyBind(Keys.Left));
            Default.Add("MenuRight", new KeyBind(Keys.Right));
            Default.Add("MenuUp", new KeyBind(Keys.Up));
            Default.Add("MenuDown", new KeyBind(Keys.Down));
            Default.Add("MenuCycleF", new KeyBind(Keys.Tab));
            Default.Add("MenuCycleB", new KeyBind(Keys.Tab, modifiers: new KeyBind(Keys.LeftShift)));
            Default.Add("GoBack", new KeyBind(Keys.Escape));
            Default.Add("Continue", new KeyBind(Keys.Enter));
        }

        public InputLayout(string name)
        {
            if (Profiles.ContainsKey(name)) throw new ArgumentException("An input profile with that name already exists");
            Profiles[name] = this;
        }

        public void Add(string binding, KeyBind option)
        {
            KeyBindings[binding].Options.Add(option);
        }
        public static void AddBind(string binding, KeyBind option, string profile = "Default")
        {
            Profiles[profile].Add(binding, option);
        }
    }
}
