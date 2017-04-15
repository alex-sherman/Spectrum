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
        TimeSpan currentTimeValue;
        int currentKeyFrame;
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
            currentKeyFrame = 0;
            currentClipValue = clip;

            if (clip == null) return;

            currentTimeValue = TimeSpan.Zero;
            animationSource?.GetSkinningData()?.ToDefault();
            Update(TimeSpan.Zero);
        }


        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public void Update(TimeSpan time)
        {
            SkinningData SkinningData = animationSource?.GetSkinningData();
            if (SkinningData != null && currentClipValue != null)
            {
                UpdateTime(time, SkinningData);
                List<Keyframe> Keyframes = currentClipValue.Keyframes;
                for (; currentKeyFrame < Keyframes.Count && Keyframes[currentKeyFrame].Time <= currentTimeValue; currentKeyFrame++)
                {
                    Bone currentBone = SkinningData.Bones[Keyframes[currentKeyFrame].Bone];
                    Matrix rotation = Keyframes[currentKeyFrame].Rotation ?? currentBone.defaultRotation;
                    Matrix translation = Keyframes[currentKeyFrame].Translation ?? currentBone.defaultTranslation;
                    currentBone.transform = rotation * translation;
                }
            }
        }

        private void UpdateTime(TimeSpan time, SkinningData SkinningData)
        {
            if (currentClipValue.Duration == TimeSpan.Zero) { return; }
            currentTimeValue += time;
            if (currentTimeValue > currentClipValue.Duration)
            {
                currentTimeValue -= currentClipValue.Duration;
                currentKeyFrame = 0;
            }
        }
    }
}
