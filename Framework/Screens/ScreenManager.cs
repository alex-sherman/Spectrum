#region File Description
//-----------------------------------------------------------------------------
// ScreenManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.Input;
using Spectrum.Framework.Content;
#endregion

namespace Spectrum.Framework.Screens
{
    /// <summary>
    /// The screen manager is a component which manages one or more GameScreen
    /// instances. It maintains a stack of screens, calls their Update and Draw
    /// methods at the appropriate times, and automatically routes input to the
    /// topmost active screen.
    /// </summary>
    public class ScreenManager : DrawableGameComponent
    {
        #region Fields

        InputState input = new InputState();

        public SpriteBatch SpriteBatch;
        public static ScreenManager CurrentManager;
        public Viewport Viewport;
        public ContentHelper TextureLoader { get; private set; }

        #endregion

        #region Properties
        public RootElement Root { get; private set; }
        public bool IsActive
        {
            get { return Game.IsActive; }
        }

        #endregion

        #region Initialization


        public ScreenManager(SpectrumGame game, ContentHelper textureLoader)
            : base(game)
        {
            Root = new RootElement(this);
            Root.Initialize();
            TextureLoader = textureLoader;
            CurrentManager = this;
            Game.Services.AddService(typeof(ScreenManager), this);
            Viewport = game.GraphicsDevice.Viewport;
            game.OnScreenResize += OnScreenResize;
        }

        public void OnScreenResize(object sender, EventArgs args)
        {
            Viewport = (sender as SpectrumGame).GraphicsDevice.Viewport;
        }


        /// <summary>
        /// Initializes the screen manager component.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            SpriteBatch = new SpriteBatch(GraphicsDevice);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            foreach(var addScreen in addScreens)
                Root.AddElement(addScreen, 0);
            addScreens.Clear();

            foreach(var removeScreen in removeScreens)
                Root.RemoveElement(removeScreen);
            removeScreens.Clear();

            // Read the keyboard and gamepad.
            input.Update();
            Root.UpdateFocus(IsActive);
            if (IsActive)
                Root.HandleInput(false, input);
            Root.Update(gameTime);
        }


        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise);
            Root.PositionUpdate();
            Root.DrawWithChildren(gameTime, SpriteBatch, 1.0f);
            SpriteBatch.End();
        }


        #endregion

        #region Public Methods
        private List<GameScreen> addScreens = new List<GameScreen>();
        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(GameScreen screen)
        {
            addScreens.Add(screen);
        }

        private List<GameScreen> removeScreens = new List<GameScreen>();
        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen screen)
        {
            removeScreens.Add(screen);
        }

        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToBlack(float alpha)
        {
            Viewport viewport = GraphicsDevice.Viewport;


            SpriteBatch.Draw(ContentHelper.Blank,
                             new Rectangle(0, 0, viewport.Width, viewport.Height),
                             Color.Black * alpha);

        }


        #endregion

        #region Helper Methods
        /// <summary>
        /// Helps take keyboard input for a text box or something.
        /// Should be called in HandleInput
        /// </summary>
        /// <param name="currentString">The string being modified</param>
        /// <param name="input">Input from the HandleInput call</param>
        /// <returns>Modified string</returns>
        public static void TakeKeyboardInput(ref int position, ref string currentString, InputState input)
        {
            Keys[] pressedKeys = input.KeyboardState.GetPressedKeys();
            foreach (Keys key in pressedKeys)
            {

                if (input.IsNewKeyPress(key))
                {

                    if (key == Keys.Back && position > 0)
                    {
                        position--;
                        currentString = currentString = currentString.Remove(position, 1);
                    }
                    char typedChar = GetChar(key, input.IsKeyDown(Keys.LeftShift) || input.IsKeyDown(Keys.RightShift));
                    if (typedChar != (char)0)
                    {
                        currentString = currentString.Insert(position, "" + typedChar);
                        position++;
                    }
                }
            }
        }
        private static char GetChar(Keys key, bool shiftHeld)
        {
            if (key == Keys.Space) return ' ';
            if (key >= Keys.A && key <= Keys.Z)
            {
                if (shiftHeld)
                {
                    return key.ToString()[0];
                }
                else
                {
                    return key.ToString().ToLower()[0];
                }
            }
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                if (shiftHeld)
                {
                    if (key == Keys.D2) return '@';
                    else if (key == Keys.D0) return ')';
                    else if (key == Keys.D6) return '^';
                    else if (key == Keys.D8) return '*';
                    else
                    {
                        if (key > Keys.D5) { return (char)(key - Keys.D0 + 31); }
                        else return (char)(key - Keys.D0 + 32);
                    }
                }
                else return (key - Keys.D0).ToString()[0];
            }
            if (key >= Keys.NumPad0 && key <= Keys.NumPad9) return (key - Keys.NumPad0).ToString()[0];
            if (key == Keys.OemPeriod || key == Keys.Decimal) return '.';
            if (key == Keys.OemQuestion) return '/';
            return (char)0;
        }
        #endregion
    }
}
