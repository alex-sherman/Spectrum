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
        public ListOption<T> Selected
        {
            get { return selected; }
            set
            {
                Expanded = false;
                selected = value;
                if (OnSelectedChanged != null)
                    OnSelectedChanged(selected);
            }
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
                    if (value)
                    {
                        foreach (ListOption<T> option in Children)
                        {
                            option.Display = ElementDisplay.Visible;
                        }
                    }
                    else
                    {
                        foreach (ListOption<T> option in Children.Where(e => e != Selected))
                        {
                            option.Display = ElementDisplay.Hidden;
                        }
                    }
                }
            }
        }

        private Rectangle optionRect(int i)
        {
            return new Rectangle(Rect.X, Rect.Y + Rect.Height * (i + 1), Rect.Width, Rect.Height);
        }
        public Dropdown(params ListOption<T>[] options)
        {
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
        public override void Initialize()
        {
            base.Initialize();
            Width.Flat = 100;
            Height.Flat = (int)Font.LineSpacing;
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
            option.OnClick += option_OnClick;
            option.Display = Expanded ? ElementDisplay.Visible : ElementDisplay.Hidden;
            options.Add(option);
            AddElement(option);
        }
        public void RemoveOption(ListOption<T> option)
        {
            RemoveElement(option);
            options.Remove(option);
            if (Selected == option)
                Selected = null;
        }

        void option_OnClick(InputElement clicked)
        {
            if (!Expanded && clicked == Selected)
                Expanded = true;
            else
                Selected = clicked as ListOption<T>;
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
        public override void Draw(GameTime time, SpriteBatch spritebatch)
        {
            base.Draw(time, spritebatch);
            if (selected != null)
            {
                spritebatch.DrawString(Font, selected.Text, new Vector2(AbsoluteX, AbsoluteY),
                    Color.Black, Layer(3));
            }
        }
    }
}
