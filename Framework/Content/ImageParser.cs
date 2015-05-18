using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    class ImageParser : CachedContentParser<Texture2D, Texture2D>
    {
        protected override Texture2D LoadData(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                if (System.IO.File.Exists(path + ".png")) path += ".png";
                else if (System.IO.File.Exists(path + ".jpg")) path += ".jpg";
                else throw new FileNotFoundException("The texture could not be loaded: ", path);
            }
            return Texture2D.FromStream(SpectrumGame.Game.GraphicsDevice, new FileStream(path, FileMode.Open, FileAccess.Read));
        }

        protected override Texture2D SafeCopy(Texture2D cache)
        {
            return cache;
        }
    }
}
