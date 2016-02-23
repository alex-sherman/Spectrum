using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Graphics.Effects
{
    public class GrassEffect : SpectrumEffect
    {
        public Matrix[] Worlds { set { this.Parameters["worlds"].SetValue(value); } }
        public GrassEffect()
            : base(ContentHelper.Load<Effect>("Grass"))
        {
        }
    }
}
