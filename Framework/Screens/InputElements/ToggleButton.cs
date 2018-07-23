﻿using Microsoft.Xna.Framework;
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
        public bool Value
        {
            get => valueIndicator.Display;
            set => valueIndicator.Display = value;
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
            valueIndicator.Width = 6;
            valueIndicator.Height = 6;
            valueIndicator.Tags.Add("toggle-indicator");
            Value = false;
            AddElement(valueIndicator);
            OnClick += (_) => ToggleValue();
        }
    }
}
