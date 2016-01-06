using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Graphics
{
    public class TerrainEffect : SpectrumEffect
    {
        public float VertexBlend
        {
            get { return Parameters["VertexBlend"].GetValueSingle(); }
            set { Parameters["VertexBlend"].SetValue(value); }
        }

        public TerrainEffect(string textureA, string textureB, string textureC, string textureD)
            : base(ContentHelper.Load<Effect>("TerrainEffect"))
        {
            //Parameters["MultiTextureA"].SetValue(ContentHelper.Load<Texture2D>(textureA));
            Parameters["MultiTextureB"].SetValue(ContentHelper.Load<Texture2D>(textureB));
            //Parameters["MultiTextureC"].SetValue(ContentHelper.Load<Texture2D>(textureC));
            //Parameters["MultiTextureD"].SetValue(ContentHelper.Load<Texture2D>(textureD));
        }
    }
}
