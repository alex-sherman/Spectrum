using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Screens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    public class ScalableTextureParser : CachedContentParser<ScalableTexture, ScalableTexture>
    {
        public ScalableTextureParser()
        {
            Prefix = @"Textures\";
        }
        protected override ScalableTexture LoadData(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                if (System.IO.File.Exists(path + ".png")) path += ".png";
                else if (System.IO.File.Exists(path + ".jpg")) path += ".jpg";
                else throw new FileNotFoundException("The texture could not be loaded: ", path);
            }
            return new ScalableTexture(Texture2D.FromStream(SpectrumGame.Game.GraphicsDevice, new FileStream(path, FileMode.Open, FileAccess.Read)), 0);
        }

        protected override ScalableTexture SafeCopy(ScalableTexture cache)
        {
            return cache;
        }
    }
}
