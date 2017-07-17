using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Spectrum.Framework.Graphics;
using System.Windows.Forms;
using Spectrum.Framework;
using Spectrum.Framework.Network;
using Spectrum.Framework.Physics;
using Spectrum.Framework.Screens;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Content;
using Spectrum.Framework.Audio;
using Spectrum.Framework.Input;
using Valve.VR;
using Spectrum.Framework.VR;

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
        public bool DebugDrawAll = false;

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

        public Dictionary<string, Plugin> Plugins = new Dictionary<string, Plugin>();
        GraphicsDeviceManager graphics;
        public EntityManager EntityManager { get; set; }
        public MultiplayerService MP { get; set; }
        public RootElement Root { get; private set; }
        bool newResize = false;
        private Point mousePosition;
        public bool UsingSteam { get; private set; }

        public SpectrumGame(Guid? guid = null, string nick = "Player")
        {
            NetID ID;
            UsingSteam = guid == null;
            if (UsingSteam)
            {
                if (!Steamworks.SteamAPI.Init())
                    throw new Exception("Steam init failed!");
                ID = new NetID(Steamworks.SteamUser.GetSteamID().m_SteamID);
            }
            else
            {
                ID = new NetID(guid.Value);
            }
            Game = this;
            InputLayout.Init();
            MP = new MultiplayerService(ID, nick);
            EntityManager = new EntityManager(MP);
            PhysicsEngine.Init(EntityManager);
            graphics = new GraphicsDeviceManager(this);
            AudioManager.Init();
            this.Window.AllowUserResizing = true;
            WindowForm = (Form)Form.FromHandle(Window.Handle);
            IsFixedTimeStep = false;
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
        public bool FullScreen
        {
            get { return Window.IsBorderless; }
            set
            {
                bool lastBorderless = Window.IsBorderless;
                WindowForm.WindowState = FormWindowState.Normal;
                Window.IsBorderless = value;
                if (value)
                {
                    var bounds = Screen.FromControl(WindowForm).Bounds;
                    WindowForm.DesktopLocation = new System.Drawing.Point(bounds.X, bounds.Y);
                    WindowForm.ClientSize = new System.Drawing.Size(bounds.Width, bounds.Height);
                }
                else
                {
                    if (lastBorderless)
                        WindowForm.WindowState = FormWindowState.Maximized;
                }
                newResize = true;
            }
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            string path = "save.dat";
            if (File.Exists(path))
            {
                LoadSettings(File.OpenRead(path));
            }
            else { SaveSettings(File.Create(path)); }
            this.IsMouseVisible = true;
            Content.RootDirectory = "Content";
            Root = new RootElement();
            Serialization.InitSurrogates();
            LoadHelper.LoadTypes();
            Serialization.Model.CompileInPlace();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            AudioManager.Shutdown();
            SaveSettings(File.OpenWrite("save.dat"));
            MP.Dispose();
            if(UsingSteam)
            {
                Steamworks.SteamAPI.Shutdown();
            }
        }
        InputState input = new InputState();
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            DebugTiming.StartFrame();
            if (UsingSteam)
            {
                Steamworks.SteamAPI.RunCallbacks();
                Steamworks.SteamUtils.RunFrame();
            }
            if (graphics.PreferredBackBufferHeight != WindowForm.ClientRectangle.Height || graphics.PreferredBackBufferWidth != WindowForm.ClientRectangle.Width)
            {
                graphics.PreferredBackBufferHeight = WindowForm.ClientRectangle.Height;
                graphics.PreferredBackBufferWidth = WindowForm.ClientRectangle.Width;
                graphics.ApplyChanges();
                Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                if (OnScreenResize != null && graphics.GraphicsDevice.Viewport.Height > 0 && graphics.GraphicsDevice.Viewport.Width > 0)
                {
                    OnScreenResize(this, EventArgs.Empty);
                }
                newResize = false;
            }
            if(SpecVR.Running)
            {
                SpecVR.Update(gameTime);
            }
            input.Update();
            Root.Update(gameTime, input, IsActive);
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Root.Draw(gameTime);
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
