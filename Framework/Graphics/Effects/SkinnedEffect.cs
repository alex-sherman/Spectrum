using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using Spectrum.Framework.Graphics.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Graphics
{
    public class SpectrumSkinnedEffect : SpectrumEffect
    {
        private string[] BoneNames;
        public Matrix[] BoneTransforms { get; protected set; }
        public SpectrumSkinnedEffect() : base(ContentHelper.Load<Effect>("SkinnedEffect")) { }
        protected override bool OnApply()
        {
            if (BoneTransforms != null)
                Parameters["Bones"].SetValue(BoneTransforms);
            return base.OnApply();
        }
        public void SetBoneNames(params string[] boneNames)
        {
            BoneTransforms = new Matrix[boneNames.Count()];
            for (int i = 0; i < boneNames.Count(); i++)
            {
                BoneTransforms[i] = Matrix.Identity;
            }
            BoneNames = boneNames;
        }
        public void UpdateBoneTransforms(SkinningData SkinningData)
        {
            for (int i = 0; i < BoneTransforms.Count(); i++)
            {
                BoneTransforms[i] = SkinningData.Bones[BoneNames[i]].absoluteTransform;
            }
        }
    }
}
