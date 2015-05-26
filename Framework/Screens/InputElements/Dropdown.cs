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
            Tag = tag;
            Text = text;
        }
        public override void Initialize()
        {
            base.Initialize();
            RelativeHeight = 1;
            RelativeWidth = 1;
        }
        public override void Draw(GameTime time)
        {
            base.Draw(time);
            if (Text != null)
            {
                Vector2 pos = new Vector2(Rect.X, Rect.Y);
                ScreenManager.CurrentManager.DrawString(Font, Text, pos, FontColor, Layer(1));
            }
        }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            return base.HandleInput(otherTookInput, input);
        }
    }

    public class Dropdown : InputElement
    {
        private List<DropdownOption> options = new List<DropdownOption>();
        private DropdownOption selected = null;
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
            SetOptions(options);
            OnClick += Dropdown_OnClick;
        }

        void Dropdown_OnClick(InputElement clicked)
        {
            Expanded = !Expanded;
        }
        public override void Initialize()
        {
            base.Initialize();
            FlatWidth = 100;
            FlatHeight = (int)Font.LineSpacing;
        }
        public void SetOptions(params DropdownOption[] options)
        {
            foreach (DropdownOption option in options)
            {
                AddOption(option);
            }
        }
        public void AddOption(DropdownOption option)
        {
            if (Children.Count == 0)
                option.Margin.TopRelative = 1;
            option.OnClick += option_OnClick;
            option.Display = ElementDisplay.Hidden;
            AddElement(option);
        }

        void option_OnClick(InputElement clicked)
        {
            Selected = clicked as DropdownOption;
        }
        public event EventHandler SelectedItemChanged;

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
        public override void Draw(GameTime time)
        {
            base.Draw(time);
            if (selected != null)
            {
                ScreenManager.CurrentManager.DrawString(Font, selected.Text, new Vector2(X, Y),
                    Color.Black, Layer(3));
            }
            if (Expanded)
            {
                for (int i = 0; i < options.Count(); i++)
                {
                    ScreenManager.CurrentManager.DrawString(Font, options[i].Text, Rect,
                        Color.Black, Layer(2));
                }
            }
        }
        public DropdownOption Selected
        {
            get { return selected; }
            set
            {
                Expanded = false;
                selected = value;
                if (SelectedItemChanged != null)
                    SelectedItemChanged(selected, null);
            }
        }
    }
}
