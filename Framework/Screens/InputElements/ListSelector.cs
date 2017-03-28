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
    public delegate void PickedEventHandler(object picked);
    public class ListSelector : InputElement
    {
        public event InterfaceEventHandler OnPick;
        private List<ListOption> options = new List<ListOption>();
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
        public void AddOption(object tag, string text)
        {
            int optionHeight = stringHeight;
            ListOption option = new ListOption(tag, text);
            option.OnClick += new InterfaceEventHandler(optionClicked);
            this.options.Add(option);
            Height.Flat += optionHeight;
            AddElement(option);
        }
        private void optionClicked(InputElement clicked)
        {
            if (OnPick != null)
                OnPick(clicked);
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
