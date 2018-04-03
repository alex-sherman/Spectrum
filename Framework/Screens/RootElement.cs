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
        public RootElement()
        {
            Width.Type = SizeType.MatchParent;
            Height.Type = SizeType.MatchParent;
            SpriteBatch = new SpriteBatch(SpectrumGame.Game.GraphicsDevice);
            Initialize();
        }
    
        public void Update(GameTime gameTime, InputState input, bool hasFocus)
        {
            Measure(PixelWidth, PixelHeight);
            Layout(new Rectangle(0, 0, PixelWidth, PixelHeight));
            base.Update(gameTime);
            UpdateFocus(hasFocus);
            if (hasFocus)
                HandleInput(false, input);
        }
        
        public void Draw(GameTime gameTime)
        {
            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise);
            DrawWithChildren(gameTime, SpriteBatch, 1.0f);
            SpectrumGame.Game.GraphicsDevice.SetRenderTarget(Target);
            if (Target != null)
                SpectrumGame.Game.GraphicsDevice.Clear(Color.Transparent);
            SpriteBatch.End();
        }
    
    }
}
