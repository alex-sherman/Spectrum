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
                    var translation1 = kvp.Value.FindLast((kf) => kf.Time <= currentTimeValue && kf.Translation.HasValue);
                    var translation2 = kvp.Value.Find((kf) => kf.Time > currentTimeValue && kf.Translation.HasValue);

                    Matrix translation = currentBone.defaultTranslation;
                    if (translation1 != null && translation2 != null)
                    {
                        float w = (currentTimeValue - translation1.Time) / (translation2.Time - translation1.Time);
                        translation = Matrix.Add(Matrix.Multiply(translation1.Translation.Value, 1 - w), Matrix.Multiply(translation2.Translation.Value, w));
                    }
                    var rotation1 = kvp.Value.FindLast((kf) => kf.Time <= currentTimeValue && kf.Rotation.HasValue);
                    var rotation2 = kvp.Value.Find((kf) => kf.Time > currentTimeValue && kf.Rotation.HasValue);
                    Matrix rotation = currentBone.defaultRotation;
                    if (rotation1 != null && rotation2 != null)
                    {
                        float w = (currentTimeValue - rotation1.Time) / (rotation2.Time - rotation1.Time);
                        rotation = Matrix.Add(Matrix.Multiply(rotation1.Rotation.Value, 1 - w), Matrix.Multiply(rotation2.Rotation.Value, w));
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
            }
        }
    }
}
