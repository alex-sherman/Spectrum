using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Screens
{
    public class RootElement : Element
    {
        public RenderTarget2D Target { get; set; }
        public int PixelWidth { get { return Target?.Width ?? SpectrumGame.Game.GraphicsDevice.Viewport.Width; } }
        public int PixelHeight { get { return Target?.Height ?? SpectrumGame.Game.GraphicsDevice.Viewport.Height; } }
        public SpriteBatch SpriteBatch;
        public override bool HasFocus => SpectrumGame.Game.IsActive;
        public RootElement()
        {
            Width = 1.0;
            Height = 1.0;
            SpriteBatch = new SpriteBatch(SpectrumGame.Game.GraphicsDevice);
            Initialize();
        }

        public void Update(float dt, InputState input)
        {
            // Needs to happen first because DrawEnabled will be updated in these handlers
            if (HasFocus)
                HandleInput(false, input);
            ClearMeasure();
            Measure(0, 0);
            Measure(PixelWidth, PixelHeight);
            Layout(new Rectangle(0, 0, PixelWidth, PixelHeight));
            base.Update(dt);
        }

        public void Draw(float gameTime)
        {
            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise);
            var children = RecursiveChildren.ToList();
            children.Reverse();

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child.Display)
                    child.Draw(gameTime, SpriteBatch, i * 1.0f / children.Count);
            }
            SpectrumGame.Game.GraphicsDevice.SetRenderTarget(Target);
            if (Target != null)
                SpectrumGame.Game.GraphicsDevice.Clear(Color.Transparent);
            SpriteBatch.End();
        }

    }
}
