using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Screens.InputElements;
using Spectrum.Framework.Input;

namespace Spectrum.Framework.Screens
{
    public class MenuScreen : GameScreen
    {
        private string MenuTitle;

        public MenuScreen(string menuTitle)
        {
            MenuTitle = menuTitle;
        }
        public override void Initialize()
        {
            base.Initialize();
            TextElement Title = new TextElement(MenuTitle);
            AddElement(Title);
            Title.Center();
        }

        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            otherTookInput |= base.HandleInput(otherTookInput, input);
            if (!otherTookInput)
            {
                if (input.IsNewKeyPress("GoBack"))
                {
                    otherTookInput = true;
                    Exit();
                }
            }
            return true;
        }
    }
}
