using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens
{
    public class ScalableTexture
    {
        public int BorderWidth;
        public string TextureName;
        public Texture2D Texture;
        public Rectangle ULCornerSource;
        public Rectangle URCornerSource;
        public Rectangle LLCornerSource;
        public Rectangle LRCornerSource;
        public Rectangle TopSource;
        public Rectangle LeftSource;
        public Rectangle RightSource;
        public Rectangle BottomSource;
        public Rectangle CenterSource;
        private Rectangle rect;
        public ScalableTexture(Texture2D texture, int borderwidth)
        {
            BorderWidth = borderwidth;
            Texture = texture;
            ULCornerSource = new Rectangle(0, 0, BorderWidth, BorderWidth);
            URCornerSource = new Rectangle(Texture.Bounds.Width - BorderWidth, 0, BorderWidth, BorderWidth);
            LLCornerSource = new Rectangle(0, Texture.Bounds.Height - BorderWidth, BorderWidth, BorderWidth);
            LRCornerSource = new Rectangle(Texture.Bounds.Width - BorderWidth, Texture.Bounds.Height - BorderWidth, BorderWidth, BorderWidth);
            LeftSource = new Rectangle(0, BorderWidth, BorderWidth, Texture.Bounds.Height - BorderWidth * 2);
            RightSource = new Rectangle(Texture.Bounds.Width - BorderWidth, BorderWidth, BorderWidth, Texture.Bounds.Height - BorderWidth * 2);
            TopSource = new Rectangle(BorderWidth, 0, Texture.Bounds.Width - BorderWidth * 2, BorderWidth);
            BottomSource = new Rectangle(BorderWidth, Texture.Bounds.Height - BorderWidth, Texture.Bounds.Width - BorderWidth * 2, BorderWidth);
            CenterSource = new Rectangle(BorderWidth, BorderWidth, Texture.Bounds.Width - 2 * BorderWidth, Texture.Bounds.Height - 2 * BorderWidth);
        }
        public ScalableTexture(string texture, int borderwidth)
            : this(ContentHelper.Load<Texture2D>(texture), borderwidth) { }
        public Rectangle ULCorner
        {
            get { return new Rectangle(rect.X, rect.Y, BorderWidth, BorderWidth); }
        }
        public Rectangle URCorner
        {
            get { return new Rectangle(rect.X + rect.Width - BorderWidth, rect.Y, BorderWidth, BorderWidth); }
        }
        public Rectangle LLCorner
        {
            get { return new Rectangle(rect.X, rect.Y + rect.Height - BorderWidth, BorderWidth, BorderWidth); }
        }
        public Rectangle LRCorner
        {
            get { return new Rectangle(rect.X + rect.Width - BorderWidth, rect.Y + rect.Height - BorderWidth, BorderWidth, BorderWidth); }
        }
        public Rectangle Top
        {
            get { return new Rectangle(rect.X + BorderWidth, rect.Y, rect.Width - 2 * BorderWidth, BorderWidth); }
        }
        private Rectangle Left
        {
            get { return new Rectangle(rect.X, rect.Y + BorderWidth, BorderWidth, rect.Height - 2 * BorderWidth); }
        }
        private Rectangle Right
        {
            get { return new Rectangle(rect.X + rect.Width - BorderWidth, rect.Y + BorderWidth, BorderWidth, rect.Height - 2 * BorderWidth); }
        }
        private Rectangle Bottom
        {
            get { return new Rectangle(rect.X + BorderWidth, rect.Y + rect.Height - BorderWidth, rect.Width - 2 * BorderWidth, BorderWidth); }
        }
        private Rectangle Center
        {
            get { return new Rectangle(rect.X + BorderWidth, rect.Y + BorderWidth, rect.Width - 2 * BorderWidth, rect.Height - 2 * BorderWidth); }
        }

        public void Draw(Rectangle destination, SpriteBatch spriteBatch, float layer)
        {
            rect = destination;
            spriteBatch.Draw(Texture, ULCorner, ULCornerSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, layer);
            spriteBatch.Draw(Texture, URCorner, URCornerSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, layer);
            spriteBatch.Draw(Texture, LLCorner, LLCornerSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, layer);
            spriteBatch.Draw(Texture, LRCorner, LRCornerSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, layer);
            spriteBatch.Draw(Texture, Left, LeftSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, layer);
            spriteBatch.Draw(Texture, Right, RightSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, layer);
            spriteBatch.Draw(Texture, Top, TopSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, layer);
            spriteBatch.Draw(Texture, Bottom, BottomSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, layer);
            spriteBatch.Draw(Texture, Center, CenterSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, layer);
        }
    }
}
