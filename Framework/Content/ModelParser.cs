﻿using Microsoft.Xna.Framework;
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
using System.Text.RegularExpressions;

namespace Spectrum.Framework.Content
{
    public class MaterialData
    {
        public List<MaterialTexture> textures = new List<MaterialTexture>();
        public Color diffuseColor = Color.HotPink;
        public Color specularColor = Color.Black;
    }
    public struct MaterialTexture
    {
        public string Id;
        public string Filename;
        public string Type;
    }
    class ModelParserCache
    {
        public JObject jobj;
        public Dictionary<string, MeshPartData> parts = new Dictionary<string, MeshPartData>();
        public Dictionary<string, MaterialData> materials = new Dictionary<string, MaterialData>();
        public Dictionary<string, AnimationClip> animations = new Dictionary<string, AnimationClip>();
        public string Directory;
        public string FileName;

        public ModelParserCache(string fileName)
        {
            this.FileName = fileName;
        }
    }
    struct MeshPartData
    {
        public VertexBuffer vbuffer;
        public IndexBuffer ibuffer;
        public MeshPartData(VertexBuffer vbuffer, IndexBuffer ibuffer)
        {
            this.vbuffer = vbuffer;
            this.ibuffer = ibuffer;
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
        public ModelParser()
        {
            Prefix = @"Models\";
        }

        protected override ModelParserCache LoadData(string path, string name)
        {
            path = TryExtensions(path, ".g3dj");
            ModelParserCache modelData = new ModelParserCache(name);
            modelData.Directory = Path.GetDirectoryName(path);
            JsonTextReader reader = new JsonTextReader(new StreamReader(path));
            modelData.jobj = JObject.Load(reader);
            if (modelData.jobj["meshes"] == null) { throw new InvalidOperationException("Provided model has no mesh data"); }

            foreach (JObject mesh in modelData.jobj["meshes"])
            {
                List<string> attributes = new List<string>();
                foreach (string attribute in mesh["attributes"])
                {
                    attributes.Add(attribute);
                }

                JArray jsonVertices = (JArray)mesh["vertices"];
                List<SkinnedVertex> vertices = new List<SkinnedVertex>();
                for (int i = 0; i < jsonVertices.Count;)
                {
                    SkinnedVertex vertex = ParseVertex(attributes, jsonVertices, ref i);
                    vertices.Add(vertex);
                }
                VertexBuffer vbuffer = VertexHelper.MakeVertexBuffer(SkinnedVertex.VertexDeclaration, vertices.Count);

                foreach (JObject meshPart in mesh["parts"])
                {
                    List<uint> indices = ((JArray)meshPart["indices"]).ToList().ConvertAll(x => (uint)x);
                    VertexHelper.ComputeTangents(vertices, indices);
                    IndexBuffer ibuffer = VertexHelper.MakeIndexBuffer(indices);
                    modelData.parts[(string)meshPart["id"]] = new MeshPartData(vbuffer, ibuffer);
                }
                vbuffer.SetData(vertices.ToArray());
            }

            modelData.materials = ReadMaterials(modelData.jobj);
            if(modelData.jobj["animations"] != null)
            {
                modelData.animations = AnimationParser.GetAnimations(modelData.jobj);
            }
            return modelData;
        }

        private static SkinnedVertex ParseVertex(List<string> attributes, JArray jsonVertices, ref int i)
        {
            SkinnedVertex vertex = new SkinnedVertex();
            List<BoneWeight> weights = new List<BoneWeight>();
            foreach (string attribute in attributes)
            {
                Match m = Regex.Match(attribute, "^(\\w+?)(\\d*)$");
                string attributeType = attribute.Substring(m.Groups[1].Index, m.Groups[1].Length);
                int attributeIndex = m.Groups[2].Length > 0 ? int.Parse(attribute.Substring(m.Groups[2].Index, m.Groups[2].Length)) : 0;
                switch (attributeType)
                {
                    case "POSITION":
                        vertex.Position = new Vector3((float)jsonVertices[i++], (float)jsonVertices[i++], (float)jsonVertices[i++]);
                        break;
                    case "NORMAL":
                        vertex.Normal = new Vector3((float)jsonVertices[i++], (float)jsonVertices[i++], (float)jsonVertices[i++]);
                        break;
                    case "TEXCOORD":
                        vertex.TextureCoordinate = new Vector2((float)jsonVertices[i++], (float)jsonVertices[i++]);
                        break;
                    case "COLOR":
                        i += 4;
                        break;
                    case "BLENDWEIGHT":
                        int index = (int)jsonVertices[i++];
                        float weight = (float)jsonVertices[i++];
                        weights.Add(new BoneWeight(index, weight));
                        break;
                    default:
                        break;
                }
            }
            weights.Sort((a, b) => b.Weight.CompareTo(a.Weight));
            for (int j = 0; j < 4 && j < weights.Count; j++)
            {
                switch (j)
                {
                    case 0:
                        vertex.BlendIndices.X = weights[j].Index;
                        vertex.Blendweight0.X = weights[j].Weight;
                        break;
                    case 1:
                        vertex.BlendIndices.Y = weights[j].Index;
                        vertex.Blendweight0.Y = weights[j].Weight;
                        break;
                    case 2:
                        vertex.BlendIndices.Z = weights[j].Index;
                        vertex.Blendweight0.Z = weights[j].Weight;
                        break;
                    case 3:
                        vertex.BlendIndices.W = weights[j].Index;
                        vertex.Blendweight0.W = weights[j].Weight;
                        break;
                    default:
                        break;
                }
            }
            return vertex;
        }

        public Dictionary<string, MaterialData> ReadMaterials(JObject jobj)
        {
            Dictionary<string, MaterialData> output = new Dictionary<string, MaterialData>();
            if (jobj["materials"] != null)
            {
                foreach (JObject material in jobj["materials"])
                {
                    MaterialData materialData = new MaterialData();
                    output[(string)material["id"]] = materialData;
                    if (material["emissive"] != null)
                    {
                        JArray diffuseColor = (JArray)material["emissive"];
                        materialData.diffuseColor = new Color((float)Math.Pow((float)diffuseColor[0], .45), (float)Math.Pow((float)diffuseColor[1], .45), (float)Math.Pow((float)diffuseColor[2], .45));
                    }
                    if (material["textures"] != null)
                    {
                        foreach (JObject texture in material["textures"])
                        {
                            MaterialTexture materialTexture = new MaterialTexture();
                            materialTexture.Id = (string)texture["id"];
                            materialTexture.Filename = (string)texture["filename"];
                            materialTexture.Type = (string)texture["type"];
                            materialData.textures.Add(materialTexture);
                        }
                    }
                }
            }
            return output;
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

        private void parseNode(JToken node, ModelParserCache data, Dictionary<string, DrawablePart> parts)
        {
            foreach (JToken nodePart in node["parts"])
            {
                List<string> meshpartids = new List<string>();
                meshpartids.Add((string)nodePart["meshpartid"]);
                DrawablePart part = parts[(string)nodePart["meshpartid"]];
                part.effect = new SpectrumEffect();
                if (nodePart["bones"] != null)
                {
                    part.effect.SetBoneNames((nodePart["bones"]).ToList().ConvertAll(x => (string)x["node"]).ToArray());
                    part.effect.SetTechnique("Skinned");
                }

                if (nodePart["materialid"] != null && data.materials.ContainsKey((string)nodePart["materialid"]))
                {
                    MaterialData materialData = data.materials[(string)nodePart["materialid"]];
                    part.effect.DiffuseColor = materialData.diffuseColor;
                    List<MaterialTexture> materialTextures = materialData.textures;
                    foreach (MaterialTexture texture in materialTextures)
                    {
                        if (texture.Type == "NONE" || texture.Type == "DIFFUSE")
                        {
                            part.effect.Texture = ContentHelper.Load<Texture2D>(data.Directory + "\\" + texture.Filename, false) ?? ContentHelper.Blank;
                        }
                        if (texture.Type == "NORMAL")
                        {
                            part.effect.NormalMap = ContentHelper.Load<Texture2D>(data.Directory + "\\" + texture.Filename, false) ?? ContentHelper.Blank;
                        }
                        if (texture.Type == "TRANSPARENCY")
                        {
                            part.effect.Transparency = ContentHelper.Load<Texture2D>(data.Directory + "\\" + texture.Filename, false) ?? ContentHelper.Blank;
                        }
                    }
                }
            }
            if (node["children"] != null)
            {
                foreach (var child in node["children"])
                {
                    parseNode(child, data, parts);
                }
            }
        }

        protected override SpecModel SafeCopy(ModelParserCache data)
        {
            Dictionary<string, DrawablePart> parts = new Dictionary<string, DrawablePart>();
            foreach (KeyValuePair<string, MeshPartData> part in data.parts)
            {
                parts[part.Key] = new DrawablePart(part.Value.vbuffer, part.Value.ibuffer);
                parts[part.Key].transform = Matrix.CreateFromYawPitchRoll((float)Math.PI, 0, 0);
            }
            foreach (var node in ((JArray)data.jobj["nodes"]).Where(node => node["parts"] != null))
            {
                parseNode(node, data, parts);
            }
            SpecModel model = new SpecModel(data.FileName, parts, GetSkinningData(data.jobj));
            model.AnimationPlayer = new AnimationPlayer(data.animations);
            return model;
        }
    }
}
