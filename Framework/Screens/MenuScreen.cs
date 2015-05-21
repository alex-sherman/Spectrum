using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Screens.InterfaceElements;

namespace Spectrum.Framework.Screens
{
    public class MenuScreen : GameScreen
    {
        private MouseState lastMouse = Mouse.GetState();
        List<InterfaceElement> centeredElements = new List<InterfaceElement>();
        List<MenuButton> menuButtons = new List<MenuButton>();
        public string MenuTitle { get; set; }

        public MenuScreen(string menuTitle)
        {
            this.MenuTitle = menuTitle;
        }
        public override void Draw(GameTime gameTime, float layer)
        {
            //UpdateMenuItemLocations();

            SpriteFont font = ScreenManager.Font;

            // title!
            Vector2 titlePosition = new Vector2(Manager.GraphicsDevice.Viewport.Width / 2, 80);
            Vector2 titleOrigin = font.MeasureString(MenuTitle) / 2;
            Color col = Color.DarkRed;
            float titleScale = 1.5f;

            ScreenManager.CurrentManager.DrawString(font, MenuTitle, titlePosition, col, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);
            base.Draw(gameTime, layer);
        }
    }
}
