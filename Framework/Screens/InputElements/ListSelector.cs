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
            X.Flat = x;
            Y.Flat = y;
            Width.Flat = width;
        }
        public override void Initialize()
        {
            base.Initialize();
            Parent.MoveElement(this, 0);
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
            option.OnClick += new InterfaceEventHandler(optionClicked);
            this.options.Add(option);
            Height.Flat += optionHeight;
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

        public override void PositionUpdate()
        {
            base.PositionUpdate();
            int distFromBottom = Manager.Root.TotalHeight - ((int)Y.Flat + TotalHeight);
            if (distFromBottom < 0)
                Y.Flat += distFromBottom;
        }

        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            otherTookInput |= base.HandleInput(otherTookInput, input);
            if (otherTookInput || input.IsNewMousePress(0) && !Rect.Contains(Mouse.GetState().X, Mouse.GetState().Y))
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
