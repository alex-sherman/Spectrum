using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;

namespace Spectrum.Framework.Graphics
{
    public class PostProcessEffect
    {
        public static Effect effect;
        public static void Initialize()
        {
            effect = ContentHelper.Load<Effect>(@"PostProcessEffect");
            effect.Parameters["darkness"].SetValue(0.0f);
            ResetViewPort();
        }
        public static bool ShadowMapEnabled
        {
            get { return effect.Parameters["shadowMapEnabled"].GetValueBoolean(); }
            set { effect.Parameters["shadowMapEnabled"].SetValue(value); }
        }
        public static void ResetViewPort()
        {
            effect.Parameters["viewPort"].SetValue(new Vector2(SpectrumGame.Game.GraphicsDevice.Viewport.Width, SpectrumGame.Game.GraphicsDevice.Viewport.Height));
        }
        public static Matrix LightViewProj
        {
            get { return effect.Parameters["lightViewProjectionMatrix"].GetValueMatrix(); }
            set { effect.Parameters["lightViewProjectionMatrix"].SetValue(value); }
        }
        public static Texture2D AATarget
        {
            get { return effect.Parameters["AATarget"].GetValueTexture2D(); }
            set { effect.Parameters["AATarget"].SetValue(value); }
        }
        public static Texture2D ShadowMap
        {
            get { return effect.Parameters["ShadowMapTex"].GetValueTexture2D(); }
            set { effect.Parameters["ShadowMapTex"].SetValue(value); }
        }
        public static bool AAEnabled
        {
            get { return effect.Parameters["AAEnabled"].GetValueBoolean(); }
            set { effect.Parameters["AAEnabled"].SetValue(value); }
        }
        public static float Darkness
        {
            get { return effect.Parameters["darkness"].GetValueSingle(); }
            set { effect.Parameters["darkness"].SetValue(value); }
        }
        public static bool Vingette
        {
            get { return effect.Parameters["vingette"].GetValueBoolean(); }
            set { effect.Parameters["vingette"].SetValue(value); }
        }
        public static String Technique
        {
            get { return effect.CurrentTechnique.Name; }
            set { effect.CurrentTechnique = effect.Techniques[value]; }
        }
        public static void ApplyPass(int pass)
        {
            effect.CurrentTechnique.Passes[pass].Apply();
        }
    }
}
