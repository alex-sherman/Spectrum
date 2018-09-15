using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{
    public class Button : InputElement
    {
        public readonly TextElement TextElement;
        public string Text
        {
            get => TextElement.Text;
            set => TextElement.Text = value;
        }
        public Button(string text)
        {
            TextElement = AddElement(new TextElement(text) { Positioning = PositionType.Center });
        }
    }
    public class ToggleButton : Button
    {
        public bool Value
        {
            get => HasTag("highlight");
            set
            {
                if (value)
                    AddTag("highlight");
                else
                    RemoveTag("highlight");
                OnValueChanged?.Invoke(value);
            }
        }
        public event Action<bool> OnValueChanged;
        public ToggleButton(string text) : base(text)
        {
            OnClick += (_) => Value ^= true;
        }
    }
    public class CycleButton<T> : Button
    {
        public readonly List<Tuple<T, string>> Options;
        public T Value { get; private set; }
        private int index;
        public CycleButton(List<Tuple<T, string>> options) : base(options[0].Item2)
        {
            Options = options;
            Value = options[0].Item1;
            OnClick += (_) =>
            {
                index = (index + 1) % Options.Count;
                Value = Options[index].Item1;
                Text = Options[index].Item2;
            };
        }
    }
}
