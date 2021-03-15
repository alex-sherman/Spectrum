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
    /// <summary>
    /// The animation player is in charge of decoding bone position
    /// matrices from an animation clip.
    /// </summary>
    public class AnimationPlayer
    {
        #region Fields
        // Information about the currently playing animation clip.
        AnimationClip clip;
        bool loop;
        float currentTimeValue;
        public string DefaultClip = "Idle";
        private Dictionary<string, Keyframe> translations = new Dictionary<string, Keyframe>();
        private Dictionary<string, Keyframe> rotations = new Dictionary<string, Keyframe>();
        IAnimationSource animationSource;

        #endregion

        public AnimationPlayer(IAnimationSource animationSource)
        {
            this.animationSource = animationSource;
        }

        public string CurrentClip() => clip?.Name;

        public void StartClip(string animation, bool loop = false)
        {
            StartClip(animationSource?.GetAnimation(animation), loop);
        }

        /// <summary>
        /// Starts decoding the specified animation clip.
        /// </summary>
        public void StartClip(AnimationClip clip, bool loop)
        {
            this.clip = clip;
            this.loop = loop;

            if (clip == null) return;

            currentTimeValue = 0;
            animationSource?.GetSkinningData()?.ToDefault();

            translations = this.clip.Keyframes.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.First());
            rotations = this.clip.Keyframes.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.First());
            Update(0);
        }


        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public void Update(float time)
        {
            if (animationSource == null)
                return;
            SkinningData SkinningData = animationSource.GetSkinningData();
            UpdateTime(time);
            if (SkinningData != null && clip != null)
            {
                foreach (var kvp in clip.Keyframes)
                {
                    if (!SkinningData.Bones.ContainsKey(kvp.Key)) continue;
                    Bone currentBone = SkinningData.Bones[kvp.Key];

                    Matrix translation = currentBone.DefaultTranslation;
                    if (translations.ContainsKey(kvp.Key))
                    {
                        var translation1 = translations[kvp.Key];
                        while (translation1.NextTranslation != null && currentTimeValue > translation1.NextTranslation.Time)
                        {
                            translation1 = translation1.NextTranslation;
                            translations[kvp.Key] = translation1;
                        }
                        var translation2 = translations[kvp.Key].NextTranslation;
                        if (translation1 != null && translation2 != null)
                        {
                            float w = (currentTimeValue - translation1.Time) / (translation2.Time - translation1.Time);
                            translation = translation1.Translation.Value * (1 - w) + translation2.Translation.Value * w;
                        }
                    }
                    Matrix rotation = currentBone.DefaultRotation;
                    if (rotations.ContainsKey(kvp.Key))
                    {
                        var rotation1 = rotations[kvp.Key];
                        while (rotation1.NextRotation != null && currentTimeValue > rotation1.NextRotation.Time)
                        {
                            rotation1 = rotation1.NextRotation;
                            rotations[kvp.Key] = rotation1;
                        }
                        var rotation2 = rotations[kvp.Key].NextRotation;
                        if (rotation1 != null && rotation2 != null)
                        {
                            float w = (currentTimeValue - rotation1.Time) / (rotation2.Time - rotation1.Time);
                            rotation = rotation1.Rotation.Value * (1 - w) + rotation2.Rotation.Value * w;
                        }
                    }
                    currentBone.Transform = rotation * translation;
                }
            }
        }

        private void UpdateTime(float time)
        {
            if (clip == null || clip.Duration == 0) { return; }
            currentTimeValue += time;
            if (currentTimeValue > clip.Duration)
            {
                if (loop)
                {
                    currentTimeValue -= clip.Duration;
                    if (clip != null)
                    {
                        translations = clip.Keyframes.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.First());
                        rotations = clip.Keyframes.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.First());
                    }
                }
                else StartClip(DefaultClip, true);
            }
        }
    }
}
