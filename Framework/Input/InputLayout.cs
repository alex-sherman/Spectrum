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
            Default.Add("MenuLeft", new Button(Keys.Left));
            Default.Add("MenuRight", new Button(Keys.Right));
            Default.Add("MenuUp", new Button(Keys.Up));
            Default.Add("MenuDown", new Button(Keys.Down));
            Default.Add("MenuCycleF", new Button(Keys.Tab));
            Default.Add("MenuCycleB", new Button(Keys.Tab, modifiers: new Button(Keys.LeftShift)));
            Default.Add("GoBack", new Button(Keys.Escape));
            Default.Add("Continue", new Button(Keys.Enter));
        }

        public InputLayout(string name)
        {
            if (Profiles.ContainsKey(name)) throw new ArgumentException("An input profile with that name already exists");
            Profiles[name] = this;
        }

        public void Add(string binding, Button option)
        {
            KeyBindings[binding].Options.Add(option);
        }
        public static void AddBind(string binding, Button option, string profile = "Default")
        {
            Profiles[profile].Add(binding, option);
        }
    }
}
