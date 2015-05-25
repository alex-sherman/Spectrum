using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InterfaceElements
{
    public delegate void ChangedEventHandler(object sender, EventArgs e);


    public class DropdownOption
    {
        public string text;
        public object tag;

        public DropdownOption(string text, object tag)
        {
            this.tag = tag;
            this.text = text;
        }
    }

    public class Dropdown : InterfaceElement
    {
        private List<DropdownOption> options = new List<DropdownOption>();
        private DropdownOption selected = null;
        private bool expanded;
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
        }
        public override void Initialize()
        {
            base.Initialize();
            FlatWidth = 100;
            FlatHeight = (int)Font.LineSpacing;
        }
        public void SetOptions(params DropdownOption[] options)
        {
            this.options = options.ToList();
        }
        public void AddOption(DropdownOption option)
        {
            this.options.Add(option);
        }
        public void RemoveOption(DropdownOption option)
        {
            this.options.Remove(option);
        }
        public event EventHandler SelectedItemChanged;

        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            if (otherTookInput) { expanded = false; return false; }
            if (input.IsNewMousePress(0))
            {
                if (expanded && expandedRect.Contains(input.MouseState.X, input.MouseState.Y))
                {
                    for (int i = 0; i < options.Count(); i++)
                    {
                        Rectangle rect = optionRect(i);
                        if (rect.Contains(input.MouseState.X, input.MouseState.Y))
                        {
                            if (SelectedItemChanged != null)
                            {
                                SelectedItemChanged(options[i], null);
                            }
                            selected = options[i];
                            expanded = false;
                        }
                    }
                    return true;
                }
                else if (MouseInside() && !expanded)
                {
                    expanded = true;
                    return true;
                }
                else
                {
                    expanded = false;
                }
            }
            return false;
        }
        public override void Draw(GameTime time)
        {
            base.Draw(time);
            if (selected != null)
            {
                ScreenManager.CurrentManager.DrawString(Font, selected.text, new Vector2(X, Y),
                    Color.Black, Layer(3));
            }
            if (expanded)
            {
                for (int i = 0; i < options.Count(); i++)
                {
                    ScreenManager.CurrentManager.DrawString(Font, options[i].text, Rect,
                        Color.Black, Layer(2));
                }
            }
        }
        public DropdownOption Selected { get { return selected; } }
    }
}
