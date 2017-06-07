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

        public DefaultDict<string, List<BindingOption>> KeyBindings = new DefaultDict<string, List<BindingOption>>(() => new List<BindingOption>(), true);
        public Dictionary<string, Axis1> Axes1 = new Dictionary<string, Axis1>();

        public static void Init()
        {
            Default = new InputLayout("Default");
            Default.Add("MenuLeft", new BindingOption(Keys.Left));
            Default.Add("MenuRight", new BindingOption(Keys.Right));
            Default.Add("MenuUp", new BindingOption(Keys.Up));
            Default.Add("MenuDown", new BindingOption(Keys.Down));
            Default.Add("MenuCycleF", new BindingOption(Keys.Tab));
            Default.Add("MenuCycleB", new BindingOption(Keys.Tab, keyModifier: Keys.LeftShift));
            Default.Add("GoBack", new BindingOption(Keys.Escape));
            Default.Add("Continue", new BindingOption(Keys.Enter));
        }

        public InputLayout(string name)
        {
            if (Profiles.ContainsKey(name)) throw new ArgumentException("An input profile with that name already exists");
            Profiles[name] = this;
        }

        public void Add(string binding, BindingOption option)
        {
            KeyBindings[binding].Add(option);
        }
    }
}
