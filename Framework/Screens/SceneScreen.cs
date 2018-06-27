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

namespace Spectrum.Framework.Screens
{
    public class SceneScreen : GameScreen
    {
        public EntityManager Manager = SpectrumGame.Game.EntityManager;
        public RenderTarget2D RenderTarget;
        public Camera Camera;
        public bool CaptureMouse { get; set; } = true;
        public override bool HasFocus
        {
            get
            {
                if (!base.HasFocus || Parent.Children.IndexOf(this) != 0)
                    return false;
                foreach (Element child in Children)
                {
                    if (child is GameScreen && (child as GameScreen).Display == ElementDisplay.Visible)
                        return false;
                }
                return true;
            }
        }
        [ThreadStatic]
        public static Matrix Projection;
        public static bool Dirty { get; set; }
        public SceneScreen()
        {
            Width.Type = SizeType.MatchParent;
            Height.Type = SizeType.MatchParent;
        }
        public override void Layout(Rectangle bounds)
        {
            base.Layout(bounds);
            if ((Dirty || RenderTarget == null || bounds.Width != RenderTarget.Width || bounds.Height != RenderTarget.Height)
                && (bounds.Width > 0 && bounds.Height > 0))
            {
                Dirty = false;
                Projection = Settings.GetProjection(bounds.Width, bounds.Height);
                RenderTarget?.Dispose();
                RenderTarget = new RenderTarget2D(SpectrumGame.Game.GraphicsDevice, bounds.Width, bounds.Height,
                    false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                GraphicsEngine.ResetOnResize(bounds.Width, bounds.Height);
            }
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            var timer = DebugTiming.Main.Time("Draw");
            if (Camera != null)
            {
                GraphicsEngine.Camera = Camera;
                GraphicsEngine.Render(Manager.Entities.DrawSorted, gameTime, RenderTarget);
                spriteBatch.Draw(RenderTarget, Rect, Color.White, Z);
            }
            timer.Stop();
        }

        public override void Update(GameTime gameTime)
        {
            var timer = DebugTiming.Main.Time("Update");
            SpectrumGame.Game.MP.MakeCallbacks(gameTime);
            PhysicsEngine.Single.Update(gameTime);
            Manager.Update(gameTime);
            base.Update(gameTime);
            timer.Stop();
        }

        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            bool output = base.HandleInput(otherTookInput, input);
            if (CaptureMouse)
            {
                if (HasFocus != !SpectrumGame.Game.IsMouseVisible)
                {
                    if (HasFocus)
                        SpectrumGame.Game.HideMouse();
                    else
                        SpectrumGame.Game.ShowMouse();
                }
                if (HasFocus)
                    Mouse.SetPosition(SpectrumGame.Game.GraphicsDevice.Viewport.Width / 2,
                                  SpectrumGame.Game.GraphicsDevice.Viewport.Height / 2);
            }
            return output;
        }
    }
}
