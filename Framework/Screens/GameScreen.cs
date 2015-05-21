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
    /// A screen is a single layer that has update and draw logic, and which
    /// can be combined with other layers to build up a complex menu system.
    /// For instance the main menu, the options menu, the "are you sure you
    /// want to quit" message box, and the main game itself are all implemented
    /// as screens.
    /// </summary>
    public abstract class GameScreen : Element
    {

        protected SpriteFont font;

        #region Properties

        public virtual bool MouseInside(int x, int y)
        {
            return false;
        }
        public bool IsOverlay = false;
        public bool HasFocus { get; protected set; }
        public bool Focusable = true;
        public bool IsExiting = false;

        /// <summary>
        /// Gets the manager that this screen belongs to.
        /// </summary>
        public ScreenManager Manager { get { return ScreenManager.CurrentManager; } }

        #endregion

        public GameScreen() : this(ScreenManager.Font) { }

        public GameScreen(SpriteFont font)
            : base(null)
        {
            this.font = font;
            RelativeHeight = 1;
            RelativeWidth = 1;
            Positioning = PositionType.Absolute;
        }

        public virtual void FocusChanged(bool gainFocus)
        {
            HasFocus = gainFocus;
        }

        public void ExitScreen()
        {
            IsExiting = true;
        }
    }
}
