using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Screens.InputElements;
using Spectrum.Framework.Input;
using Spectrum.Framework.Content;

namespace Spectrum.Framework.Screens
{
    public class InGameScreen : GameScreen
    {
        protected int TopBarHeight = 40;
        bool dragging = false;
        Vector2 dragMouseBegin;
        Vector2 dragBegin;
        public string Title;
        public InGameScreen(string title, ScalableTexture background = null, ScalableTexture topBar = null, ScalableTexture closeButton = null)
        {
            if (title == null)
                throw new ArgumentNullException("Title cannot be null");
            this.Title = title;

        }
        public override void Initialize()
        {
            base.Initialize();
            RelativeHeight = 0;
            RelativeWidth = 0;
            FlatHeight = 100;
            FlatWidth = 100;
            X = 0;
            Y = 0;

            Element TitleContainer = new Element();
            TitleContainer.RelativeWidth = 1;
            TitleContainer.Tags.Add("ingame-window-title-container");
            AddElement(TitleContainer);
            TextElement TitleElement = new TextElement(Title);
            TitleElement.Tags.Add("ingame-window-title");
            TitleContainer.AddElement(TitleElement);
            TitleElement.Center();
            TitleContainer.FlatHeight = TitleElement.FlatHeight;
        }

        public Rectangle CloseButtonRect
        {
            get { return new Rectangle(Rect.X + Rect.Width - TopBarHeight, Rect.Y + TopBarRect.Height / 2 - 18, 38, 36); }
        }
        public Rectangle TopBarRect
        {
            get { return new Rectangle(Rect.X, Rect.Y, Rect.Width, TopBarHeight); }
        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Color borderColor = Color.Black;
            borderColor.A = 100;
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
                if (!input.MouseState.buttons[0])
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
