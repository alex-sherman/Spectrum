﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using Spectrum.Framework.Graphics;
using System.Windows.Forms;
using Spectrum.Framework;
using Spectrum.Framework.Network;
using Spectrum.Framework.Physics;
using Spectrum.Framework.Screens;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Content;

namespace Spectrum
{
    public class SpectrumGame : Game
    {
        private static System.Drawing.Point PointToPoint(Point point)
        {
            return new System.Drawing.Point(point.X, point.Y);
        }
        private static Point PointToPoint(System.Drawing.Point point)
        {
            return new Point(point.X, point.Y);
        }
        public static SpectrumGame Game { get; private set; }

        public bool Debug = false;
        public bool DebugDraw = false;

        #region Window Properties
        public event EventHandler OnScreenResize;
        public Form WindowForm { get; private set; }
        public bool WindowMaximized
        {
            get { return WindowForm.WindowState == FormWindowState.Maximized; }
            set
            {
                if (value)
                {
                    WindowForm.WindowState = FormWindowState.Maximized;
                }
                else
                {
                    WindowForm.WindowState = FormWindowState.Normal;
                }
            }
        }
        public Point WindowLocation
        {
            get { return PointToPoint(WindowForm.Location); }
            set { WindowForm.Location = PointToPoint(value); }
        }
        public int ResolutionWidth
        {
            get { return graphics.PreferredBackBufferWidth; }
            set { graphics.PreferredBackBufferWidth = value; }
        }
        public int ResolutionHeight
        {
            get { return graphics.PreferredBackBufferHeight; }
            set { graphics.PreferredBackBufferHeight = value; }
        }
        #endregion

        public RealDict<string, Plugin> Plugins = new RealDict<string, Plugin>();
        GraphicsDeviceManager graphics;
        public ScreenManager ScreenManager { get; private set; }
        bool newResize = false;
        private Point mousePosition;
        public Guid ID { get; private set; }

        public SpectrumGame(Guid id)
        {
            ID = id;
            Game = this;
            graphics = new GraphicsDeviceManager(this);
            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += WindowSizeChange;
            WindowForm = (Form)Form.FromHandle(Window.Handle);
            IsFixedTimeStep = false;
            string path = "save.dat";
            if (File.Exists(path))
            {
                LoadSettings(File.OpenRead(path));
            }
            else { SaveSettings(File.Create(path)); }
            this.IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        private void LoadSettings(FileStream stream)
        {
            byte[] buffer = new byte[32];
            stream.Read(buffer, 0, 4);
            graphics.PreferredBackBufferWidth = BitConverter.ToInt32(buffer, 0);
            stream.Read(buffer, 0, 4);
            graphics.PreferredBackBufferHeight = BitConverter.ToInt32(buffer, 0);

            stream.Read(buffer, 0, 4);
            Point newP;
            newP.X = BitConverter.ToInt32(buffer, 0);
            stream.Read(buffer, 0, 4);
            newP.Y = BitConverter.ToInt32(buffer, 0);
            WindowLocation = newP;

            stream.Read(buffer, 0, 1);
            WindowMaximized = BitConverter.ToBoolean(buffer, 0);

            stream.Read(buffer, 0, 1);
            if (BitConverter.ToBoolean(buffer, 0))
            {
                graphics.IsFullScreen = true;
            }

            graphics.ApplyChanges();
            stream.Close();
        }
        private void SaveSettings(FileStream stream)
        {
            byte[] buffer;
            buffer = BitConverter.GetBytes(graphics.PreferredBackBufferWidth);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(graphics.PreferredBackBufferHeight);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(WindowLocation.X);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(WindowLocation.Y);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(WindowMaximized);
            stream.Write(buffer, 0, 1);
            buffer = BitConverter.GetBytes(graphics.IsFullScreen);
            stream.Write(buffer, 0, 1);
            stream.Close();
        }
        public void SetResolution(Tuple<int, int> res, bool borderless = false)
        {
            if (!graphics.IsFullScreen)
            {
                WindowForm.WindowState = FormWindowState.Normal;
                Window.IsBorderless = borderless;
                GraphicsDeviceManager.PreferredBackBufferWidth = res.Item1;
                GraphicsDeviceManager.PreferredBackBufferHeight = res.Item2;
            }
            newResize = true;
        }
        private void WindowSizeChange(object sender, EventArgs e)
        {
            newResize = true;
            graphics.PreferredBackBufferHeight = WindowForm.ClientRectangle.Height;
            graphics.PreferredBackBufferWidth = WindowForm.ClientRectangle.Width;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            ScreenManager = new ScreenManager(this, ContentHelper.Single);
            Components.Add(ScreenManager);
            ScreenManager.Initialize();
            LoadHelper.LoadTypes();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            SaveSettings(File.OpenWrite("save.dat"));
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (newResize)
            {
                graphics.ApplyChanges();
                if (OnScreenResize != null)
                {
                    OnScreenResize(this, EventArgs.Empty);
                }
                newResize = false;
            }
            base.Update(gameTime);
        }
        public GraphicsDeviceManager GraphicsDeviceManager
        {
            get { return graphics; }
        }
        public void ShowMouse()
        {
            IsMouseVisible = true;
            Mouse.SetPosition(mousePosition.X, mousePosition.Y);
        }
        public void HideMouse()
        {
            mousePosition = Mouse.GetState().Position;
            IsMouseVisible = false;
            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
        }
    }
}
