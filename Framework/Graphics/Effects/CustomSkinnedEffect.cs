using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Graphics.Animation;
using Spectrum.Framework.Content;

namespace Spectrum.Framework.Graphics
{
    /// <summary>
    /// Custom effect for rendering skinned character models.
    /// </summary>
    public class CustomSkinnedEffect : SpectrumEffect
    {
        public const int MaxBones = 72;
        
        /// <summary>
        /// Sets an array of skinning bone transform matrices.
        /// </summary>
        private Matrix[] parBones { set { Parameters["Bones"].SetValue(value); } }
        public string[] Bones;
        public SkinningData SkinningData = null;
        Matrix[] boneTransforms;

        /// <summary>
        /// Creates a new CustomSkinnedEffect with default parameter settings.
        /// </summary>
        public CustomSkinnedEffect(string[] Bones)
            : base(ContentHelper.Load<Effect>("CustomSkinnedEffect"))
        {
            Parameters["world"].SetValue(Matrix.Identity);

            this.boneTransforms = new Matrix[Bones.Count()];
            for (int i = 0; i < Bones.Count(); i++)
            {
                boneTransforms[i] = Matrix.Identity;
            }
            parBones = boneTransforms;
            this.Bones = Bones;
        }
        public void UpdateBoneTransforms()
        {
            if (SkinningData != null)
            {
                for (int i = 0; i < boneTransforms.Count(); i++)
                {
                    boneTransforms[i] = SkinningData.Bones[Bones[i]].absoluteTransform;
                }
                parBones = boneTransforms;
            }
        }
    }
}
