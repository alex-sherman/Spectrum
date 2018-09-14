using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content
{
    class EffectParser : CachedContentParser<Effect, Effect>
    {
        public EffectParser() : base("mgfx")
        {
            Prefix = "HLSL";
        }
        protected override Effect LoadData(string path, string name)
        {
            using (var f = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] output = new byte[f.Length];
                f.Read(output, 0, (int)f.Length);
                return new Effect(SpectrumGame.Game.GraphicsDevice, output);
            }
        }

        protected override Effect SafeCopy(Effect toClone)
        {
            return toClone.Clone();
        }
    }
    class SpectrumEffectParser : EffectParser
    {
        protected override Effect SafeCopy(Effect toClone)
        {
            return new SpectrumEffect(toClone);
        }
    }
}
