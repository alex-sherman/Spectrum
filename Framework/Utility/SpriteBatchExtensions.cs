using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public static class SpriteBatchExtensions
    {
        /// <summary>
        /// Solution from: https://stackoverflow.com/questions/2869801/is-there-a-fast-alternative-to-creating-a-texture2d-from-a-bitmap-object-in-xna
        /// </summary>
        public static Texture2D GetTexture2DFromBitmap(this System.Drawing.Bitmap bitmap, GraphicsDevice device)
        {
            Texture2D tex = new Texture2D(device, bitmap.Width, bitmap.Height, false, SurfaceFormat.Bgra32);
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int bufferSize = data.Height * data.Stride;
            byte[] bytes = new byte[bufferSize];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            tex.SetData(bytes);
            bitmap.UnlockBits(data);
            return tex;
        }
        public static void DrawString(this SpriteBatch spritebatch, SpriteFont spriteFont, string text, Vector2 position, Color color, float layerDepth, Rectangle? clip = null)
        {
            //spritebatch.DrawString(font, text, pos, textColor, 0, Vector2.Zero, 1, SpriteEffects.None, layer);
            //CheckValid(spriteFont, text);
            var scale = Vector2.One;
            float sortKey = 0;
            // set SortKey based on SpriteSortMode.
            switch (spritebatch.SortMode)
            {
                // Comparison of Texture objects.
                case SpriteSortMode.Texture:
                    sortKey = 0;
                    break;
                // Comparison of Depth
                case SpriteSortMode.FrontToBack:
                    sortKey = layerDepth;
                    break;
                // Comparison of Depth in reverse
                case SpriteSortMode.BackToFront:
                    sortKey = -layerDepth;
                    break;
            }

            var flipAdjustment = Vector2.Zero;

            //var flippedVert = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            //var flippedHorz = (effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

            //if (flippedVert || flippedHorz)
            //{
            //    Vector2 size;

            //    var source = new SpriteFont.CharacterSource(text);
            //    spriteFont.MeasureString(ref source, out size);

            //    if (flippedHorz)
            //    {
            //        origin.X *= -1;
            //        flipAdjustment.X = -size.X;
            //    }

            //    if (flippedVert)
            //    {
            //        origin.Y *= -1;
            //        flipAdjustment.Y = spriteFont.LineSpacing - size.Y;
            //    }
            //}

            Matrix transformation = Matrix.CreateScale(scale.X, scale.Y, 1) * Matrix.CreateTranslation(position.X, position.Y, 0);
            //float cos = 0, sin = 0;
            //if (rotation == 0)
            //{
            //    transformation.M11 = (flippedHorz ? -scale.X : scale.X);
            //    transformation.M22 = (flippedVert ? -scale.Y : scale.Y);
            //    transformation.M41 = ((flipAdjustment.X - origin.X) * transformation.M11) + position.X;
            //    transformation.M42 = ((flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y;
            //}
            //else
            //{
            //    cos = (float)Math.Cos(rotation);
            //    sin = (float)Math.Sin(rotation);
            //    transformation.M11 = (flippedHorz ? -scale.X : scale.X) * cos;
            //    transformation.M12 = (flippedHorz ? -scale.X : scale.X) * sin;
            //    transformation.M21 = (flippedVert ? -scale.Y : scale.Y) * (-sin);
            //    transformation.M22 = (flippedVert ? -scale.Y : scale.Y) * cos;
            //    transformation.M41 = (((flipAdjustment.X - origin.X) * transformation.M11) + (flipAdjustment.Y - origin.Y) * transformation.M21) + position.X;
            //    transformation.M42 = (((flipAdjustment.X - origin.X) * transformation.M12) + (flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y;
            //}

            var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    offset.X = 0;
                    offset.Y += spriteFont.LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }

                var currentGlyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
                var pCurrentGlyph = spriteFont.Glyphs[currentGlyphIndex];

                // The first character on a line might have a negative left side bearing.
                // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                //  so that text does not hang off the left side of its rectangle.
                if (firstGlyphOfLine)
                {
                    offset.X = Math.Max(pCurrentGlyph.LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                }
                else
                {
                    offset.X += spriteFont.Spacing + pCurrentGlyph.LeftSideBearing;
                }

                var p = offset;

                //if (flippedHorz)
                //    p.X += pCurrentGlyph->BoundsInTexture.Width;
                p.X += pCurrentGlyph.Cropping.X;

                //if (flippedVert)
                //    p.Y += pCurrentGlyph->BoundsInTexture.Height - spriteFont.LineSpacing;
                p.Y += pCurrentGlyph.Cropping.Y;
                p = p.Transform(transformation);

                _texCoordTL.X = pCurrentGlyph.BoundsInTexture.X * spriteFont.Texture.TexelWidth;
                _texCoordTL.Y = pCurrentGlyph.BoundsInTexture.Y * spriteFont.Texture.TexelHeight;
                _texCoordBR.X = (pCurrentGlyph.BoundsInTexture.X + pCurrentGlyph.BoundsInTexture.Width) * spriteFont.Texture.TexelWidth;
                _texCoordBR.Y = (pCurrentGlyph.BoundsInTexture.Y + pCurrentGlyph.BoundsInTexture.Height) * spriteFont.Texture.TexelHeight;

                //if ((effects & SpriteEffects.FlipVertically) != 0)
                //{
                //    var temp = _texCoordBR.Y;
                //    _texCoordBR.Y = _texCoordTL.Y;
                //    _texCoordTL.Y = temp;
                //}
                //if ((effects & SpriteEffects.FlipHorizontally) != 0)
                //{
                //    var temp = _texCoordBR.X;
                //    _texCoordBR.X = _texCoordTL.X;
                //    _texCoordTL.X = temp;
                //}
                offset.X += pCurrentGlyph.Width + pCurrentGlyph.RightSideBearing;

                var rect = new Rectangle((int)p.X, (int)p.Y, (int)(pCurrentGlyph.BoundsInTexture.Width * scale.X), (int)(pCurrentGlyph.BoundsInTexture.Height * scale.Y));
                if (clip.HasValue && !Clip(ref rect, clip.Value, ref _texCoordTL, ref _texCoordBR))
                    continue;

                var item = spritebatch.Batcher.CreateBatchItem();
                item.Texture = spriteFont.Texture;
                item.SortKey = sortKey;
                //if (rotation == 0f)
                //{
                item.Set(rect.X, rect.Y, rect.Width, rect.Height,
                        color,
                        _texCoordTL,
                        _texCoordBR,
                        layerDepth);
                //}
                //else
                //{
                //    item.Set(p.X,
                //            p.Y,
                //            0,
                //            0,
                //            pCurrentGlyph.BoundsInTexture.Width * scale.X,
                //            pCurrentGlyph.BoundsInTexture.Height * scale.Y,
                //            sin,
                //            cos,
                //            color,
                //            _texCoordTL,
                //            _texCoordBR,
                //            layerDepth);
                //}


                // We need to flush if we're using Immediate sort mode.
                //FlushIfNeeded();
            }
        }
        public static void Draw(this SpriteBatch spritebatch, Texture2D tex, Rectangle rect, Color c, float layer)
        {
            spritebatch.Draw(tex, rect, null, c, 0, Vector2.Zero, SpriteEffects.None, layer);
        }
        static Vector2 _texCoordTL, _texCoordBR;
        static bool Clip(ref Rectangle rect, Rectangle clip, ref Vector2 texTL, ref Vector2 texBR)
        {
            if (clip.X >= rect.Right || clip.Right <= rect.X ||
                clip.Y >= rect.Bottom || clip.Bottom <= rect.Y)
                return false;
            var minX = Math.Max(clip.X, rect.X);
            var minY = Math.Max(clip.Y, rect.Y);
            var maxX = Math.Min(clip.Right, rect.Right);
            var maxY = Math.Min(clip.Bottom, rect.Bottom);
            var texWidth = texBR.X - texTL.X;
            var texHeight = texBR.Y - texTL.Y;
            texBR.X = texTL.X + texWidth * (maxX - rect.X) / rect.Width;
            texBR.Y = texTL.Y + texHeight * (maxY - rect.Y) / rect.Height;
            texTL.X = texTL.X + texWidth * (minX - rect.X) / rect.Width;
            texTL.Y = texTL.Y + texHeight * (minY - rect.Y) / rect.Height;
            rect = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            return true;
        }
        public static void Draw(this SpriteBatch spriteBatch, ImageAsset image, Rectangle destinationRectangle, Color color, float layerDepth, Rectangle? sourceRectangle = null, Rectangle? clip = null)
        {
            var texture = image.GetTexture(destinationRectangle);
            if (texture == null)
                return;
            //CheckValid(texture);


            if (sourceRectangle.HasValue)
            {
                var srcRect = sourceRectangle.GetValueOrDefault();
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width) * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;

                //if (srcRect.Width != 0)
                //    origin.X = origin.X * (float)destinationRectangle.Width / (float)srcRect.Width;
                //else
                //    origin.X = origin.X * (float)destinationRectangle.Width * texture.TexelWidth;
                //if (srcRect.Height != 0)
                //    origin.Y = origin.Y * (float)destinationRectangle.Height / (float)srcRect.Height;
                //else
                //    origin.Y = origin.Y * (float)destinationRectangle.Height * texture.TexelHeight;
            }
            else
            {
                _texCoordTL = Vector2.Zero;
                _texCoordBR = Vector2.One;

                //origin.X = origin.X * (float)destinationRectangle.Width * texture.TexelWidth;
                //origin.Y = origin.Y * (float)destinationRectangle.Height * texture.TexelHeight;
            }

            if (clip.HasValue && !Clip(ref destinationRectangle, clip.Value, ref _texCoordTL, ref _texCoordBR))
                return;

            var item = spriteBatch.Batcher.CreateBatchItem();

            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            switch (spriteBatch.SortMode)
            {
                // Comparison of Texture objects.
                case SpriteSortMode.Texture:
                    item.SortKey = 0;
                    break;
                // Comparison of Depth
                case SpriteSortMode.FrontToBack:
                    item.SortKey = layerDepth;
                    break;
                // Comparison of Depth in reverse
                case SpriteSortMode.BackToFront:
                    item.SortKey = -layerDepth;
                    break;
            }

            //if ((effects & SpriteEffects.FlipVertically) != 0)
            //{
            //    var temp = _texCoordBR.Y;
            //    _texCoordBR.Y = _texCoordTL.Y;
            //    _texCoordTL.Y = temp;
            //}
            //if ((effects & SpriteEffects.FlipHorizontally) != 0)
            //{
            //    var temp = _texCoordBR.X;
            //    _texCoordBR.X = _texCoordTL.X;
            //    _texCoordTL.X = temp;
            //}

            //if (rotation == 0f)
            //{
            item.Set(destinationRectangle.X,
                    destinationRectangle.Y,
                    destinationRectangle.Width,
                    destinationRectangle.Height,
                    color,
                    _texCoordTL,
                    _texCoordBR,
                    layerDepth);
            //}
            //else
            //{
            //    item.Set(destinationRectangle.X,
            //            destinationRectangle.Y,
            //            -origin.X,
            //            -origin.Y,
            //            destinationRectangle.Width,
            //            destinationRectangle.Height,
            //            (float)Math.Sin(rotation),
            //            (float)Math.Cos(rotation),
            //            color,
            //            _texCoordTL,
            //            _texCoordBR,
            //            layerDepth);
            //}

            //FlushIfNeeded();
        }
    }
}
