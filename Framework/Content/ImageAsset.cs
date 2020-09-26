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
        public static readonly ImageAsset Missing;
        static ImageAsset()
        {
            Blank = new ImageAsset(ContentHelper.Blank);
            Missing = new ImageAsset(ContentHelper.Missing);
        }
        public SvgDocument SVG = null;
        public Texture2D Texture = null;
        private Texture2D rasterized = null;
        // Marking as public requires a reference to SVG to resolve method
        public ImageAsset(SvgDocument svg) { SVG = svg; }
        public ImageAsset(Texture2D texture) { Texture = texture; }
        public static implicit operator ImageAsset(Texture2D texture) => new ImageAsset(texture);
        public static implicit operator ImageAsset(string path)
        {
            return ContentHelper.Load<ImageAsset>(path) ?? Missing;
        }
        public ImageAsset() { }
        public void Rasterize(int width, int height)
        {
            System.Drawing.Bitmap bitmap = SVG.Draw(width, height);
            rasterized?.Dispose();
            rasterized = bitmap.GetTexture2DFromBitmap(SpectrumGame.Game.GraphicsDevice);
        }
        public Texture2D GetTexture(Rectangle rect)
        {
            if (SVG != null)
            {
                if (rasterized == null || rasterized.Bounds.Width != rect.Width || rasterized.Bounds.Height != rect.Height)
                {
                    Rasterize(rect.Width, rect.Height);
                }
                return rasterized;
            }
            else if (Texture != null)
            {
                return Texture;
            }
            return null;
        }
        public ImageAsset Clone()
        {
            if (SVG != null)
                return new ImageAsset(SVG);
            return new ImageAsset(Texture);
        }
    }
}
