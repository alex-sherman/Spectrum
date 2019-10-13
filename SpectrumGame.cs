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
using Steamworks;
using System.Threading.Tasks;

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
        #endregion

        public Dictionary<string, Plugin> Plugins = new Dictionary<string, Plugin>();
        public EntityManager EntityManager { get; set; }
        public MultiplayerService MP { get; set; }
        public RootElement Root { get; private set; }
        private Point mousePosition;
        protected uint SteamAppID = 0;
        public static readonly bool UsingSteam =
#if STEAM
            true;
#else
            false;
#endif

        public SpectrumGame(Guid? guid = null, string nick = "Player")
        {
            NetID ID;
#if STEAM
            if (!SteamAPI.Init())
                throw new Exception("Steam init failed!");
            ID = new NetID(SteamUser.GetSteamID().m_SteamID);
#else
            ID = new NetID(guid ?? Guid.NewGuid());
#endif
            Game = this;
            Graphics = new GraphicsDeviceManager(this);
            MP = new MultiplayerService(ID, nick);
            WindowForm = (Form)Form.FromHandle(Window.Handle);
            IsFixedTimeStep = false;
        }

        private void LoadSettings(FileStream stream)
        {
            byte[] buffer = new byte[32];
            stream.Read(buffer, 0, 4);
            Graphics.PreferredBackBufferWidth = BitConverter.ToInt32(buffer, 0);
            stream.Read(buffer, 0, 4);
            Graphics.PreferredBackBufferHeight = BitConverter.ToInt32(buffer, 0);
            WindowForm.ClientSize = new System.Drawing.Size(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);
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
                Graphics.IsFullScreen = true;
            }

            Graphics.ApplyChanges();
            stream.Close();
        }
        private void SaveSettings(FileStream stream)
        {
            byte[] buffer;
            buffer = BitConverter.GetBytes(Graphics.PreferredBackBufferWidth);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(Graphics.PreferredBackBufferHeight);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(WindowLocation.X);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(WindowLocation.Y);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(WindowMaximized);
            stream.Write(buffer, 0, 1);
            buffer = BitConverter.GetBytes(Graphics.IsFullScreen);
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
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            InputLayout.Init();
            EntityManager = new EntityManager();
            AudioManager.Init();
            Window.AllowUserResizing = true;
            if (UsingSteam)
                SteamAPI.RestartAppIfNecessary((AppId_t)SteamAppID);
        }
        protected override void LoadContent()
        {
            base.LoadContent();
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            string path = "save.dat";
            if (!File.Exists(path))
            {
                WindowForm.WindowState = FormWindowState.Maximized;
                SaveSettings(File.Create(path));
            }
            LoadSettings(File.OpenRead(path));
            IsMouseVisible = true;
            Root = new RootElement();
            Serialization.InitSurrogates();
            LoadHelper.LoadTypes();
            Serialization.Model.CompileInPlace();
            GraphicsEngine.Initialize();
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
            if (UsingSteam)
            {
                SteamAPI.Shutdown();
            }
        }
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
                SteamAPI.RunCallbacks();
                SteamUtils.RunFrame();
            }
            if (Graphics.PreferredBackBufferHeight != WindowForm.ClientRectangle.Height || Graphics.PreferredBackBufferWidth != WindowForm.ClientRectangle.Width)
            {
                Graphics.PreferredBackBufferHeight = WindowForm.ClientRectangle.Height;
                Graphics.PreferredBackBufferWidth = WindowForm.ClientRectangle.Width;
                Graphics.ApplyChanges();
                if (OnScreenResize != null && Graphics.GraphicsDevice.Viewport.Height > 0 && Graphics.GraphicsDevice.Viewport.Width > 0)
                {
                    OnScreenResize(this, EventArgs.Empty);
                }
            }
            if (SpecVR.Running)
                SpecVR.PollEvents();
            SpecTime.Update(gameTime.DT());
            InputState.Current.Update(gameTime.DT());
            Root.Update(gameTime.DT(), InputState.Current);
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Root.Draw(gameTime.DT());
        }
        protected override void EndDraw()
        {
            using (DebugTiming.Main.Time("GPU Draw"))
            {
                base.EndDraw();
                GraphicsEngine.EndDraw();
                if (SpecVR.Running)
                    SpecVR.Update();
            }
        }

        public GraphicsDeviceManager Graphics { get; private set; }

        public void ShowMouse(bool resetPosition = true)
        {
            IsMouseVisible = true;
            if (resetPosition)
                Mouse.SetPosition(mousePosition.X, mousePosition.Y);
        }
        public void HideMouse()
        {
            mousePosition = Mouse.GetState().Position;
            IsMouseVisible = false;
        }
    }
}
