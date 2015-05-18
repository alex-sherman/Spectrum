using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Screens.InterfaceElements;
using Spectrum.Framework.Input;

namespace Spectrum.Framework.Screens
{
    public class InGameScreen : GameScreen
    {
        protected ScalableTexture background;
        protected ScalableTexture topBar;
        protected ScalableTexture closeButton;
        protected Rectangle rect = new Rectangle(100, 100, 200, 200);
        protected int TopBarHeight = 40;
        public override Rectangle Rect
        {
            get
            {
                return rect;
            }
        }
        bool dragging = false;
        Vector2 dragMouseBegin;
        Vector2 dragBegin;
        protected string Title = "";
        protected InGameScreen()
        {
            this.IsOverlay = true;
        }
        public override void LoadContent()
        {
            background = new ScalableTexture(@"UI\panel", 5);
            topBar = new ScalableTexture(@"UI\blue_panel", 5);
            closeButton = new ScalableTexture(@"UI\button_close", 0);

            base.LoadContent();
        }
        public override bool MouseInside(int x, int y)
        {
            return rect.Contains(x, y) || TopBarRect.Contains(x, y);
        }
        public virtual void Toggle()
        {
            if (ScreenState == ScreenState.Active)
            {
                ScreenState = ScreenState.Hidden;
            }
            else if (ScreenState == ScreenState.Hidden)
            {
                ScreenState = ScreenState.Active;
            }
        }
        public Rectangle CloseButtonRect
        {
            get { return new Rectangle(rect.X + rect.Width - TopBarHeight, TopBarRect.Y + TopBarRect.Height / 2 - 18, 38, 36); }
        }
        public Rectangle TopBarRect
        {
            get { return new Rectangle(OuterRect.X, OuterRect.Y - TopBarHeight, OuterRect.Width, TopBarHeight + topBar.BorderWidth); }
        }
        public Rectangle OuterRect
        {
            get
            {
                return new Rectangle(rect.X - background.BorderWidth, rect.Y - background.BorderWidth,
                  rect.Width + 2 * background.BorderWidth, rect.Height + 2 * background.BorderWidth);
            }
        }
        public override void Draw(GameTime gameTime, float layer)
        {
            base.DrawElements(gameTime, layer);
            if (background != null)
            {
                background.Draw(OuterRect, Manager.SpriteBatch, layer);
            }
            if (closeButton != null)
            {
                closeButton.Draw(CloseButtonRect, Manager.SpriteBatch, layer);
            }
            if (topBar != null)
            {
                topBar.Draw(TopBarRect, Manager.SpriteBatch, ScreenManager.Layer(-1, layer));
            }
            Color borderColor = Color.Black;
            borderColor.A = 100;
            if (Title != "")
            {
                Manager.DrawString(font, Title, 
                    new Vector2(
                        TopBarRect.X + TopBarRect.Width/2 - font.MeasureString(Title).X/2,
                        TopBarRect.Y + TopBarRect.Height/ 2 - font.MeasureString(Title).Y / 2), Color.LightGray, layer);
            }
        }
        public override bool HandleInput(InputState input)
        {
            bool tookInput = false;
            if (input.IsNewKeyPress("GoBack"))
            {
                input.Update();
                ScreenState = ScreenState.Hidden;
                tookInput = true;
            }
            tookInput |= HandleElementInput(input);
            if (input.IsNewMousePress(0))
            {
                if (rect.Contains(input.MouseState.X, input.MouseState.Y)) { tookInput = true; }
                if (CloseButtonRect.Contains(input.MouseState.X, input.MouseState.Y))
                {
                    ScreenState = ScreenState.Hidden;
                }
                if (TopBarRect.Contains(input.MouseState.X, input.MouseState.Y))
                {
                    dragging = true;
                    dragMouseBegin.X = input.MouseState.X;
                    dragMouseBegin.Y = input.MouseState.Y;
                    dragBegin.X = this.rect.X;
                    dragBegin.Y = this.rect.Y;
                    tookInput = true;
                }
            }
            if (input.MouseState.LeftButton == ButtonState.Released)
            {
                dragging = false;
            }
            if (dragging)
            {
                tookInput = true;
                Vector2 newPos = new Vector2(input.MouseState.X, input.MouseState.Y) - dragMouseBegin + dragBegin;

                this.rect.X = (int)newPos.X;
                this.rect.Y = (int)newPos.Y;
            }
            return tookInput;
        }
    }
}
