using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Graphics
{
    class WaterEffect : SpectrumEffect
    {
        public static float WaterPerturbation = 1.4f;
        public static Matrix ReflectionView;
        public static Matrix ReflectionProj;
        public static float WaterTime;
        protected override void OnApply()
        {
            base.OnApply();
            Parameters["waterPerturbCoef"].SetValue(WaterPerturbation);
            Parameters["reflView"].SetValue(ReflectionView);
            Parameters["reflProj"].SetValue(ReflectionProj);
            Parameters["waterTime"].SetValue(WaterTime);
            Texture2D faff = Parameters["Reflection"].GetValueTexture2D();
            Texture2D test = Parameters["Refraction"].GetValueTexture2D();
            Parameters["Reflection"].SetValue(Water.reflectionRenderTarget);
            Parameters["Refraction"].SetValue(Water.refractionRenderTarget);
        }
        public WaterEffect(string waterBumpMap1, string waterBumpMap2)
            : base(ContentHelper.Load<Effect>("WaterEffect"))
        {

            Parameters["WaterBumpBase"].SetValue(ContentHelper.Load<Texture2D>(waterBumpMap1));
            Parameters["WaterBump"].SetValue(ContentHelper.Load<Texture2D>(waterBumpMap2));
            Vector2 windDirection = (new Vector2(1, 2));
            windDirection.Normalize();
            Parameters["windDirection"].SetValue(windDirection);
        }
    }
}
