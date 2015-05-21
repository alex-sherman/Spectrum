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
        }

        public override void Draw(GameTime gameTime, float layer)
        {
            base.Draw(gameTime, layer);
        }
    }
}