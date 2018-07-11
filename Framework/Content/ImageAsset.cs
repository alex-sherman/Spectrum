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
        public static readonly ImageAsset Blank;
        static ImageAsset() => Blank = new ImageAsset(ContentHelper.Blank);
        public SvgDocument svg = null;
        private Texture2D texture = null;
        private Texture2D rasterized = null;
        // Marking as public requires a reference to SVG to resolve method
        internal ImageAsset(SvgDocument svg) { this.svg = svg; }
        public ImageAsset(Texture2D texture) { this.texture = texture; }
        public ImageAsset() { }
        public void Rasterize(int width, int height)
        {
            System.Drawing.Bitmap bitmap = svg.Draw(width, height);
            rasterized = bitmap.GetTexture2DFromBitmap(SpectrumGame.Game.GraphicsDevice);
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
