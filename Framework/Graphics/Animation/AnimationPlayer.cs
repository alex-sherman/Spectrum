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
        AnimationClip currentClipValue;
        float currentTimeValue;
        public string DefaultClip = "Default";
        private Dictionary<string, Keyframe> translations = new Dictionary<string, Keyframe>();
        private Dictionary<string, Keyframe> rotations = new Dictionary<string, Keyframe>();
        IAnimationSource animationSource;

        #endregion

        public AnimationPlayer(IAnimationSource animationSource)
        {
            this.animationSource = animationSource;
        }

        public string CurrentClip()
        {
            return currentClipValue?.Name;
        }

        public void StartClip(string animation)
        {
            StartClip(animationSource?.GetAnimation(animation));
        }

        /// <summary>
        /// Starts decoding the specified animation clip.
        /// </summary>
        public void StartClip(AnimationClip clip)
        {
            currentClipValue = clip;

            if (clip == null) return;

            currentTimeValue = 0;
            animationSource?.GetSkinningData()?.ToDefault();

            translations = currentClipValue.Keyframes.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.First());
            rotations = currentClipValue.Keyframes.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.First());
            Update(0);
        }


        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public void Update(float time)
        {
            SkinningData SkinningData = animationSource?.GetSkinningData();
            if (SkinningData != null && currentClipValue != null)
            {
                UpdateTime(time, SkinningData);
                foreach (var kvp in currentClipValue.Keyframes)
                {
                    if (!SkinningData.Bones.ContainsKey(kvp.Key)) continue;
                    Bone currentBone = SkinningData.Bones[kvp.Key];

                    Matrix translation = currentBone.defaultTranslation;
                    if (translations.ContainsKey(kvp.Key)) {
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
                            translation = Matrix.Add(Matrix.Multiply(translation1.Translation.Value, 1 - w), Matrix.Multiply(translation2.Translation.Value, w));
                        }
                    }
                    Matrix rotation = currentBone.defaultRotation;
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
                            rotation = Matrix.Add(Matrix.Multiply(rotation1.Rotation.Value, 1 - w), Matrix.Multiply(rotation2.Rotation.Value, w));
                        }
                    }
                    currentBone.transform = rotation * translation;
                }
            }
        }

        private void UpdateTime(float time, SkinningData SkinningData)
        {
            if (currentClipValue.Duration == 0) { return; }
            currentTimeValue += time;
            if (currentTimeValue > currentClipValue.Duration)
            {
                currentTimeValue -= currentClipValue.Duration;
                if (currentClipValue != null)
                {
                    translations = currentClipValue.Keyframes.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.First());
                    rotations = currentClipValue.Keyframes.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.First());
                }
            }
        }
    }
}
