using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content
{
    class EffectParser : CachedContentParser<byte[], Effect>
    {
        public EffectParser()
        {
            Prefix = "HLSL";
        }
        protected override byte[] LoadData(string path, string name)
        {
            path = TryThrowExtensions(path, ".mgfx");
            using (var f = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] output = new byte[f.Length];
                f.Read(output, 0, (int)f.Length);
                return output;
            }
        }

        protected override Effect SafeCopy(byte[] data)
        {
            return new Effect(SpectrumGame.Game.GraphicsDevice, data);
        }
    }
}
