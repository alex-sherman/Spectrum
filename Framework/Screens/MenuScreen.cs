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
        int menuButtonIndex = 0;
        public string MenuTitle { get; set; }

        public MenuScreen(string menuTitle)
        {
            this.MenuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }
        public void CenterElement(InterfaceElement element)
        {
            int x = Manager.Viewport.Width / 2 - element.Rect.Width / 2;
            int y = 150;
            foreach (InterfaceElement centeredElement in centeredElements)
            {
                y += centeredElement.Rect.Height + 10;
            }
            element.Position = new Vector2(x, y);
            centeredElements.Add(element);
        }
        public override void AddElement(InterfaceElement element)
        {
            base.AddElement(element);
            if (element is MenuButton)
            {
                menuButtons.Add(element as MenuButton);
            }
        }
        public override void RemoveElement(InterfaceElement element)
        {
            base.RemoveElement(element);
            if (element is MenuButton)
            {
                if (menuButtonIndex > menuButtons.IndexOf(element as MenuButton)) { menuButtonIndex--; }
                menuButtons.Remove(element as MenuButton);
            }
        }
        public override void Draw(GameTime gameTime, float layer)
        {
            //UpdateMenuItemLocations();

            SpriteFont font = ScreenManager.Font;

            // be fancy
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // title!
            Vector2 titlePosition = new Vector2(Manager.GraphicsDevice.Viewport.Width / 2, 80);
            Vector2 titleOrigin = font.MeasureString(MenuTitle) / 2;
            Color col = Color.DarkRed * TransitionAlpha;
            float titleScale = 1.5f;

            titlePosition.Y -= transitionOffset * 100;

            ScreenManager.CurrentManager.DrawString(font, MenuTitle, titlePosition, col, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);
            base.Draw(gameTime, layer);
        }
    }
}
