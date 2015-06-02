using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Graphics.Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    public struct MaterialTexture
    {
        public string Id;
        public string Filename;
        public string Type;
    }
    struct ModelParserCache
    {
        public JObject jobj;
        public VertexBuffer vBuffer;
        public Dictionary<string, IndexBuffer> indices;
        public Dictionary<string, List<MaterialTexture>> materials;
        public string Directory;
    }
    class ModelParser : CachedContentParser<ModelParserCache, SpecModel>
    {
        public ModelParser()
        {
            Prefix = @"Models\";
            Suffix = ".g3dj";
        }
        protected override ModelParserCache LoadData(string path)
        {
            
            ModelParserCache modelData = new ModelParserCache();
            modelData.Directory = Path.GetDirectoryName(path);
            JsonTextReader reader = new JsonTextReader(new StreamReader(path));
            modelData.jobj = JObject.Load(reader);
            if (modelData.jobj["meshes"] == null) { throw new InvalidOperationException("Provided model has no mesh data"); }


            JObject mesh = (JObject)modelData.jobj["meshes"][0];
            List<string> attributes = new List<string>();
            foreach (string attribute in mesh["attributes"])
            {
                attributes.Add(attribute);
            }

            JArray jsonVertices = (JArray)mesh["vertices"];
            List<SkinnedVertex> vertices = new List<SkinnedVertex>();
            for (int i = 0; i < jsonVertices.Count; )
            {
                SkinnedVertex vertex = new SkinnedVertex();
                foreach (string attribute in attributes)
                {
                    switch (attribute)
                    {
                        case "POSITION":
                            vertex.Position = new Vector3((float)jsonVertices[i++], (float)jsonVertices[i++], (float)jsonVertices[i++]);
                            break;
                        case "NORMAL":
                            vertex.Normal = new Vector3((float)jsonVertices[i++], (float)jsonVertices[i++], (float)jsonVertices[i++]);
                            break;
                        case "TEXCOORD0":
                            vertex.TextureCoordinate = new Vector2((float)jsonVertices[i++], (float)jsonVertices[i++]);
                            break;
                        case "BLENDWEIGHT0":
                            vertex.BlendIndices.X = (float)jsonVertices[i++];
                            vertex.Blendweight0.X = (float)jsonVertices[i++];
                            break;
                        case "BLENDWEIGHT1":
                            vertex.BlendIndices.Y = (float)jsonVertices[i++];
                            vertex.Blendweight0.Y = (float)jsonVertices[i++];
                            break;
                        case "BLENDWEIGHT2":
                            vertex.BlendIndices.Z = (float)jsonVertices[i++];
                            vertex.Blendweight0.Z = (float)jsonVertices[i++];
                            break;
                        case "BLENDWEIGHT3":
                            vertex.BlendIndices.W = (float)jsonVertices[i++];
                            vertex.Blendweight0.W = (float)jsonVertices[i++];
                            break;
                        default:
                            break;
                    }
                }
                vertices.Add(vertex);
            }
            modelData.vBuffer = VertexHelper.MakeVertexBuffer(vertices);

            modelData.indices = new Dictionary<string, IndexBuffer>();
            foreach (JObject meshPart in mesh["parts"])
            {
                List<uint> indices = ((JArray)meshPart["indices"]).ToList().ConvertAll(x => (uint)x);
                IndexBuffer iBuffer = VertexHelper.MakeIndexBuffer(indices);
                modelData.indices[(string)meshPart["id"]] = iBuffer;
            }

            modelData.materials = ReadMaterials(modelData.jobj);
            return modelData;
        }

        public Dictionary<string, List<MaterialTexture>> ReadMaterials(JObject jobj)
        {
            Dictionary<string, List<MaterialTexture>> output = new Dictionary<string,List<MaterialTexture>>();
            if (jobj["materials"] != null)
            {
                foreach (JObject material in jobj["materials"])
                {
                    List<MaterialTexture> textures = new List<MaterialTexture>();
                    output[(string)material["id"]] = textures;
                    if (material["textures"] != null)
                    {
                        foreach (JObject texture in material["textures"])
                        {
                            MaterialTexture materialTexture = new MaterialTexture();
                            materialTexture.Id = (string)texture["id"];
                            materialTexture.Filename = (string)texture["filename"];
                            materialTexture.Type = (string)texture["type"];
                            textures.Add(materialTexture);
                        }
                    }
                }
            }
            return output;
        }

        public SkinningData GetSkinningData(JObject jobj)
        {
            Dictionary<string, Bone> bones = new Dictionary<string, Bone>();
            if (((JArray)jobj["nodes"]).Count < 2)
                return null;
            Bone rootBone = JObjToBone(((JArray)jobj["nodes"])[1], bones);

            SkinningData output = new SkinningData(rootBone, bones);

            foreach (Bone bone in output.Bones.Values)
            {
                bone.inverseBindPose = Matrix.Invert(bone.withParentTransform);
            }
            output.Root.defaultRotation = Matrix.CreateFromYawPitchRoll((float)Math.PI, -(float)Math.PI / 2, 0);
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
            foreach (KeyValuePair<string, IndexBuffer> partIbuffer in data.indices)
            {
                parts[partIbuffer.Key] = new DrawablePart(data.vBuffer, partIbuffer.Value);
            }
            foreach (JToken nodePart in ((JArray)data.jobj["nodes"])[0]["parts"])
            {
                DrawablePart part = parts[(string)nodePart["meshpartid"]];
                if (nodePart["bones"] != null)
                    part.effect = new CustomSkinnedEffect((nodePart["bones"]).ToList().ConvertAll(x => (string)x["node"]).ToArray());
                else
                    part.effect = new SpectrumEffect();

                if(nodePart["materialid"] != null)
                {
                    List<MaterialTexture> materialTextures = data.materials[(string)nodePart["materialid"]];
                    foreach (MaterialTexture texture in materialTextures)
                    {
                        if(texture.Type == "NONE")
                        {
                            part.effect.Texture = ContentHelper.Load<Texture2D>(data.Directory + "\\" + texture.Filename, false);
                        }
                    }
                }
            }
            return new SpecModel(parts, GetSkinningData(data.jobj));
        }
    }
}
