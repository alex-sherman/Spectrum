﻿using System;
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

namespace Spectrum.Framework.Screens
{
    public class SceneScreen : GameScreen
    {
        public EntityManager Manager = SpectrumGame.Game.EntityManager;
        public RenderTarget2D RenderTarget;
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
            if((Dirty || RenderTarget == null || bounds.Width != RenderTarget.Width || bounds.Height != RenderTarget.Height)
                && (bounds.Width > 0 && bounds.Height > 0))
            {
                Dirty = false;
                Projection = Settings.GetProjection(bounds.Width, bounds.Height);
                RenderTarget?.Dispose();
                RenderTarget = new RenderTarget2D(SpectrumGame.Game.GraphicsDevice, (int)(bounds.Width), (int)(bounds.Height),
                    false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                GraphicsEngine.ResetOnResize(bounds.Width, bounds.Height);
            }
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            var timer = DebugTiming.Main.Time("Draw");
            GraphicsEngine.Render(Manager.Entities.DrawSorted, gameTime, RenderTarget);
            spriteBatch.Draw(RenderTarget, Rect, Color.White, Z);
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
    }
}