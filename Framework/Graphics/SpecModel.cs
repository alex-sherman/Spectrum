using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Content;
using Spectrum.Framework.Graphics.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Graphics
{
    public class SpecModel : List<DrawablePart>
    {
        public string Path { get; private set; }
        public AnimationData Animations { get; set; }
        public SkinningData SkinningData { get; protected set; }
        public Dictionary<string, DrawablePart> MeshParts { get; private set; }
        public SpecModel(string path, Dictionary<string, DrawablePart> meshParts, SkinningData skinningData)
        {
            Path = path;
            MeshParts = meshParts;
            AddRange(meshParts.Values);
            SkinningData = skinningData;
        }
        public Texture2D Texture
        {
            set
            {
                foreach (var part in this)
                {
                    part.effect.Texture = value;
                }
            }
        }
        public void Update(GameTime gameTime)
        {
            foreach (var part in this)
            {
                if (SkinningData != null)
                    part.effect.UpdateBoneTransforms(SkinningData);
            }
        }
    }
}
