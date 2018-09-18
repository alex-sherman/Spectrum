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
        private readonly List<T> values;
        public T Value { get; private set; }
        public int Index { get; private set; }
        public CycleButton(List<Tuple<T, string>> options) : base(options[0].Item2)
        {
            Options = options;
            Value = options[0].Item1;
            values = options.Select(t => t.Item1).ToList();
            OnClick += (_) => Cycle();
        }
        public void Cycle(int amount = 1)
        {
            SetIndex((Index + amount) % Options.Count);
        }
        public void SetIndex(int index)
        {
            this.Index = index;
            Value = Options[index].Item1;
            Text = Options[index].Item2;
        }
        public bool SetValue(T value)
        {
            var i = values.IndexOf(value);
            if (i >= 0)
            {
                SetIndex(i);
                return true;
            }
            return false;
        }
    }
}
