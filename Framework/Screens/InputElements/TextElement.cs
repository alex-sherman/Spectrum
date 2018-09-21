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
        private int _measuredWidth;
        private int _measuredHeight;
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

        public override void OnMeasure(int width, int height)
        {
            if (TextSource != null)
                Text = TextSource();
            if (_dirty)
            {
                _dirty = false;
                if (Text == null)
                {
                    _measuredWidth = MeasureWidth(width, 0);
                    _measuredHeight = MeasureHeight(height, (int)Font.MeasureString("a").Y);
                }
                else
                {
                    _measuredWidth = MeasureWidth(width, (int)Font.MeasureString(Text).X);
                    _measuredHeight = MeasureHeight(height, (int)Math.Max(Font.MeasureString("a").Y, Font.MeasureString(Text).Y));
                }
            }
            MeasuredWidth = _measuredWidth;
            MeasuredHeight = _measuredHeight;
        }

        public override void Draw(float time, SpriteBatch spritebatch, float layer)
        {
            base.Draw(time, spritebatch, layer);
            if (Text != null)
                spritebatch.DrawString(Font, Text, new Vector2(Rect.X, Rect.Y), FontColor, layer, Parent?.Clipped);
        }
    }
}
