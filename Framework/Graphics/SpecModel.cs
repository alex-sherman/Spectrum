﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Graphics.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Graphics
{
    public class SpecModel : List<DrawablePart>
    {
        public string Path { get; private set; }
        public AnimationPlayer AnimationPlayer { get; set; }
        public SkinningData SkinningData { get; protected set; }
        public Dictionary<string, DrawablePart> MeshParts { get; private set; }
        public string CurrentAnimation
        {
            get
            {
                if (AnimationPlayer == null) { return ""; }
                if (AnimationPlayer.CurrentClip == null) { return ""; }
                return AnimationPlayer.CurrentClip.Name;
            }
            set
            {
                if (AnimationPlayer != null && AnimationPlayer.AnimationNames.Contains(value)) { AnimationPlayer.StartClip(value, SkinningData); }
            }
        }
        public void StartAnimation(string animation, bool playOnce = false)
        {
            if (AnimationPlayer != null)
                AnimationPlayer.StartClip(animation, SkinningData, playOnce);
        }
        public SpecModel(string path, Dictionary<string, DrawablePart> meshParts, SkinningData skinningData)
        {
            Path = path;
            MeshParts = meshParts;
            AddRange(meshParts.Values);
            SkinningData = skinningData;
        }
        public Texture2D Texture
        {
            set
            {
                foreach (var part in this)
                {
                    part.effect.Texture = value;
                }
            }
        }
        public void Update(GameTime gameTime)
        {
            if (AnimationPlayer != null)
            {
                AnimationPlayer.Update(gameTime.ElapsedGameTime, SkinningData);
            }
            foreach (var part in this)
            {
                if (SkinningData != null)
                    part.effect.UpdateBoneTransforms(SkinningData);
            }
        }
    }
}
