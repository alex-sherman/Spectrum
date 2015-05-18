using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Graphics
{
    public class SpectrumEffect : Effect
    {
        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        public Matrix World
        {
            get { return Parameters["world"].GetValueMatrix(); }

            set
            {
                Parameters["world"].SetValue(value);
            }
        }


        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        public Matrix View
        {
            get { return Parameters["view"].GetValueMatrix(); }

            set
            {
                Parameters["view"].SetValue(value);
            }
        }


        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get { return Parameters["proj"].GetValueMatrix(); }

            set
            {
                Parameters["proj"].SetValue(value);
            }
        }
        public float MixLerp
        {
            set { Parameters["mixLerp"].SetValue(value); }
        }
        public Color MixColor
        {
            set { Parameters["mixColor"].SetValue(value.ToVector3()); }
        }
        public bool LightingEnabled
        {
            get { return Parameters["lightingEnabled"].GetValueBoolean(); }
            set { Parameters["lightingEnabled"].SetValue(value); }
        }
        public static Vector3 LightPos;

        public static Vector4 SpecularLightColor;
        public static Vector3 CameraPos = new Vector3();
        public static bool Clip = false;
        public static bool AboveWater = true;
        public static Vector4 ClipPlane;
        public Texture2D Texture { set { Parameters["Texture"].SetValue(value); } }
        //TODO: Set values only when necessary
        protected override bool OnApply()
        {
            Parameters["aboveWater"].SetValue(AboveWater);
            Parameters["Clip"].SetValue(Clip);
            Parameters["cameraPosition"].SetValue(CameraPos);
            Parameters["ClipPlane"].SetValue(ClipPlane);
            Parameters["specularLightColor"].SetValue(SpecularLightColor);
            Parameters["lightPosition"].SetValue(LightPos);
            return base.OnApply();
        }
        public SpectrumEffect() : this(ContentHelper.Load<Effect>("SpectrumEffect")) { }
        public SpectrumEffect(Effect effect)
            : base(effect)
        {
            Parameters["ambientLightColor"].SetValue(
                Color.White.ToVector4() * .4f);
            Parameters["diffuseLightColor"].SetValue(
                Color.White.ToVector4());
            Parameters["specularLightColor"].SetValue(
                Color.White.ToVector4() / 3);
            effect.Parameters["lightPosition"].SetValue(
                    new Vector3(10f, 10f, 0));
            Parameters["ClipPlane"].SetValue(new Vector4(0, 1, 0, -Water.waterHeight));
            Parameters["world"].SetValue(Matrix.Identity);
        }
    }
}
