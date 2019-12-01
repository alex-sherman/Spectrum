using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Input;

namespace Spectrum.Framework.Screens.InputElements
{
    public class Slider : InputElement
    {
        InputElement sliderPull;
        Element sliderTrack;
        bool dragging = false;
        float sliderValue = 0;
        public event Action<float> OnValueChanged = null;
        public event Action<float> OnSliderFinished = null;
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
            AddTag("slider");
            Width = 100;
            Height = 20;
        }
        public override void Initialize()
        {
            base.Initialize();
            sliderPull = new InputElement();
            sliderPull.AddTag("slider-pull");
            sliderPull.Height = 0.5f;
            sliderPull.Positioning = PositionType.Relative;
            AddElement(sliderPull);
            sliderPull.OnClick += (_) => dragging = true;

            sliderTrack = new Element();
            sliderTrack.AddTag("slider-track");
            sliderTrack.Height = 2;
            sliderTrack.Positioning = PositionType.Relative;
            AddElement(sliderTrack);

            OnClick += (_) =>
                dragging = true;
        }
        public override void OnMeasure(int width, int height)
        {
            base.OnMeasure(width, height);
            sliderPull.Width = sliderPull.MeasuredHeight;
            sliderTrack.Width = MeasuredWidth - Padding.WidthTotal(width) - sliderPull.MeasuredHeight;
        }
        public override void Layout(Rectangle bounds)
        {
            sliderTrack.Y = Rect.Height / 2 - sliderTrack.MeasuredHeight / 2;
            sliderTrack.X = Rect.Width / 2 - sliderTrack.MeasuredWidth / 2;
            sliderPull.X = (int)(sliderTrack.MeasuredWidth * sliderValue) + sliderTrack.X - sliderPull.MeasuredWidth / 2;
            sliderPull.Y = Rect.Height / 2 - sliderPull.MeasuredHeight / 2;
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
            if (dragging)
            {
                otherTookInput = true;
                Value = (input.MousePosition.X - sliderTrack.Rect.Left) * 1.0f / sliderTrack.MeasuredWidth;
                OnValueChanged?.Invoke(Value);
            }
            return otherTookInput;
        }
    }
}
