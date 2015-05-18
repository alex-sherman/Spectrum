using System;
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
using Spectrum.Framework.Network.Directory;
using Spectrum.Framework.Physics;
using Spectrum.Framework.Screens;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Content;

namespace Spectrum
{
    public class SpectrumGame : Game
    {

        public bool Debug = false;

        public bool DebugDraw = false;

        public bool UseAuthSave = true;

        public bool Offline = false;

        public event EventHandler OnScreenResize;
        public static SpectrumGame Game { get; private set; }
        public RealDict<string, Plugin> Plugins = new RealDict<string, Plugin>();
        GraphicsDeviceManager graphics;
        public ScreenManager ScreenManager { get; private set; }
        int adapterNum = 0;
        int windowedWidth;
        int windowedHeight;
        bool newResize = false;
        bool resetLocation = true;
        bool maximized = false;
        bool setMaximized = false;
        System.Drawing.Point newP = new System.Drawing.Point();
        System.Drawing.Point p = new System.Drawing.Point();
        private Point mousePosition;
        public Guid ID { get; private set; }
        public MultiplayerService MP { get; private set; }
        public EntityManager EntityManager { get; private set; }
        public DirectoryHelper AuthManager;

        public SpectrumGame()
        {
            if (!Offline)
                AuthManager = new DirectoryHelper();
            ID = Guid.NewGuid();
            Game = this;
            graphics = new GraphicsDeviceManager(this);

            EntityCollection ECollection = new EntityCollection();
            PhysicsEngine.Init(ECollection);
            MP = new MultiplayerService(ID);
            NetworkMutex.Init(MP);
            DebugPrinter.display(MP);
            EntityManager = new EntityManager(ECollection, MP);
            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += WindowSizeChange;
            ((Form)Form.FromHandle(Window.Handle)).LocationChanged += WindowLocationChange;
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
            adapterNum = stream.ReadByte();
            byte[] buffer = new byte[32];
            stream.Read(buffer, 0, 4);
            graphics.PreferredBackBufferWidth = BitConverter.ToInt32(buffer, 0);
            windowedWidth = graphics.PreferredBackBufferWidth;
            stream.Read(buffer, 0, 4);
            graphics.PreferredBackBufferHeight = BitConverter.ToInt32(buffer, 0);
            windowedHeight = graphics.PreferredBackBufferHeight;

            stream.Read(buffer, 0, 1);
            if (BitConverter.ToBoolean(buffer, 0))
            {
                graphics.IsFullScreen = true;
            }

            stream.Read(buffer, 0, 1);
            setMaximized = BitConverter.ToBoolean(buffer, 0);

            stream.Read(buffer, 0, 4);
            newP.X = BitConverter.ToInt32(buffer, 0);
            stream.Read(buffer, 0, 4);
            newP.Y = BitConverter.ToInt32(buffer, 0);

            graphics.ApplyChanges();
            stream.Close();
        }
        private void SaveSettings(FileStream stream)
        {
            byte[] buffer;
            stream.WriteByte((byte)adapterNum);
            buffer = BitConverter.GetBytes(graphics.PreferredBackBufferWidth);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(graphics.PreferredBackBufferHeight);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(graphics.IsFullScreen);
            stream.Write(buffer, 0, 1);
            buffer = BitConverter.GetBytes(maximized);
            stream.Write(buffer, 0, 1);
            buffer = BitConverter.GetBytes(p.X);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(p.Y);
            stream.Write(buffer, 0, 4);
            stream.Close();
        }
        public void SetResolution(Tuple<int, int> res)
        {
            if (!graphics.IsFullScreen)
            {
                windowedHeight = res.Item2;
                windowedWidth = res.Item1;
                this.GraphicsDeviceManager.PreferredBackBufferWidth = res.Item1;
                this.GraphicsDeviceManager.PreferredBackBufferHeight = res.Item2;
                this.GraphicsDeviceManager.ApplyChanges();
            }
            newResize = true;
        }
        public void ToggleFullScreen()
        {
            if (!graphics.IsFullScreen)
            {
                graphics.PreferredBackBufferWidth = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
                graphics.IsFullScreen = true;
            }
            else
            {
                graphics.PreferredBackBufferWidth = windowedWidth;
                graphics.PreferredBackBufferHeight = windowedHeight;
                graphics.IsFullScreen = false;
            }
            graphics.ApplyChanges();
        }
        private void WindowSizeChange(object sender, EventArgs e)
        {
            newResize = true;
        }
        private void WindowLocationChange(object sender, EventArgs e)
        {
            Form f = sender as Form;
            p = f.Location;
            maximized = f.WindowState == FormWindowState.Maximized;
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
            Form form = (Form)Form.FromHandle(Window.Handle);
            if (resetLocation)
            {
                form.Location = newP;
                resetLocation = false;
            }
            if (setMaximized)
            {
                form.WindowState = FormWindowState.Maximized;
                maximized = true;
                setMaximized = false;
            }
            if (newResize)
            {
                if (form.ClientRectangle.Height > 0 && form.ClientRectangle.Width > 0)
                {
                    //Setting the resolution updates the prefered back buffer for saving the settings back to disk
                    SetResolution(new Tuple<int, int>(form.ClientRectangle.Width, form.ClientRectangle.Height));
                    if(OnScreenResize != null)
                    {
                        OnScreenResize(this, EventArgs.Empty);
                    }
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
