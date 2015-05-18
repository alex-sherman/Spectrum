#region File Description
//-----------------------------------------------------------------------------
// SkinningData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Spectrum.Framework.Graphics.Animation;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
#endregion

namespace Spectrum.Framework.Graphics.Animation
{
    /// <summary>
    /// Combines all the data needed to render and animate a skinned object.
    /// This is typically stored in the Tag property of the Model being animated.
    /// </summary>
    public class SkinningData
    {
        /// <summary>
        /// Constructs a new skinning data object.
        /// </summary>
        public SkinningData(Bone root, Dictionary<string, Bone> bones)
        {
            Bones = bones;
            Root = root;
        }

        public void ToDefault()
        {
            foreach (Bone bone in Bones.Values)
            {
                bone.transform = bone.defaultRotation * bone.defaultTranslation;
            }
        }

        public Bone Root { get; private set; }

        public Dictionary<string, Bone> Bones { get; private set; }
    }
}
