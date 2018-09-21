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
    public class InputElement : Element
    {
        public event Action<InputElement> OnClick;
        public event Action<InputElement> OnRightClick;
        public object Data;
        public InputElement()
        {
            RegisterHandler(new KeyBind(0), (input) =>
            {
                if (OnClick != null && MouseInside(input))
                {
                    OnClick(this);
                    return true;
                }
                return false;
            });
            RegisterHandler(new KeyBind(1), (input) =>
            {
                if (OnRightClick != null && MouseInside(input))
                {
                    OnRightClick(this);
                    return true;
                }
                return false;
            });
        }
        public void Click() => OnClick?.Invoke(this);
        public void RightClick() => OnRightClick?.Invoke(this);
    }
}
