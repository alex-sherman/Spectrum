using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{
    public class Button : InputElement
    {
        public object DrawObject;
        public string Text;

        public Button(int width, int height)
        {
            FlatWidth = width;
            FlatHeight = height;
        }

        public Button(string text)
        {
            Text = text;
        }

        public override void Initialize()
        {
            base.Initialize();
            if (Text != null)
            {
                FlatWidth = (int)Font.MeasureString(Text).X;
                FlatHeight = (int)Font.LineSpacing;
            }
        }

        public override void Draw(GameTime time)
        {
            base.Draw(time);
            if (DrawObject != null)
            {
                ScreenManager.CurrentManager.Draw(ScreenManager.CurrentManager.TextureLoader.ObjectTexture(DrawObject), Rect, Color.White, Layer(1));
            }
            if (Text != null)
            {
                Vector2 pos = new Vector2(Rect.X, Rect.Y) + (new Vector2(Rect.Width, Rect.Height) - Font.MeasureString(Text)) / 2;
                ScreenManager.CurrentManager.DrawString(Font, Text, pos, FontColor, Layer(2));
            }
        }
    }
}
