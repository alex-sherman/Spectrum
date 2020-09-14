using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{
    public class TextElement : Element
    {
        private int measuredWidth;
        private int measuredHeight;
        private int contentWidth;
        private int contentHeight;
        private bool _dirty = false;
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    _dirty = true;
                }
            }
        }
        public Func<string> TextSource;

        public TextElement(string text = null) { Text = text; }
        private void CacheDims()
        {
            if (_dirty)
            {
                _dirty = false;
                if (Text == null)
                {
                    contentWidth = 0;
                    contentHeight = (int)Font.MeasureString("a").Y;
                }
                else
                {
                    contentWidth = (int)Font.MeasureString(Text).X;
                    contentHeight = (int)Math.Max(Font.MeasureString("a").Y, Font.MeasureString(Text).Y);
                }
            }
        }
        public override int ContentWidth { get { CacheDims(); return contentWidth; } }
        public override int ContentHeight { get { CacheDims(); return contentHeight; } }

        public override void OnMeasure(int width, int height)
        {
            if (TextSource != null)
                Text = TextSource();
            CacheDims();
            MeasuredWidth = MeasureWidth(width, contentWidth);
            MeasuredHeight = MeasureHeight(height, contentHeight);
        }

        public override void Draw(float time, SpriteBatch spritebatch)
        {
            base.Draw(time, spritebatch);
            if (Text != null)
                spritebatch.DrawString(Font, Text, new Vector2(Rect.X, Rect.Y), FontColor, LayerDepth, Parent?.Clipped);
        }
    }
}
