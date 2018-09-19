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
        private ListOption<T> selected = null;
        private ListOption<T> childOption = new ListOption<T>();
        private LinearLayout optionContainer = new LinearLayout() { Positioning = PositionType.Relative, AllowScrollY = true };
        public int MaxHeight
        {
            get => optionContainer.Width.WrapContent ? 0 : optionContainer.Width.Flat;
            set => optionContainer.Width = new ElementSize(value, wrapContent: value == 0);
        }
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
                    optionContainer.Display = value;
                }
            }
        }
        public Dropdown(params ListOption<T>[] options)
        {
            AddElement(new TextElement("+") { Positioning = PositionType.Relative });
            AddElement(optionContainer);
            AddElement(childOption);
            SetOptions(options.ToList());
            OnClick += Dropdown_OnClick;
            Width = new ElementSize { Flat = 100, WrapContent = true };
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
            foreach (var option in optionContainer.Children.ToList())
            {
                RemoveOption(option as ListOption<T>);
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
            option.OnClick += Option_OnClick;
            optionContainer.AddElement(option);
        }
        public void RemoveOption(ListOption<T> option)
        {
            optionContainer.RemoveElement(option);
            option.OnClick -= Option_OnClick;
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
