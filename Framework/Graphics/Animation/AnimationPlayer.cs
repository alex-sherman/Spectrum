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
        // Backlink to the bind pose and skeleton hierarchy data.
        Dictionary<string, AnimationClip> AnimationClips;
        private bool playOnce = false;
        public string DefaultClip = "Default";

        #endregion


        public string[] AnimationNames
        {
            get
            {
                string[] output = new string[AnimationClips.Count];
                AnimationClips.Keys.CopyTo(output, 0);
                return output;
            }
        }

        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        public AnimationPlayer(Dictionary<string, AnimationClip> Animations)
        {
            this.AnimationClips = Animations;
        }


        /// <summary>
        /// Starts decoding the specified animation clip.
        /// </summary>
        public void StartClip(string animation, SkinningData SkinningData, bool playOnce = false)
        {
            currentKeyFrame = 0;
            this.playOnce = playOnce;
            if (!AnimationClips.ContainsKey(animation)) return;
            AnimationClip clip = AnimationClips[animation];

            currentClipValue = clip;
            currentTimeValue = TimeSpan.Zero;

            SkinningData.ToDefault();

            Update(TimeSpan.Zero, SkinningData);
        }


        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public void Update(TimeSpan time, SkinningData SkinningData)
        {
            if (currentClipValue != null)
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
                if (playOnce)
                {
                    if (AnimationClips.ContainsKey(DefaultClip))
                        StartClip(DefaultClip, SkinningData);
                    else
                        currentClipValue = null;
                    return;
                }
            }
        }

        public AnimationClip CurrentClip
        {
            get { return currentClipValue; }
        }
    }
}
