using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{
    public delegate void ChangedEventHandler(object sender, EventArgs e);


    public class DropdownOption : InputElement
    {
        public string Text;

        public DropdownOption(string text, object tag)
        {
            Data = tag;
            Text = text;
        }
        public override void Initialize()
        {
            base.Initialize();
            Height.Relative = 1;
            Width.Relative = 1;
        }
        public override void Draw(GameTime time, SpriteBatch spritebatch)
        {
            base.Draw(time, spritebatch);
            if (Text != null)
            {
                Vector2 pos = new Vector2(Rect.X, Rect.Y);
                spritebatch.DrawString(Font, Text, pos, FontColor, Layer(1));
            }
        }
    }

    public class Dropdown : InputElement
    {
        public event InterfaceEventHandler OnSelectedChanged;
        private List<DropdownOption> options = new List<DropdownOption>();
        private DropdownOption selected = null;
        public DropdownOption Selected
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
                        foreach (DropdownOption option in Children)
                        {
                            option.Display = ElementDisplay.Visible;
                        }
                    }
                    else
                    {
                        foreach (DropdownOption option in Children)
                        {
                            option.Display = ElementDisplay.Hidden;
                        }
                    }
                }
            }
        }
        private Rectangle expandedRect
        {
            get
            {
                int height = Rect.Height * options.Count();
                return new Rectangle(Rect.X, Rect.Y + Rect.Height, Rect.Width, height);
            }
        }
        private Rectangle optionRect(int i)
        {
            return new Rectangle(Rect.X, Rect.Y + Rect.Height * (i + 1), Rect.Width, Rect.Height);
        }
        public Dropdown(params DropdownOption[] options)
        {
            SetOptions(options.ToList());
            OnClick += Dropdown_OnClick;
        }

        void Dropdown_OnClick(InputElement clicked)
        {
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
        public void SetOptions(List<DropdownOption> options)
        {
            ClearOptions();
            foreach (var option in options)
            {
                AddOption(option);
            }
        }
        public void AddOption(DropdownOption option)
        {
            if (Children.Count == 0)
                option.Margin.TopRelative = 1;
            option.OnClick += option_OnClick;
            option.Display = Expanded ? ElementDisplay.Visible : ElementDisplay.Hidden;
            options.Add(option);
            AddElement(option);
        }
        public void RemoveOption(DropdownOption option)
        {
            RemoveElement(option);
            options.Remove(option);
            if (Selected == option)
                Selected = null;
        }

        void option_OnClick(InputElement clicked)
        {
            Selected = clicked as DropdownOption;
        }

        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            if (base.HandleInput(otherTookInput, input)) return true;

            //When handle input returns false, we should close the dropdown
            if (input.IsNewMousePress(0))
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
