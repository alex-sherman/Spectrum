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
        public static void ResetViewPort()
        {
            effect.Parameters["viewPort"].SetValue(new Vector2(SpectrumGame.Game.GraphicsDevice.Viewport.Width, SpectrumGame.Game.GraphicsDevice.Viewport.Height));
        }
        public static float DepthBlurStart
        {
            get { return effect.Parameters["depthBlurStart"].GetValueSingle(); }
            set { effect.Parameters["depthBlurStart"].SetValue(value); }
        }
        public static float DepthBlurScale
        {
            get { return effect.Parameters["depthBlurScale"].GetValueSingle(); }
            set { effect.Parameters["depthBlurScale"].SetValue(value); }
        }
        public static Texture2D DepthTarget
        {
            get { return effect.Parameters["DepthTarget"].GetValueTexture2D(); }
            set { effect.Parameters["DepthTarget"].SetValue(value); }
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
        public static string Technique
        {
            get { return effect.CurrentTechnique.Name; }
            set { effect.CurrentTechnique = effect.Techniques[value]; }
        }
    }
}
