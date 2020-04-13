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
        public override bool HasFocus => SpectrumGame.Game.IsActive && SpectrumGame.Game.WindowForm.Focused;
        private List<Element> orderedChildren;
        public RootElement()
        {
            Font = DefaultFont;
            Width = 1.0;
            Height = 1.0;
            SpriteBatch = new SpriteBatch(SpectrumGame.Game.GraphicsDevice);
            Initialize();
        }

        public void Update(float dt, InputState input)
        {
            List<Element> roots = new List<Element>() { this };
            List<List<Element>> trees = new List<List<Element>>() { };
            while (roots.Any())
            {
                var current = roots.Pop();
                trees.Add(OrderChildren(current, roots).ToList());
            }
            orderedChildren = trees.OrderBy(t => t[0].Z).SelectMany(t => t).ToList();
            // Needs to happen first because DrawEnabled will be updated in these handlers
            if (HasFocus)
            {
                bool otherTookInput = false;

                for (int i = orderedChildren.Count - 1; i >= 0; i--)
                {
                    otherTookInput |= orderedChildren[i].HandleInput(otherTookInput, input);
                }
            }
            ClearMeasure();
            Measure(0, 0);
            Measure(PixelWidth, PixelHeight);
            Layout(new Rectangle(0, 0, PixelWidth, PixelHeight));
            base.Update(dt);
        }

        private IEnumerable<Element> OrderChildren(Element element, List<Element> roots)
        {
            // TODO: Do a where on children for detached elements
            roots.AddRange(element.Children.Where(c => c.ZDetach));
            return Enumerable.Empty<Element>().Union(element).Union(element.Children.Where(c => !c.ZDetach).OrderBy(c => c.Z).SelectMany(c => OrderChildren(c, roots)));
        }
        public void Draw(float gameTime)
        {
            //SpriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise);

            for (int i = 0; i < orderedChildren.Count; i++)
            {
                var child = orderedChildren[orderedChildren.Count - i - 1];
                // Some children may have been removed during update
                if (child.Parent != null && child.Display)
                {
                    // Reserve 0
                    child.LayerDepth = (i + 1.0f) / (orderedChildren.Count + 1);
                    child.Draw(gameTime, SpriteBatch);
                }
            }
            SpectrumGame.Game.GraphicsDevice.SetRenderTarget(Target);
            if (Target != null)
                SpectrumGame.Game.GraphicsDevice.Clear(Color.Transparent);
            SpriteBatch.End();
        }

    }
}
