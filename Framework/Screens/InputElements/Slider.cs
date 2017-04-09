using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Input;

namespace Spectrum.Framework.Screens.InputElements
{
    public delegate void OnSliderChanged(float value);
    public class Slider : InputElement
    {
        InputElement sliderPull;
        Element sliderTrack;
        bool dragging = false;
        float sliderValue = 0;
        public event OnSliderChanged OnValueChanged = null;
        public event OnSliderChanged OnSliderFinished = null;
        public float Value
        {
            get { return sliderValue; }
            set
            {
                sliderValue = Math.Min(1, Math.Max(0, value));
            }
        }
        public Slider()
        {
            Tags.Add("slider");
        }
        public override void Initialize()
        {
            base.Initialize();
            sliderPull = new InputElement();
            sliderPull.Tags.Add("slider-pull");
            sliderPull.Width.Flat = 10;
            sliderPull.Height.Flat = 10;
            sliderPull.Positioning = PositionType.Relative;
            AddElement(sliderPull);
            sliderPull.OnClick += (_) => dragging = true;

            sliderTrack = new Element();
            sliderTrack.Tags.Add("slider-track");
            sliderTrack.Height.Flat = 2;
            sliderTrack.Positioning = PositionType.Relative;
            AddElement(sliderTrack);

            OnClick += (_) => dragging = true;
            Width.Flat = 100;
            Height.Flat = 20;
        }
        public override void OnMeasure(int width, int height)
        {
            base.OnMeasure(width, height);
            sliderTrack.Width.Flat = MeasuredWidth - 10;
        }
        public override void Layout(Rectangle bounds)
        {
            sliderTrack.Y = MeasuredHeight / 2 - sliderTrack.MeasuredHeight / 2;
            sliderTrack.X = MeasuredWidth / 2 - sliderTrack.MeasuredWidth / 2;
            sliderPull.X = (int)(sliderTrack.MeasuredWidth * sliderValue) + sliderTrack.X - sliderPull.MeasuredWidth / 2;
            sliderPull.Y = MeasuredHeight / 2 - sliderPull.MeasuredHeight / 2;
            base.Layout(bounds);
        }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            if (!input.IsMouseDown(0) || otherTookInput)
            {
                if (dragging && OnSliderFinished != null)
                    OnSliderFinished(Value);
                dragging = false;
            }
            otherTookInput |= base.HandleInput(otherTookInput, input);
            if(dragging)
            {
                otherTookInput = true;
                Value = (input.MouseState.X - sliderTrack.Rect.Left) * 1.0f / sliderTrack.Width.Size;
                if (OnValueChanged != null)
                    OnValueChanged(Value);
            }
            return otherTookInput;
        }
    }
}
