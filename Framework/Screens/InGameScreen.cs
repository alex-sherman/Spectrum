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
        protected Element TitleContainer;
        bool dragging = false;
        Vector2 dragMouseBegin;
        Vector2 dragBegin;
        public string Title;
        public InGameScreen(string title)
        {
            if (title == null)
                throw new ArgumentNullException("Title cannot be null");
            this.Title = title;

        }
        public override void Initialize()
        {
            base.Initialize();
            LayoutManager = new LinearLayoutManager();
            TitleContainer = new Element();
            TitleContainer.Width.Type = SizeType.MatchParent;
            TitleContainer.Tags.Add("ingame-window-title-container");
            AddElement(TitleContainer);
            TextElement TitleElement = new TextElement(Title);
            TitleElement.Tags.Add("ingame-window-title");
            TitleContainer.AddElement(TitleElement);
            TitleElement.Center();
            TitleContainer.Height.Type = SizeType.WrapContent;
        }

        public Rectangle CloseButtonRect
        {
            get { return new Rectangle(Rect.X + Rect.Width - TitleContainer.Rect.Height, Rect.Y + TitleContainer.Rect.Height / 2 - 18, 38, 36); }
        }

        public override ElementDisplay Toggle(bool? show = null)
        {
            Parent.MoveElement(this, 0);
            return base.Toggle(show);
        }

        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            bool childTookInput = base.HandleInput(otherTookInput, input);
            if (childTookInput && !otherTookInput)
            {
                Parent.MoveElement(this, 0);
            }
            otherTookInput |= childTookInput;

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
                    if (TitleContainer.Rect.Contains(input.MouseState.X, input.MouseState.Y))
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
