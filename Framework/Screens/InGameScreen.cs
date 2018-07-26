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
        public TextElement Title;
        public KeyBind? ToggleButton;
        public InGameScreen(string title = null)
        {
            Title = new TextElement(title);
            Display = false;
        }
        public override void Initialize()
        {
            base.Initialize();
            LayoutManager = new LinearLayoutManager(LinearLayoutType.Vertical);
            TitleContainer = new Element();
            TitleContainer.Width = 1.0;
            TitleContainer.Height.WrapContent = true;
            TitleContainer.Height = Font.LineSpacing;
            TitleContainer.Tags.Add("ingame-window-title-container");
            AddElement(TitleContainer);
            Title.Tags.Add("ingame-window-title");
            TitleContainer.AddElement(Title);
            Title.Center();
        }

        public Rectangle CloseButtonRect
        {
            get { return new Rectangle(Rect.X + Rect.Width - TitleContainer.Rect.Height, Rect.Y + TitleContainer.Rect.Height / 2 - 18, 38, 36); }
        }

        public override bool Toggle(bool? show = null)
        {
            Parent?.MoveElement(this, 0);
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
            if (!otherTookInput && ToggleButton.HasValue && input.IsNewKeyPress(ToggleButton.Value))
                Toggle();
            if (!Display)
                return otherTookInput;
            if (!otherTookInput)
            {
                if (Rect.Contains(input.MousePosition))
                    otherTookInput = true;
                if (input.IsNewKeyPress("GoBack"))
                {
                    input.Update();
                    Display = false;
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
                        Display = false;
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
