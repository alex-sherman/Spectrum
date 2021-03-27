#region File Description
//-----------------------------------------------------------------------------
// AnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Graphics.Animation;
using Newtonsoft.Json.Linq;
using System.Linq;
#endregion

namespace Spectrum.Framework.Graphics.Animation
{

    public interface IAnimationSource
    {
        AnimationClip GetAnimation(string name);
        SkinningData GetSkinningData();
    }
    // TODO: Add weights and transitions or something
    class AnimationLayer
    {
        public bool Loop;
        public float Time;
        public AnimationClip Clip;
        public bool UpdateTime(float dt)
        {
            if (Clip == null || Clip.Duration == 0) { return false; }
            Time += dt;
            if (Time > Clip.Duration && Loop)
                Time -= Clip.Duration;
            if (Time > Clip.Duration) return true;
            return false;
        }
        public void Apply(SkinningData skinningData)
        {
            if (Clip == null) return;
            foreach (var bone in new HashSet<string>(Clip.Translations.Keys.Union(Clip.Rotations.Keys)))
            {
                if (!skinningData.Bones.ContainsKey(bone)) continue;
                Bone currentBone = skinningData.Bones[bone];

                if (Clip.Translations.TryGetValue(bone, out var translations))
                    currentBone.Translation =
                        translations.LerpTo(Time, currentBone.Translation, (a, b, w) => a * (1 - w) + b * w);
                if (Clip.Rotations.TryGetValue(bone, out var rotations))
                    currentBone.Rotation = rotations.LerpTo(Time, currentBone.Rotation, Quaternion.Slerp);
                // TODO: I think this can be combined into one operation
            }
        }
    }
    /// <summary>
    /// The animation player is in charge of decoding bone position
    /// matrices from an animation clip.
    /// </summary>
    public class AnimationPlayer
    {
        #region Fields
        // Information about the currently playing animation clip.
        AnimationLayer Base;
        List<AnimationLayer> layers = new List<AnimationLayer>();
        public string DefaultClip = "Idle";
        IAnimationSource animationSource;
        #endregion

        public AnimationPlayer(IAnimationSource animationSource)
        {
            this.animationSource = animationSource;
        }

        public string CurrentClip() => Base?.Clip?.Name;

        public void StartClip(string animation, bool loop = false)
        {
            StartClip(animationSource?.GetAnimation(animation), loop);
        }

        /// <summary>
        /// Starts decoding the specified animation clip.
        /// </summary>
        public void StartClip(AnimationClip clip, bool loop)
        {
            if (clip == null)
            {
                Base = null;
                return;
            }
            Base = new AnimationLayer() { Clip = clip, Loop = loop, Time = 0 };
            Update(0);
        }

        public void StartLayer(string animation)
        {
            var clip = animationSource?.GetAnimation(animation);
            if (clip != null)
                StartLayer(clip);
        }

        public void StartLayer(AnimationClip clip)
        {
            layers.Add(new AnimationLayer() { Clip = clip, Time = 0, Loop = false });
        }

        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public void Update(float time)
        {
            if (animationSource == null)
                return;
            SkinningData skin = animationSource.GetSkinningData();
            UpdateTime(time);
            if (skin != null)
            {
                skin.ToDefault();
                Base?.Apply(skin);
                foreach (var layer in layers)
                    layer.Apply(skin);
            }
        }

        private void UpdateTime(float dt)
        {
            if (Base?.UpdateTime(dt) ?? true) StartClip(DefaultClip, true);
            for (int i = 0; i < layers.Count;)
                if (layers[i].UpdateTime(dt)) layers.RemoveAt(i);
                else i++;
        }
    }
}
