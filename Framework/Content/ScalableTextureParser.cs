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
        protected override ScalableTexture LoadData(string path, string name)
        {
            Texture2D texture = ContentHelper.ContentParsers[typeof(Texture2D)].Load(path, name) as Texture2D;
            if (texture == null)
                throw new FileNotFoundException();
            return new ScalableTexture(texture, 0);
        }

        protected override ScalableTexture SafeCopy(ScalableTexture cache)
        {
            return cache;
        }
    }
}
