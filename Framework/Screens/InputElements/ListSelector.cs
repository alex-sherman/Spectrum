using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{
    public delegate void PickedEventHandler<T>(int id, T picked);
    public class ListSelector<T>  : InputElement
    {
        public event PickedEventHandler<T> OnPick;
        private List<ListOption<T>> options = new List<ListOption<T>>();
        private int stringHeight;
        //The list selector's _rect is in absolute coordinates unlike other interface elements
        public ListSelector(Element parent, int x, int y, int width)
        {
            Positioning = PositionType.Absolute;
            X = x;
            Y = y;
            Width = new ElementSize() { Flat = width, WrapContent = true };
            LayoutManager = new LinearLayoutManager();
        }
        public override void Initialize()
        {
            base.Initialize();
            stringHeight = (int)Font.LineSpacing;
        }
        public void AddOption(string text, T tag)
        {
            int id = Children.Select(ele => ele is ListOption<T> ? (ele as ListOption<T>).Id : 0).DefaultIfEmpty(0).Max() + 1;
            AddOption(id, text, tag);
        }
        public void AddOption(int id, string text, T tag = default(T))
        {
            int optionHeight = stringHeight;
            ListOption<T> option = new ListOption<T>(id, text, tag);
            option.OnClick += optionClicked;
            options.Add(option);
            AddElement(option);
        }
        private void optionClicked(InputElement clicked)
        {
            if (OnPick != null)
            {
                var option = clicked as ListOption<T>;
                OnPick(option.Id, option.Option);
            }
            Close();
        }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            if (!Display)
                return false;
            otherTookInput |= base.HandleInput(otherTookInput, input);
            if (otherTookInput || input.IsNewMousePress(0) && !Rect.Contains(input.MousePosition))
            {
                Close();
            }
            return true;
        }
        public void Close()
        {
            Parent.RemoveElement(this);
        }
    }
}
