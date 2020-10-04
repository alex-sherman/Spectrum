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
            var random = new Random(0);
            var sampleCount = 16;
            HBAOSamples = Enumerable.Range(0, sampleCount)
                .Select(i =>
                {
                    var v = new Vector3(random.NextFloat(-1, 1), random.NextFloat(-1, 1), random.NextFloat(0, 1)).Normal();
                    MathHelper.Lerp(.1f, 1f, 1.0f * i / sampleCount);
                    return v;
                }).ToArray();
        }
        public static void ResetViewPort(int width, int height)
        {
            effect.Parameters["viewPort"].SetValue(new Vector2(width, height));
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
        public static Texture2D PositionTarget
        {
            get { return effect.Parameters["PositionTarget"].GetValueTexture2D(); }
            set { effect.Parameters["PositionTarget"].SetValue(value); }
        }
        public static bool AAEnabled
        {
            get { return effect.Parameters["AAEnabled"].GetValueBoolean(); }
            set { effect.Parameters["AAEnabled"].SetValue(value); }
        }
        public static float AAThreshold
        {
            get { return effect.Parameters["aaThreshold"].GetValueSingle(); }
            set { effect.Parameters["aaThreshold"].SetValue(value); }
        }
        public static float AABlurFactor
        {
            get { return effect.Parameters["aaBlurFactor"].GetValueSingle(); }
            set { effect.Parameters["aaBlurFactor"].SetValue(value); }
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
        public static Vector3 CameraPosition
        {
            get => effect.Parameters["cameraPosition"].GetValueVector3();
            set => effect.Parameters["cameraPosition"].SetValue(value);
        }
        public static Vector3[] HBAOSamples
        {
            get => effect.Parameters["hbaoSamples"].GetValueVector3Array().Cast<Vector3>().ToArray();
            set => effect.Parameters["hbaoSamples"].SetValue(value.Select(v => (Microsoft.Xna.Framework.Vector3)v).ToArray());
        }
        public static bool Enabled = true;
    }
}
