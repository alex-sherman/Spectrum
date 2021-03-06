﻿using Microsoft.Xna.Framework;
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
            set => Parameters["world"]?.SetValue(value);
        }
        public Matrix View
        {
            set => Parameters["view"]?.SetValue(value);
        }
        public Matrix Projection
        {
            set => Parameters["proj"]?.SetValue(value);
        }

        public bool LightingEnabled
        {
            set { Parameters["lightingEnabled"]?.SetValue(value); }
        }
        public static Vector3 LightPos;
        public static Matrix LightView;
        public static Vector3 DiffuseLightColor = new Vector3(1f);
        public static Vector3 AmbientLightColor = new Vector3(0.5f);
        public static Vector3 SpecularLightColor = new Vector3(1);
        public static Vector3 CameraPos = new Vector3();
        public static bool Clip = false;
        public static Vector4 ClipPlane;
        public Color DiffuseColor
        {
            set => Parameters["diffuseColor"]?.SetValue(value.ToVector4());
        }
        public Color MaterialDiffuse
        {
            set => Parameters["materialDiffuse"]?.SetValue(value.ToVector4());
        }
        public Vector2 DiffuseTextureOffset
        {
            set => Parameters["DiffuseTextureOffset"]?.SetValue(value);
        }
        public Texture2D Texture
        {
            set
            {
                Parameters["UseTexture"]?.SetValue(value != null);
                if ((value?.Tag as Texture2DData)?.HasAlpha ?? false)
                    HasTransparency = true;
                Parameters["Texture"]?.SetValue(value);
            }
        }
        public bool TextureMagFilter
        {
            set
            {
                Parameters["TextureMagFilter"]?.SetValue(value);
            }
        }
        public static float ShadowThreshold = 2.7e-5f;
        public Texture2D ShadowMap
        {
            set
            {
                Parameters["UseShadowMap"]?.SetValue(value != null);
                Parameters["ShadowMapTexture"]?.SetValue(value);
            }
        }
        public Texture2D NormalMap { set { Parameters["NormalMap"].SetValue(value); Parameters["UseNormalMap"].SetValue(value != null); } }
        public bool HasTransparency { get; set; } = false;
        public virtual bool CanInstance { get { return true; } }
        public Texture2D Transparency
        {
            set
            {
                Parameters["Transparency"].SetValue(value);
                Parameters["UseTransparency"].SetValue(value != null);
                HasTransparency = value != null;
            }
        }

        //TODO: Set values only when necessary
        protected override void OnApply()
        {
            Parameters["ShadowThreshold"]?.SetValue(ShadowThreshold);
            Parameters["Clip"]?.SetValue(Clip);
            Parameters["cameraPosition"]?.SetValue(CameraPos);
            Parameters["ClipPlane"]?.SetValue(ClipPlane);
            Parameters["specularLightColor"]?.SetValue(SpecularLightColor);
            Parameters["ambientLightColor"]?.SetValue(AmbientLightColor);
            Parameters["diffuseLightColor"]?.SetValue(DiffuseLightColor);
            Parameters["lightPosition"]?.SetValue(LightPos);
            Parameters["ShadowViewProjection"]?.SetValue(LightView * Settings.lightProjection);
            base.OnApply();
        }
        public SpectrumEffect() : this(ContentHelper.Load<Effect>("SpectrumEffect")) { }
        public SpectrumEffect(Effect effect) : base(effect) { }
        public void SetTechnique(string technique)
        {
            CurrentTechnique = Techniques[technique];
        }
        public override Effect Clone()
        {
            return new SpectrumEffect(this);
        }
    }
}
