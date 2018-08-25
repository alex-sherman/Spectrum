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
        public event Action<ListOption<T>> OnSelectedChanged;
        public DropdownOptionSource<T> OptionSource = null;
        public readonly List<ListOption<T>> Options = new List<ListOption<T>>();
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
                        option.Toggle(value);
                    }
                }
            }
        }

        public Dropdown(params ListOption<T>[] options)
        {
            AddElement(childOption);
            SetOptions(options.ToList());
            OnClick += Dropdown_OnClick;
            Width = 100;
            Width.WrapContent = true;
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
            foreach (var option in Options.ToList())
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
                option.Margin.Top = 1f;
            option.OnClick += Option_OnClick;
            option.Toggle(Expanded);
            Options.Add(option);
            AddElement(option);
        }
        public void RemoveOption(ListOption<T> option)
        {
            RemoveElement(option);
            Options.Remove(option);
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
            childOption.Option = selected == null ? default(T) : selected.Option;
            childOption.Text = selected?.Text;
            childOption.Id = selected?.Id ?? 0;
            OnSelectedChanged?.Invoke(selected);
        }
        public override void Measure(int width, int height)
        {
            base.Measure(width, height);
        }
        public override void OnMeasure(int width, int height)
        {
            base.OnMeasure(width, height);
            MeasuredHeight = Font.LineSpacing;
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
