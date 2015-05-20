using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InterfaceElements
{
    public delegate void InterfaceEventHandler(InterfaceElement clicked);
    public class InterfaceElement : Element
    {
        public static ScalableTexture DefaultTexture;
        public static SpriteFont DefaultFont;
        public ScalableTexture Texture { get; protected set; }
        public event InterfaceEventHandler OnClick;
        public object Tag;
        public int layer = 0;
        public SpriteFont Font { get; protected set; }
        public InterfaceElement(Element parent, SpriteFont font = null, ScalableTexture texture = null)
            : base(parent)
        {
            Font = font ?? DefaultFont;
            Texture = texture ?? DefaultTexture;
        }
        public virtual Rectangle InsideRect
        {
            get { return new Rectangle(Rect.X + Texture.BorderWidth, Rect.Y + Texture.BorderWidth, Rect.Width - 2 * Texture.BorderWidth, Rect.Height - 2 * Texture.BorderWidth); }
        }
        public string MouseOverText;
        public bool MouseInside()
        {
            return Rect.Contains(Mouse.GetState().X, Mouse.GetState().Y);
        }
        public override void Draw(GameTime time, float layer)
        {
            if (MouseInside() && MouseOverText != null)
            {
                ScreenManager.CurrentManager.DrawString(Font, MouseOverText, new Vector2(Mouse.GetState().X + 15, Mouse.GetState().Y), Color.Black, ScreenManager.TopLayer(layer));
            }
            if(Texture != null)
            {
                Texture.Draw(Rect, ScreenManager.CurrentManager.SpriteBatch, layer);
            }
        }
        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            if (otherTookInput) { return false; }
            if (input.IsNewMousePress(0))
            {
                if (MouseInside())
                {
                    if (OnClick != null)
                    {
                        OnClick(this);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
