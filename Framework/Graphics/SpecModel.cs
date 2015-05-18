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
            this.AddRange(meshParts.Values);
            foreach (DrawablePart part in this)
            {
                if (part.effect is CustomSkinnedEffect)
                    ((CustomSkinnedEffect)part.effect).SkinningData = skinningData;
            }
            SkinningData = skinningData;
        }
        public void Update(GameTime gameTime)
        {
            if (AnimationPlayer != null)
            {
                AnimationPlayer.Update(gameTime.ElapsedGameTime);
            }
        }
    }
}
