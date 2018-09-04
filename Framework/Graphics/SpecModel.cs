using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Content;
using Spectrum.Framework.Graphics.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Spectrum.Framework.Graphics
{
    public class SpecModel
    {
        public string Path { get; private set; }
        public string Name { get; private set; }
        public AnimationData Animations { get; set; }
        public SkinningData SkinningData { get; protected set; }
        private int _partIndex = 0;

        public Dictionary<string, DrawablePart> MeshParts { get; private set; }
        public Dictionary<string, MaterialData> Materials { get; private set; }
        public SpecModel()
        {
            Path = null;
            MeshParts = new Dictionary<string, DrawablePart>();
            Materials = new Dictionary<string, MaterialData>();
            SkinningData = null;
        }
        public SpecModel(string name, string path,
            Dictionary<string, DrawablePart> meshParts, Dictionary<string, MaterialData> materials, SkinningData skinningData)
        {
            Name = name;
            Path = path;
            MeshParts = meshParts;
            Materials = materials;
            SkinningData = skinningData;
        }
        /// <summary>
        /// Be careful using this because the GameObject's RenderTasks will not get updated
        /// </summary>
        /// <param name="part"></param>
        public void Add(DrawablePart part)
        {
            MeshParts["part_" + _partIndex] = part;
            _partIndex++;
        }
        public void Update(float dt)
        {
            foreach (var part in MeshParts.Values)
            {
                if (SkinningData != null)
                    (part.effect as SpectrumSkinnedEffect)?.UpdateBoneTransforms(SkinningData);
            }
        }

        public IEnumerator<DrawablePart> GetEnumerator()
        {
            return MeshParts.Values.GetEnumerator();
        }
    }
}
