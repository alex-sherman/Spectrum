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
        private Microsoft.Xna.Framework.Matrix[] BoneTransforms;
        public SpectrumSkinnedEffect() : base(ContentHelper.Load<Effect>("SkinnedEffect")) { }
        private SpectrumSkinnedEffect(SpectrumSkinnedEffect clone) : base(clone) { }
        public override bool CanInstance
        {
            get
            {
                return false;
            }
        }
        protected override void OnApply()
        {
            if (BoneTransforms != null)
                Parameters["Bones"].SetValue(BoneTransforms);
            base.OnApply();
        }
        public void SetBoneNames(params string[] boneNames)
        {
            BoneTransforms = new Microsoft.Xna.Framework.Matrix[boneNames.Count()];
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
        public override Effect Clone()
        {
            var result = new SpectrumSkinnedEffect(this);
            result.SetBoneNames(BoneNames);
            return result;
        }
    }
}
