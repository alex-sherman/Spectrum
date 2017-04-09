using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using Spectrum.Framework.Graphics.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Graphics
{
    public class SpectrumEffect : Effect
    {
        public Matrix World
        {
            get { return Parameters["world"].GetValueMatrix(); }

            set
            {
                Parameters["world"].SetValue(value);
            }
        }
        public Matrix View
        {
            get { return Parameters["view"].GetValueMatrix(); }

            set
            {
                Parameters["view"].SetValue(value);
            }
        }
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
        public static Vector3 DiffuseLightColor = new Vector3(0.8f);
        public static Vector3 AmbientLightColor = new Vector3(0.2f);
        public static Vector3 SpecularLightColor = new Vector3(1);
        public static Vector3 CameraPos = new Vector3();
        public static bool Clip = false;
        public static bool AboveWater = true;
        public static Vector4 ClipPlane;
        public Color DiffuseColor
        {
            set
            {
                Vector4 color = value.ToVector4();
                Parameters["diffuseColor"].SetValue(color);
            }
        }
        public Texture2D Texture
        {
            set
            {
                Parameters["UseTexture"].SetValue(value != null);
                if (value != null && (value.Tag as Texture2DData).HasAlpha)
                    HasTransparency = true;
                Parameters["Texture"].SetValue(value);
            }
        }
        public Texture2D NormalMap { set { Parameters["NormalMap"].SetValue(value); Parameters["UseNormalMap"].SetValue(value != null); } }
        public bool HasTransparency { get; protected set; } = false;
        public Texture2D Transparency
        {
            set
            {
                Parameters["Transparency"].SetValue(value);
                Parameters["UseTransparency"].SetValue(value != null);
                HasTransparency = value != null;
            }
        }

        private string[] BoneNames;
        public Matrix[] BoneTransforms { get; protected set; }
        //TODO: Set values only when necessary
        protected override bool OnApply()
        {
            Parameters["aboveWater"].SetValue(AboveWater);
            Parameters["Clip"].SetValue(Clip);
            Parameters["cameraPosition"].SetValue(CameraPos);
            Parameters["ClipPlane"].SetValue(ClipPlane);
            Parameters["specularLightColor"].SetValue(SpecularLightColor);
            Parameters["ambientLightColor"].SetValue(AmbientLightColor);
            Parameters["diffuseLightColor"].SetValue(DiffuseLightColor);
            Parameters["lightPosition"].SetValue(LightPos);
            if (BoneTransforms != null)
                Parameters["Bones"].SetValue(BoneTransforms);
            return base.OnApply();
        }
        public SpectrumEffect() : this(ContentHelper.Load<Effect>("SpectrumEffect")) { }
        public SpectrumEffect(Effect effect)
            : base(effect)
        {
            Parameters["ambientLightColor"].SetValue(
                Color.White.ToVector3() * 0.2f);
            Parameters["diffuseLightColor"].SetValue(
                Color.White.ToVector3() * 0.8f);
            Parameters["specularLightColor"].SetValue(
                Color.White.ToVector3() / 3);
            effect.Parameters["lightPosition"].SetValue(
                    new Vector3(10f, 10f, 0));
            Parameters["ClipPlane"].SetValue(new Vector4(0, 1, 0, -Water.waterHeight));
            Parameters["world"].SetValue(Matrix.Identity);
        }
        public void SetTechnique(string technique)
        {
            CurrentTechnique = Techniques[technique];
        }
        public void SetBoneNames(params string[] boneNames)
        {
            BoneTransforms = new Matrix[boneNames.Count()];
            for (int i = 0; i < boneNames.Count(); i++)
            {
                BoneTransforms[i] = Matrix.Identity;
            }
            BoneNames = boneNames;
        }
        public void UpdateBoneTransforms(SkinningData SkinningData)
        {
            for (int i = 0; i < BoneTransforms.Count(); i++)
            {
                BoneTransforms[i] = SkinningData.Bones[BoneNames[i]].absoluteTransform;
            }
        }
    }
}
