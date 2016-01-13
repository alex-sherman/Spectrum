using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Graphics.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Graphics
{
    public class SpecModel : List<DrawablePart>
    {
        public AnimationPlayer AnimationPlayer { get; set; }
        public SkinningData SkinningData { get; protected set; }
        public Dictionary<string, DrawablePart> MeshParts { get; private set; }
        public string CurrentAnimation
        {
            get
            {
                if (AnimationPlayer == null) { return ""; }
                if (AnimationPlayer.CurrentClip == null) { return ""; }
                return AnimationPlayer.CurrentClip.Name;
            }
            set
            {
                if (AnimationPlayer != null && AnimationPlayer.AnimationNames.Contains(value)) { AnimationPlayer.StartClip(value); }
            }
        }
        public SpecModel(Dictionary<string, DrawablePart> meshParts, SkinningData skinningData)
        {
            MeshParts = meshParts;
            this.AddRange(meshParts.Values);
            foreach (DrawablePart part in this)
            {
                if (part.effect is CustomSkinnedEffect)
                    ((CustomSkinnedEffect)part.effect).SkinningData = skinningData;
            }
            SkinningData = skinningData;
        }
        public Texture2D Texture
        {
            set
            {
                foreach (var part in MeshParts.Values)
                {
                    part.effect.Texture = value;
                }
            }
        }
        public void Update(GameTime gameTime)
        {
            if (AnimationPlayer != null)
            {
                AnimationPlayer.Update(gameTime.ElapsedGameTime);
            }
            foreach (var part in this)
            {
                if (part.effect is CustomSkinnedEffect)
                    ((CustomSkinnedEffect)part.effect).UpdateBoneTransforms();
            }
        }
    }
}
