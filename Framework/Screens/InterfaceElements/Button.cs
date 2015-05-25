using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InterfaceElements
{
    public class Button : InterfaceElement
    {
        public object DrawObject;
        public string Text;

        public Button(int width, int height, ScalableTexture texture = null)
            : base(texture: texture)
        {
            FlatWidth = width;
            FlatHeight = height;
        }

        public Button(string text, ScalableTexture texture = null)
            : base(texture: texture)
        {
            Text = text;
        }

        public override void Initialize()
        {
            base.Initialize();
            if (Text != null)
            {
                FlatWidth = (int)Font.MeasureString(Text).X + 2 * Texture.BorderWidth;
                FlatHeight = (int)Font.LineSpacing + 2 * Texture.BorderWidth;
            }
        }

        public override void Draw(GameTime time)
        {
            if (DrawObject != null)
            {
                ScreenManager.CurrentManager.Draw(ScreenManager.CurrentManager.TextureLoader.ObjectTexture(DrawObject), InsideRect, Color.White, Z);
            }
            if (Text != null)
            {
                Vector2 pos = new Vector2(Rect.X, Rect.Y) + (new Vector2(Rect.Width, Rect.Height) - Font.MeasureString(Text)) / 2;
                ScreenManager.CurrentManager.DrawString(Font, Text, pos, Color.Black, Layer(2));
            }
            base.Draw(time);
        }
    }
}
