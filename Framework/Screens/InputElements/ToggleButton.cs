using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Screens.InputElements
{
    public delegate void OnToggleChanged(bool value);
    public class ToggleButton : Button
    {
        Element valueIndicator;
        private bool _value = false;
        public bool Value
        {
            get { return _value; }
            set
            {
                _value = value;
                valueIndicator.Display = value ? ElementDisplay.Visible : ElementDisplay.Hidden;
            }
        }
        public event OnToggleChanged OnValueChanged = null;
        public ToggleButton() : base(20, 20) { }
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
            valueIndicator.Width.Flat = 6;
            valueIndicator.Height.Flat = 6;
            valueIndicator.Tags.Add("toggle-indicator");
            valueIndicator.Display = ElementDisplay.Hidden;
            AddElement(valueIndicator);
            OnClick += (_) => ToggleValue();
        }
    }
}
