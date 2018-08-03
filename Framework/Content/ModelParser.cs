using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Content.ModelParsing;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Graphics.Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Framework.Content
{
    interface IModelReader
    {
        ModelParserCache LoadData(string path, string name);
    }

    class ModelParserCache
    {
        public SkinningData skinningData;
        public Dictionary<string, DrawablePart> parts = new Dictionary<string, DrawablePart>();
        public Dictionary<string, MaterialData> materials = new Dictionary<string, MaterialData>();
        public AnimationData animations = new AnimationData();
        public List<string> vertexAttributes = new List<string>();
        public string Directory;
        public string FileName;
        public string Name;

        public ModelParserCache(string name, string path)
        {
            Directory = Path.GetDirectoryName(path);
            FileName = path;
            Name = name;
        }
    }
    struct BoneWeight
    {
        public int Index;
        public float Weight;
        public BoneWeight(int index, float weight)
        {
            Index = index;
            Weight = weight;
        }
    }
    class ModelParser : CachedContentParser<ModelParserCache, SpecModel>
    {
        public static Dictionary<string, IModelReader> ModelReaders = new Dictionary<string, IModelReader>()
        {
            {"g3dj", new G3DJReader()},
            {"obj", new OBJReader()}
        };
        public ModelParser() : base(ModelReaders.Keys.ToArray())
        {
            Prefix = "Models";
        }

        protected override ModelParserCache LoadData(string path, string name)
        {
            var extension = Path.GetExtension(path).Substring(1);
            return ModelReaders[extension].LoadData(path, name);
        }

        protected override SpecModel SafeCopy(ModelParserCache data)
        {
            Dictionary<string, DrawablePart> parts = new Dictionary<string, DrawablePart>();
            foreach (KeyValuePair<string, DrawablePart> part in data.parts)
            {
                parts[part.Key] = part.Value.CreateReference();
            }
            SpecModel model = new SpecModel(data.Name, data.FileName, parts, data.materials, data.skinningData?.Clone())
            {
                Animations = data.animations
            };
            return model;
        }
    }
}
