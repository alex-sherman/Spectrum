#region File Description
//-----------------------------------------------------------------------------
// GameScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Screens.InterfaceElements;
using System.Collections.Generic;
using Spectrum.Framework.Input;
#endregion

namespace Spectrum.Framework.Screens
{
    /// <summary>
    /// Enum describes the screen transition state.
    /// </summary>
    public enum ScreenState
    {
        TransitionOn,
        Active,
        TransitionOff,
        Hidden,
        Exiting
    }


    /// <summary>
    /// A screen is a single layer that has update and draw logic, and which
    /// can be combined with other layers to build up a complex menu system.
    /// For instance the main menu, the options menu, the "are you sure you
    /// want to quit" message box, and the main game itself are all implemented
    /// as screens.
    /// </summary>
    public abstract class GameScreen
    {

        protected SpriteFont font;
        protected List<InterfaceElement> elements = new List<InterfaceElement>();

        #region Properties

        public virtual Rectangle Rect { get { return Manager.Viewport.Bounds; } }

        public virtual bool MouseInside(int x, int y)
        {
            return false;
        }
        public bool IsOverlay = false;
        public bool HasFocus { get; protected set; }
        public bool Focusable = true;


        public TimeSpan TransitionOnTime
        {
            get { return transitionOnTime; }
            protected set { transitionOnTime = value; }
        }
        TimeSpan transitionOnTime = TimeSpan.Zero;

        public TimeSpan TransitionOffTime
        {
            get { return transitionOffTime; }
            protected set { transitionOffTime = value; }
        }
        TimeSpan transitionOffTime = TimeSpan.Zero;

        public float TransitionPosition
        {
            get { return transitionPosition; }
            protected set { transitionPosition = value; }
        }

        float transitionPosition = -1;


        public float TransitionAlpha
        {
            get { return 1 - Math.Abs(TransitionPosition); }
        }

        public ScreenState ScreenState;

        public bool IsExiting { get { return ScreenState == ScreenState.TransitionOff; } }

        public bool IsActive
        {
            get
            {
                return ScreenState == ScreenState.Active;
            }
        }


        /// <summary>
        /// Gets the manager that this screen belongs to.
        /// </summary>
        public ScreenManager Manager { get { return ScreenManager.CurrentManager; } }

        #endregion

        public GameScreen()
        {
            font = ScreenManager.Font;
        }

        public GameScreen(SpriteFont font)
        {
            this.font = font;
        }

        public virtual void AddElement(InterfaceElement element)
        {
            elements.Add(element);
        }
        public virtual void RemoveElement(InterfaceElement element)
        {
            elements.Remove(element);
        }

        public virtual void LoadContent()
        {
        }

        public virtual void UnloadContent() { }

        public virtual void Update(GameTime gameTime)
        {
            switch (ScreenState)
            {
                case ScreenState.TransitionOn:
                    // Otherwise the screen should transition on and become active.
                    if (!UpdateTransition(gameTime, transitionOnTime, 1, true))
                    {
                        // Still busy transitioning.
                        ScreenState = ScreenState.Active;
                    }
                    break;
                case ScreenState.Active:
                    break;
                case ScreenState.TransitionOff:
                    if (!UpdateTransition(gameTime, transitionOffTime, -1, false))
                    {
                        // When the transition finishes, remove the screen.
                        ScreenState = ScreenState.Hidden;
                    }
                    break;
                case ScreenState.Exiting:
                    if (!UpdateTransition(gameTime, transitionOffTime, -1, false))
                    {
                        // When the transition finishes, remove the screen.
                        Manager.RemoveScreen(this);
                    }
                    break;
                case ScreenState.Hidden:
                    break;
                default:
                    break;
            }
        }

        bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction, bool goToCenter)
        {
            // How much should we move by?
            float transitionDelta;

            if (time == TimeSpan.Zero)
                transitionDelta = 1;
            else
                transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds /
                                          time.TotalMilliseconds);

            // Update the transition position.
            transitionPosition += transitionDelta * direction;

            // Did we reach the end of the transition?
            if ((((direction < 0) && (transitionPosition <= -1)) && !goToCenter) || ((direction < 0) && (transitionPosition <= 0) && goToCenter) ||
                (((direction > 0) && (transitionPosition >= 1)) && !goToCenter) || (((direction > 0) && (transitionPosition >= 0)) && goToCenter))
            {
                if (goToCenter) { transitionPosition = 0; }
                else { transitionPosition = direction; }
                return false;
            }
            // Otherwise we are still busy transitioning.
            return true;
        }


        public bool HandleElementInput(InputState input)
        {
            bool inputTaken = false;
            List<InterfaceElement> toUpdate = elements.ToList();
            toUpdate.Reverse();
            foreach (InterfaceElement element in toUpdate)
            {
                inputTaken |= element.HandleInput(inputTaken, input);
            }
            return inputTaken;
        }
        public virtual bool HandleInput(InputState input)
        {
            HandleElementInput(input);
            return true;
        }



        protected void DrawElements(GameTime gameTime, float layer)
        {
            foreach (InterfaceElement element in elements)
            {
                element.Draw(gameTime, ScreenManager.Layer(1, layer));
            }
        }
        public virtual void Draw(GameTime gameTime, float layer)
        {
            DrawElements(gameTime, layer);
        }
        public virtual void FocusChanged(bool gainFocus)
        {
            HasFocus = gainFocus;
        }


        public void ExitScreen()
        {
            ScreenState = ScreenState.Exiting;
        }
    }
}
