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
using Spectrum.Framework.Screens.InputElements;
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
        public GameScreen()
        {
            Height.Relative = 1;
            Width.Relative = 1;
            Positioning = PositionType.Relative;
        }
        public virtual void Exit()
        {
            Parent.RemoveElement(this);
        }
    }
}
