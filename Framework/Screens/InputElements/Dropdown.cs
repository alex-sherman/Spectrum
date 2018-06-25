using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{

    public delegate IEnumerable<ListOption<T>> DropdownOptionSource<T>();

    public class Dropdown<T> : InputElement
    {
        public event InterfaceEventHandler OnSelectedChanged;
        public DropdownOptionSource<T> OptionSource = null;
        private List<ListOption<T>> options = new List<ListOption<T>>();
        private ListOption<T> selected = null;
        private ListOption<T> childOption = new ListOption<T>();
        public T Selected
        {
            get { return childOption.Option; }
        }
        private bool _expanded;
        private bool Expanded
        {
            get { return _expanded; }
            set
            {
                if (_expanded != value)
                {
                    _expanded = value;
                    foreach (ListOption<T> option in Children.Where(c => c != childOption))
                    {
                        option.Display = value ? ElementDisplay.Visible : ElementDisplay.Hidden;
                    }
                }
            }
        }

        public Dropdown(params ListOption<T>[] options)
        {
            AddElement(childOption);
            SetOptions(options.ToList());
            OnClick += Dropdown_OnClick;
        }

        void Dropdown_OnClick(InputElement clicked)
        {
            if (OptionSource != null)
            {
                SetOptions(OptionSource());
            }
            Expanded = !Expanded;
        }
        public void ClearOptions()
        {
            foreach (var option in options.ToList())
            {
                RemoveOption(option);
            }
        }
        public void SetOptions(IEnumerable<ListOption<T>> options)
        {
            ClearOptions();
            foreach (var option in options)
            {
                AddOption(option);
            }
        }
        public void AddOption(ListOption<T> option)
        {
            if (Children.Count == 0)
                option.Margin.TopRelative = 1;
            option.OnClick += Option_OnClick;
            option.Display = Expanded ? ElementDisplay.Visible : ElementDisplay.Hidden;
            options.Add(option);
            AddElement(option);
        }
        public void RemoveOption(ListOption<T> option)
        {
            RemoveElement(option);
            options.Remove(option);
            if (selected == option)
            {
                childOption.Text = null;
                childOption.Option = default(T);
            }
        }

        void Option_OnClick(InputElement clicked)
        {
            if (!Expanded && clicked == childOption)
                Expanded = true;
            else
            {
                Select(clicked as ListOption<T>);
                Expanded = false;
            }
        }
        public void Select(ListOption<T> option)
        {
            selected = option;
            childOption.Option = selected.Option;
            childOption.Text = selected.Text;
            childOption.Id = selected.Id;
            OnSelectedChanged?.Invoke(selected);
        }
        public override void OnMeasure(int width, int height)
        {
            base.OnMeasure(width, height);
            MeasuredWidth = Math.Max(100, MeasuredWidth);
        }
        public override void Layout(Rectangle bounds)
        {
            base.Layout(bounds);
        }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            if (base.HandleInput(otherTookInput, input)) return true;

            //When handle input returns false, we should close the dropdown
            if (Expanded && input.IsNewMousePress(0))
            {
                Expanded = false;
            }
            return false;
        }
    }
}
