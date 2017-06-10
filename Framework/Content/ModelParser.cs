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

        public ModelParserCache(string fileName)
        {
            this.FileName = fileName;
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
        public ModelParser()
        {
            Prefix = @"Models\";
        }

        protected override ModelParserCache LoadData(string path, string name)
        {
            string fullPath;
            foreach (var reader in ModelReaders)
            {
                if((fullPath = TryExtensions(path, "." + reader.Key)) != null)
                {
                    return reader.Value.LoadData(fullPath, name);
                }
            }
            throw new FileNotFoundException("The file could not be loaded: ", path);
        }

        public SkinningData GetSkinningData(JObject jobj)
        {
            JToken armature = ((JArray)jobj["nodes"]).FirstOrDefault(node => (string)node["id"] == "Armature");
            if (armature == null) return null;
            Dictionary<string, Bone> bones = new Dictionary<string, Bone>();
            Bone rootBone = JObjToBone(armature, bones);

            SkinningData output = new SkinningData(rootBone, bones);

            foreach (Bone bone in output.Bones.Values)
            {
                bone.inverseBindPose = Matrix.Invert(bone.withParentTransform);
            }
            return output;
        }

        private Bone JObjToBone(JToken rootNode, Dictionary<string, Bone> bones, Bone parent = null)
        {
            Bone rootBone = new Bone((string)rootNode["id"], parent);
            bones[rootBone.id] = rootBone;

            JArray rotation = (JArray)rootNode["rotation"];
            if (rotation != null)
            {
                rootBone.defaultRotation = MatrixHelper.CreateRotation(rotation);
            }

            JArray translation = (JArray)rootNode["translation"];
            if (translation != null)
            {
                rootBone.defaultTranslation *= MatrixHelper.CreateTranslation(translation);
            }

            rootBone.transform = rootBone.defaultRotation * rootBone.defaultTranslation;

            JToken children = rootNode["children"];
            if (children != null)
            {
                foreach (JToken child in children)
                {
                    rootBone.children.Add(JObjToBone(child, bones, rootBone));
                }
            }

            return rootBone;
        }

        protected override SpecModel SafeCopy(ModelParserCache data)
        {
            Dictionary<string, DrawablePart> parts = new Dictionary<string, DrawablePart>();
            foreach (KeyValuePair<string, DrawablePart> part in data.parts)
            {
                parts[part.Key] = part.Value.CreateReference();
            }
            SpecModel model = new SpecModel(data.FileName, parts, data.materials, data.skinningData);
            model.Animations = data.animations;
            return model;
        }
    }
}
