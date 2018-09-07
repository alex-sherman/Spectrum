﻿using System;
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
    public class InGameScreen : Element
    {
        protected Element TitleContainer;
        bool dragging = false;
        Vector2 dragMouseBegin;
        Vector2 dragBegin;
        public bool CaptureInputWhenFocused = false;
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
            Positioning = PositionType.Relative;
            LayoutManager = new LinearLayoutManager(LinearLayoutType.Vertical);
            TitleContainer = new Element();
            TitleContainer.Width = 1.0;
            TitleContainer.Height = new ElementSize(Font.LineSpacing, wrapContent: true);
            TitleContainer.AddTag("ingame-window-title-container");
            AddElement(TitleContainer);
            Title.AddTag("ingame-window-title");
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
            {
                input.ConsumeInput(ToggleButton.Value, KeyPressType.Press);
                Toggle();
            }
            if (!Display)
                return otherTookInput;
            if (!otherTookInput)
            {
                if (CaptureInputWhenFocused && MouseInside(input))
                    otherTookInput = true;
                if (!input.IsConsumed(Keys.Escape, KeyPressType.Press) && input.IsNewKeyPress(Keys.Escape))
                {
                    Close();
                    otherTookInput = true;
                }
                if (input.IsNewMousePress(0))
                {
                    if (Rect.Contains(input.CursorState.X, input.CursorState.Y))
                    {
                        otherTookInput = true;
                        Parent.MoveElement(this, 0);
                    }
                    if (CloseButtonRect.Contains(input.CursorState.X, input.CursorState.Y))
                    {
                        Close();
                    }
                    if (TitleContainer.Rect.Contains(input.CursorState.X, input.CursorState.Y))
                    {
                        dragging = true;
                        dragMouseBegin.X = input.CursorState.X;
                        dragMouseBegin.Y = input.CursorState.Y;
                        dragBegin.X = Rect.X;
                        dragBegin.Y = Rect.Y;
                        otherTookInput = true;
                    }
                }
                if (!input.CursorState.buttons[0])
                {
                    dragging = false;
                }
                if (dragging)
                {
                    otherTookInput = true;
                    Vector2 newPos = new Vector2(input.CursorState.X, input.CursorState.Y) - dragMouseBegin + dragBegin;

                    X = (int)newPos.X;
                    Y = (int)newPos.Y;
                }
            }
            return otherTookInput;
        }

        public virtual void Close()
        {
            dragging = false;
            Display = false;
        }
    }
}
