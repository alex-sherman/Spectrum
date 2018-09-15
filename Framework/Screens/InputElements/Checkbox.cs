using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Screens.InputElements
{
    public delegate void OnToggleChanged(bool value);
    public class Checkbox : InputElement
    {
        Element valueIndicator;
        public bool Value
        {
            get => valueIndicator.Display;
            set => valueIndicator.Toggle(value);
        }
        public event OnToggleChanged OnValueChanged = null;
        public Checkbox()
        {
            Width = 20; Height = 20;
        }
        public bool ToggleValue(bool? value = null)
        {
            bool newValue = value ?? !Value;
            if (newValue != Value && OnValueChanged != null)
                OnValueChanged(newValue);
            Value = newValue;
            return Value;
        }
        public override void Initialize()
        {
            base.Initialize();
            valueIndicator = new Element();
            valueIndicator.Positioning = PositionType.Center;
            valueIndicator.Width = 6;
            valueIndicator.Height = 6;
            valueIndicator.AddTag("toggle-indicator");
            Value = false;
            AddElement(valueIndicator);
            OnClick += (_) => ToggleValue();
        }
    }
}
