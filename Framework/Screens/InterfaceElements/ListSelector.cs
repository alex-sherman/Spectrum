using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InterfaceElements
{
    public delegate void PickedEventHandler(object picked);
    public class ListSelector : InterfaceElement
    {
        public event PickedEventHandler OnPick;
        private List<ListOption> options = new List<ListOption>();
        private int stringHeight;
        //The list selector's _rect is in absolute coordinates unlike other interface elements
        public ListSelector(Element parent, int x, int y)
        {
            stringHeight = (int)Font.LineSpacing;
        }
        public void AddOption(object tag, string text)
        {
            int optionHeight = stringHeight + Texture.BorderWidth * 2;
            ListOption option = new ListOption(tag, text);
            option.OnClick += new InterfaceEventHandler(optionClicked);
            this.options.Add(option);
            FlatHeight += optionHeight;
        }
        private void optionClicked(InterfaceElement clicked)
        {
            OnPick((clicked as ListOption).Tag);
            Close();
        }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            if (input.IsNewMousePress(0) && !Rect.Contains(Mouse.GetState().X, Mouse.GetState().Y))
            {
                Close();
            }
            return base.HandleInput(otherTookInput, input);
        }
        public void Close()
        {
            foreach (ListOption option in options)
            {
                Parent.RemoveElement(option);
            }
            Parent.RemoveElement(this);
        }
    }
}
