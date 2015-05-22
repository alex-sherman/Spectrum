using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Screens.InterfaceElements;
using Spectrum.Framework.Input;
using Spectrum.Framework.Content;

namespace Spectrum.Framework.Screens
{
    public class InGameScreen : GameScreen
    {
        public static ScalableTexture DefaultBackground = new ScalableTexture(ContentHelper.Blank, 0);
        public static ScalableTexture DefaultTopBar = new ScalableTexture(ContentHelper.Blank, 0);
        public static ScalableTexture DefaultCloseButton = new ScalableTexture(ContentHelper.Blank, 0);
        protected ScalableTexture Background;
        protected ScalableTexture TopBar;
        protected ScalableTexture CloseButton;
        protected int TopBarHeight = 40;
        bool dragging = false;
        Vector2 dragMouseBegin;
        Vector2 dragBegin;
        public string Title;
        public InGameScreen(string title, ScalableTexture background = null, ScalableTexture topBar = null, ScalableTexture closeButton = null)
        {
            Background = background ?? DefaultBackground;
            TopBar = topBar ?? DefaultTopBar;
            CloseButton = closeButton ?? DefaultCloseButton;
            RelativeHeight = 0;
            RelativeWidth = 0;
            FlatHeight = 100;
            FlatWidth = 100;
            X = 0;
            Y = 0;

            this.Title = title;
            this.IsOverlay = true;
        }
        public override bool MouseInside(int x, int y)
        {
            return Rect.Contains(x, y);
        }

        public Rectangle CloseButtonRect
        {
            get { return new Rectangle(Rect.X + Rect.Width - TopBarHeight, Rect.Y + TopBarRect.Height / 2 - 18, 38, 36); }
        }
        public Rectangle TopBarRect
        {
            get { return new Rectangle(Rect.X, Rect.Y, Rect.Width, TopBarHeight + TopBar.BorderWidth); }
        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (Background != null)
            {
                Background.Draw(Rect, Manager.SpriteBatch, Z);
            }
            if (TopBar != null)
            {
                TopBar.Draw(TopBarRect, Manager.SpriteBatch, Layer(1));
            }
            if (CloseButton != null)
            {
                CloseButton.Draw(CloseButtonRect, Manager.SpriteBatch, Layer(2));
            }
            Color borderColor = Color.Black;
            borderColor.A = 100;
            if (Title != "")
            {
                Title = Z.ToString();
                Manager.DrawString(font, Title,
                    new Vector2(
                        TopBarRect.X + TopBarRect.Width / 2 - font.MeasureString(Title).X / 2,
                        TopBarRect.Y + TopBarRect.Height / 2 - font.MeasureString(Title).Y / 2), Color.LightGray, Layer(2));
            }
        }

        public override ElementDisplay Toggle()
        {
            Parent.MoveElement(this, 0);
            return base.Toggle();
        }

        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            otherTookInput |= base.HandleInput(otherTookInput, input);
            if (!otherTookInput)
            {
                if (input.IsNewKeyPress("GoBack"))
                {
                    input.Update();
                    Display = ElementDisplay.Hidden;
                    otherTookInput = true;
                }
                if (input.IsNewMousePress(0))
                {
                    if (Rect.Contains(input.MouseState.X, input.MouseState.Y))
                    {
                        otherTookInput = true;
                        Parent.MoveElement(this, 0);
                    }
                    if (CloseButtonRect.Contains(input.MouseState.X, input.MouseState.Y))
                    {
                        Display = ElementDisplay.Hidden;
                    }
                    if (TopBarRect.Contains(input.MouseState.X, input.MouseState.Y))
                    {
                        dragging = true;
                        dragMouseBegin.X = input.MouseState.X;
                        dragMouseBegin.Y = input.MouseState.Y;
                        dragBegin.X = Rect.X;
                        dragBegin.Y = Rect.Y;
                        otherTookInput = true;
                    }
                }
                if (input.MouseState.LeftButton == ButtonState.Released)
                {
                    dragging = false;
                }
                if (dragging)
                {
                    otherTookInput = true;
                    Vector2 newPos = new Vector2(input.MouseState.X, input.MouseState.Y) - dragMouseBegin + dragBegin;

                    X = (int)newPos.X;
                    Y = (int)newPos.Y;
                }
            }
            return otherTookInput;
        }
    }
}
