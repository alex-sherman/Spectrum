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
using System.Threading.Tasks;

namespace Spectrum.Framework.Content.ModelParsing
{
    static class G3DJReader
    {
        public static ModelParserCache LoadModel(string path, string name)
        {
            ModelParserCache modelData = new ModelParserCache(name, path);
            JObject jobj;
            using (JsonTextReader reader = new JsonTextReader(new StreamReader(path)))
                jobj = JObject.Load(reader);
            if (jobj["meshes"] == null) { throw new InvalidOperationException("Provided model has no mesh data"); }

            foreach (JObject mesh in jobj["meshes"])
            {
                modelData.vertexAttributes = new List<string>();
                foreach (string attribute in mesh["attributes"])
                {
                    modelData.vertexAttributes.Add(attribute);
                }

                JArray jsonVertices = (JArray)mesh["vertices"];
                List<SkinnedVertex> vertices = new List<SkinnedVertex>();
                for (int i = 0; i < jsonVertices.Count;)
                {
                    SkinnedVertex vertex = ParseVertex(modelData.vertexAttributes, jsonVertices, ref i);
                    vertices.Add(vertex);
                }

                foreach (JObject meshPart in mesh["parts"])
                {
                    List<ushort> indices = ((JArray)meshPart["indices"]).ToList().ConvertAll(x => (ushort)x);
                    VertexHelper.ComputeTangents(vertices, indices);
                    IndexBuffer ibuffer = VertexHelper.MakeIndexBuffer(indices);
                    var part = DrawablePart.From(vertices, indices);
                    modelData.parts[(string)meshPart["id"]] = part;
                }
            }

            modelData.materials = ReadMaterials(modelData.Directory, jobj);
            if (jobj["animations"] != null)
            {
                modelData.animations = LoadAnimation(jobj);
            }
            foreach (var node in ((JArray)jobj["nodes"]).Where(node => node["parts"] != null))
            {
                parseNode(node, modelData, modelData.parts);
            }

            modelData.skinningData = GetSkinningData(jobj);

            return modelData;
        }

        private static void parseNode(JToken node, ModelParserCache data, Dictionary<string, DrawablePart> parts)
        {
            foreach (JToken nodePart in node["parts"])
            {
                List<string> meshpartids = new List<string>();
                meshpartids.Add((string)nodePart["meshpartid"]);
                DrawablePart part = parts[(string)nodePart["meshpartid"]];
                SpectrumEffect effect = new SpectrumEffect();
                if (!data.vertexAttributes.Contains("NORMAL"))
                    effect.LightingEnabled = false;
                if (nodePart["bones"] != null)
                {
                    var skinned = new SpectrumSkinnedEffect(); effect = skinned;
                    skinned.SetBoneNames((nodePart["bones"]).ToList().ConvertAll(x => (string)x["node"]).ToArray());
                }
                part.effect = effect;
                MaterialData materialData = MaterialData.Missing;
                if (nodePart["materialid"] != null && data.materials.ContainsKey((string)nodePart["materialid"]))
                    materialData = data.materials[(string)nodePart["materialid"]];

                part.material = materialData;
            }
            if (node["children"] != null)
            {
                foreach (var child in node["children"])
                {
                    parseNode(child, data, parts);
                }
            }
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
            // TODO: Does this need to be sorted?
            weights.Sort((a, b) => b.Weight.CompareTo(a.Weight));
            // TODO: Could be refactored out of a for loop
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

        public static Dictionary<string, MaterialData> ReadMaterials(string directory, JObject jobj)
        {
            Dictionary<string, MaterialData> output = new Dictionary<string, MaterialData>();
            if (jobj["materials"] != null)
            {
                foreach (JObject material in jobj["materials"])
                {
                    MaterialData materialData = new MaterialData();
                    output[(string)material["id"]] = materialData;
                    if (material["diffuse"] != null)
                    {
                        JArray diffuseColor = (JArray)material["diffuse"];
                        materialData.DiffuseColor = new Color((float)Math.Pow((float)diffuseColor[0], .45), (float)Math.Pow((float)diffuseColor[1], .45), (float)Math.Pow((float)diffuseColor[2], .45));
                    }
                    if (material["textures"] != null)
                    {
                        foreach (JObject texture in material["textures"])
                        {
                            LoadTexture(materialData, directory + "\\" + (string)texture["filename"], (string)texture["type"]);
                        }
                    }
                }
            }
            return output;
        }

        private static void LoadTexture(MaterialData material, string path, string type)
        {
            if (type == "NONE" || type == "DIFFUSE")
            {
                material.DiffuseTexture = ContentHelper.Load<Texture2D>(path, false) ?? ContentHelper.Blank;
            }
            if (type == "NORMAL")
            {
                material.DiffuseTexture = ContentHelper.Load<Texture2D>(path, false) ?? ContentHelper.Blank;
            }
            if (type == "TRANSPARENCY")
            {
                material.DiffuseTexture = ContentHelper.Load<Texture2D>(path, false) ?? ContentHelper.Blank;
            }
        }

        public static SkinningData GetSkinningData(JObject jobj)
        {
            JToken armature = ((JArray)jobj["nodes"]).FirstOrDefault(node => (string)node["id"] == "Armature");
            if (armature == null) return null;
            Dictionary<string, Bone> bones = new Dictionary<string, Bone>();
            Bone rootBone = JObjToBone(armature, bones);

            SkinningData output = new SkinningData(rootBone, bones);

            foreach (Bone bone in output.Bones.Values)
            {
                bone.BindPose = bone.WithParentTransform;
                bone.InverseBindPose = bone.WithParentTransform.Invert();
            }
            return output;
        }

        private static Bone JObjToBone(JToken rootNode, Dictionary<string, Bone> bones, Bone parent = null)
        {
            Bone rootBone = new Bone((string)rootNode["id"], parent);
            bones[rootBone.Id] = rootBone;

            JArray rotation = (JArray)rootNode["rotation"];
            if (rotation != null)
                rootBone.DefaultRotation = rotation.ToQuaternion();

            JArray translation = (JArray)rootNode["translation"];
            if (translation != null)
                rootBone.DefaultTranslation += translation.ToVector3();

            rootBone.Rotation = rootBone.DefaultRotation;
            rootBone.Translation = rootBone.DefaultTranslation;

            JToken children = rootNode["children"];
            if (children != null)
            {
                foreach (JToken child in children)
                {
                    rootBone.Children.Add(JObjToBone(child, bones, rootBone));
                }
            }

            return rootBone;
        }

        public static AnimationData LoadAnimation(string path, string name)
        {
            JsonTextReader reader = new JsonTextReader(new StreamReader(path));
            JObject jobj = JObject.Load(reader);
            return LoadAnimation(jobj);
        }
        private static Dictionary<string, Keyframe<T>> SortKeyframes<T>(DefaultDict<string, List<Keyframe<T>>> keyframeSet)
        {
            foreach (var keyframes in keyframeSet.Values)
                keyframes.Sort((x, y) => x.Time.CompareTo(y.Time));
            foreach (var keyframes in keyframeSet.Values)
                for (int i = 0; i < keyframes.Count - 1; i++)
                    keyframes[i].Next = keyframes[i + 1];
            return keyframeSet.ToDictionary(r => r.Key, r => r.Value.First());
        }
        public static AnimationData LoadAnimation(JObject jobj)
        {
            AnimationData output = new AnimationData();
            foreach (JToken animationNode in (JArray)jobj["animations"])
            {
                DefaultDict<string, List<Keyframe<Quaternion>>> rotations = new DefaultDict<string, List<Keyframe<Quaternion>>>(() => new List<Keyframe<Quaternion>>(), true);
                DefaultDict<string, List<Keyframe<Vector3>>> translations = new DefaultDict<string, List<Keyframe<Vector3>>>(() => new List<Keyframe<Vector3>>(), true);
                foreach (JToken boneNode in animationNode["bones"])
                {
                    foreach (JToken keyFrameNode in boneNode["keyframes"])
                    {
                        JToken rotationNode = keyFrameNode["rotation"];
                        if (rotationNode != null)
                            rotations[(string)boneNode["boneId"]].Add(new Keyframe<Quaternion>((float)keyFrameNode["keytime"] / 1000.0f, rotationNode.ToQuaternion()));
                        JToken translationNode = keyFrameNode["translation"];
                        if (translationNode != null)
                            translations[(string)boneNode["boneId"]].Add(new Keyframe<Vector3>((float)keyFrameNode["keytime"] / 1000.0f, translationNode.ToVector3()));
                    }
                }

                float duration = rotations.Values.SelectMany(k => k.Select(r => r.Time))
                    .Union(translations.Values.SelectMany(k => k.Select(r => r.Time)))
                    .Max();
                string animName = (string)animationNode["id"];
                output[animName] = new AnimationClip(animName, duration,
                    SortKeyframes(translations),
                    SortKeyframes(rotations));
            }
            return output;
        }
    }
}
