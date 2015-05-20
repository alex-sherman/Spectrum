using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.Screens.InterfaceElements;

namespace Spectrum.Framework.Screens
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    public class MessageBoxScreen : InGameScreen
    {
        private string _message;

        public event EventHandler<EventArgs> Option1;
        public event EventHandler<EventArgs> Option2;

        /// <summary>
        /// Constructor automatically includes the standard "A=ok, B=cancel"
        /// usage text prompt.
        /// </summary>
        public MessageBoxScreen(string message)
            : this(message, "OK", "Cancel")
        { }

        /// <summary>
        /// Constructor lets the caller specify whether to include the standard
        /// usage text prompt.
        /// </summary>
        public MessageBoxScreen(string message, string option1, string option2)
            : base("")
        {
            FlatWidth = 400;
            FlatHeight = 100;
            X = Manager.Viewport.Width / 2 - Width / 2;
            Y = Manager.Viewport.Height / 2 - Height / 2;
            new Button(this, 320, 60, option1).OnClick += (InterfaceElement clicked) => { Option1(this, null); };
            new Button(this, 320, 80, option2).OnClick += (InterfaceElement clicked) => { Option2(this, null); };
            font = ScreenManager.Font;
            _message = message;
        }

        public override void LoadContent()
        {
            //background = Manager.TextureLoader.Texture("gradient.png");
        }
        public override void Draw(GameTime gameTime, float layer)
        {
            base.Draw(gameTime, layer);
            Manager.DrawString(font, _message, new Vector2(Rect.X + 10, Rect.Y + 10), Color.White, ScreenManager.Layer(1, layer));
        }
    }
}