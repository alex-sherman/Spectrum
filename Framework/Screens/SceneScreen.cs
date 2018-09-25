using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Content;
using Spectrum.Framework.Physics;
using Spectrum.Framework.Input;
using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.VR;

namespace Spectrum.Framework.Screens
{
    public class SceneScreen : Element
    {
        public EntityManager Manager = SpectrumGame.Game.EntityManager;
        public RenderTarget2D RenderTarget;
        public Camera Camera;
        private bool _captureMouse = false;
        public bool CaptureMouse
        {
            get => _captureMouse;
            set
            {
                if (_captureMouse != value)
                {
                    _captureMouse = value;
                    if (!value)
                        SpectrumGame.Game.ShowMouse();
                    else
                        SpectrumGame.Game.HideMouse();
                }
            }
        }
        public static Matrix Projection;
        public static bool Dirty;
        public SceneScreen()
        {
            ZDetach = true;
            Z = 100;
            Width = ElementSize.WrapFill;
            Height = ElementSize.WrapFill;
        }
        public override void Layout(Rectangle bounds)
        {
            base.Layout(bounds);
            if ((Dirty || RenderTarget == null || bounds.Width != RenderTarget.Width || bounds.Height != RenderTarget.Height)
                && (bounds.Width > 0 && bounds.Height > 0))
            {
                Dirty = false;
                Camera.Projection = Settings.GetProjection(bounds.Width, bounds.Height);
                RenderTarget?.Dispose();
                RenderTarget = new RenderTarget2D(SpectrumGame.Game.GraphicsDevice, bounds.Width, bounds.Height,
                    false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                GraphicsEngine.ResetOnResize(bounds.Width, bounds.Height);
            }
        }
        public override void Draw(float gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            if (Camera != null && RenderTarget != null)
            {
                GraphicsEngine.Camera = Camera;
                using (DebugTiming.Main.Time("Draw"))
                {
                    var renderGroups = Manager.Batch.GetRenderTasks(gameTime);
                    if (SpecVR.Running)
                        GraphicsEngine.RenderVRScene(Camera, renderGroups, RenderTarget);
                    else
                        GraphicsEngine.RenderScene(Camera, renderGroups, RenderTarget);
                    spriteBatch.Draw(RenderTarget, Rect, Color.White, LayerDepth);
                    Manager.Batch.ClearRenderTasks();
                }
            }
        }

        public override void Update(float dt)
        {
            using (DebugTiming.Main.Time("Update"))
            {
                //TODO: Fix multiplayer update
                //using (DebugTiming.Main.Time("MPCallback"))
                //    SpectrumGame.Game.MP.MakeCallbacks(gameTime);
                Manager.Update(dt);
                base.Update(dt);
            }
        }

        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            bool output = base.HandleInput(otherTookInput, input);
            if (CaptureMouse)
            {
                Mouse.SetPosition(SpectrumGame.Game.GraphicsDevice.Viewport.Width / 2,
                              SpectrumGame.Game.GraphicsDevice.Viewport.Height / 2);
            }
            return output;
        }
    }
}
