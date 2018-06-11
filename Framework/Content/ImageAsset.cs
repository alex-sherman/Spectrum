using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content
{
    public class ImageAsset
    {
        private SvgDocument svg = null;
        private Texture2D texture = null;
        private Texture2D rasterized = null;
        public ImageAsset(SvgDocument svg) { this.svg = svg; }
        public ImageAsset(Texture2D texture) { this.texture = texture; }
        private ImageAsset() { }
        public void Rasterize(int width, int height)
        {
            System.Drawing.Bitmap bitmap = svg.Draw(width, height);
            rasterized = new Texture2D(SpectrumGame.Game.GraphicsDevice, bitmap.Width, bitmap.Height);
            Color[] data = new Color[bitmap.Width * bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    System.Drawing.Color color = bitmap.GetPixel(i, j);
                    data[i + j * bitmap.Width] = new Color(color.R, color.G, color.B, color.A);
                }
            }
            rasterized.SetData(data);
        }
        public void Draw(SpriteBatch spriteBatch, Rectangle rect, Color color, float layer)
        {
            if (svg != null) {
                if (rasterized == null || rasterized.Bounds.Width != rect.Width || rasterized.Bounds.Height != rect.Height)
                {
                    Rasterize(rect.Width, rect.Height);
                }
                spriteBatch.Draw(rasterized, rect, color, layer);
            }
            else if(texture != null)
            {
                spriteBatch.Draw(texture, rect, color, layer);
            }
        }
        public ImageAsset Clone()
        {
            if (svg != null)
                return new ImageAsset(svg);
            return new ImageAsset(texture);
        }
    }
}
