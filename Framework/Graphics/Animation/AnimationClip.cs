#region File Description
//-----------------------------------------------------------------------------
// AnimationClip.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Spectrum.Framework.Graphics.Animation
{
    /// <summary>
    /// An animation clip is the runtime equivalent of the
    /// Microsoft.Xna.Framework.Content.Pipeline.Graphics.AnimationContent type.
    /// It holds all the keyframes needed to describe a single animation.
    /// </summary>
    public class AnimationClip
    {
        /// <summary>
        /// Constructs a new animation clip object.
        /// </summary>
        public AnimationClip(string name, float duration, List<Keyframe> keyframes)
        {
            Name = name;
            Duration = duration;
            Keyframes = keyframes.GroupBy((kf) => kf.Bone).ToDictionary((g) => g.Key, (g) => g.OrderBy((kf) => kf.Time).ToList());
        }


        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private AnimationClip()
        {
        }

        public string Name { get; private set; }

        /// <summary>
        /// Gets the total length of the animation.
        /// </summary>
        public float Duration { get; private set; }


        /// <summary>
        /// Gets a combined list containing all the keyframes for all bones,
        /// sorted by time.
        /// </summary>
        public Dictionary<string, List<Keyframe>> Keyframes { get; private set; }
    }
}
