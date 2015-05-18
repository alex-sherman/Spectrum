using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;

namespace Spectrum.Framework.Screens
{
    public class BackgroundScreen : GameScreen
    {
        Texture2D texture;

        /// <summary>
        /// Constructor
        /// </summary>
        public BackgroundScreen(Texture2D texture)
        {
            this.texture = texture;
            TransitionOnTime = TimeSpan.FromSeconds(0.5f);
            TransitionOffTime = TimeSpan.FromSeconds(0.5f);
        }

        public override void Draw(GameTime gameTime, float layer)
        {
            Viewport viewport = ScreenManager.CurrentManager.Viewport;

            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            float fade = TransitionAlpha;

            ScreenManager.CurrentManager.Draw(texture,
                                           fullscreen,
                                           new Color(fade, fade, fade),layer);
        }
    }
}