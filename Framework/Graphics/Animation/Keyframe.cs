#region File Description
//-----------------------------------------------------------------------------
// Keyframe.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace Spectrum.Framework.Graphics.Animation
{
    /// <summary>
    /// Describes the position of a single bone at a single point in time.
    /// </summary>
    public class Keyframe<T>
    {
        /// <summary>
        /// Constructs a new keyframe object.
        /// </summary>
        public Keyframe(float time, T transform)
        {
            Time = time;
            Value = transform;
        }


        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private Keyframe()
        {
        }

        /// <summary>
        /// Gets the time offset from the start of the animation to this keyframe.
        /// </summary>
        public float Time { get; private set; }


        /// <summary>
        /// Gets the bone transform for this keyframe.
        /// </summary>
        public T Value { get; private set; }
        public Keyframe<T> Next;
    }
    public static class KeyframeExtensions
    {
        public static T LerpTo<T>(this Keyframe<T> head, float time, T start, Func<T, T, float, T> lerp)
        {
            while (head.Next != null && time > head.Next.Time)
                head = head.Next;
            if (head?.Next != null)
            {
                float w = (time - head.Time) / (head.Next.Time - head.Time);
                return lerp(head.Value, head.Next.Value, w);
            }
            return start;
        }
    }
}
