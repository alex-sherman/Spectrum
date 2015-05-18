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
    class ModelParserCache
    {
        public JObject jobj;
        public VertexBuffer VBuffer;
        public Dictionary<string, IndexBuffer> indices;
        public ModelParserCache(JObject jobj, VertexBuffer vbuffer, Dictionary<string, IndexBuffer> indices)
        {
            this.jobj = jobj;
            this.VBuffer = vbuffer;
            this.indices = indices;
        }
    }
    class ModelParser : CachedContentParser<ModelParserCache, SpecModel>
    {


        protected override ModelParserCache LoadData(string path)
        {
            JsonTextReader reader = new JsonTextReader(new StreamReader(path));
            JObject jobj = JObject.Load(reader);
            if (jobj["meshes"] == null) { throw new InvalidOperationException("Provided model has no mesh data"); }


            JObject mesh = (JObject)jobj["meshes"][0];
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
            VertexBuffer vBuffer = VertexHelper.MakeVertexBuffer(vertices);
            Dictionary<string, IndexBuffer> partsIndices = new Dictionary<string, IndexBuffer>();
            foreach (JObject meshPart in mesh["parts"])
            {
                List<uint> indices = ((JArray)meshPart["indices"]).ToList().ConvertAll(x => (uint)x);
                IndexBuffer iBuffer = VertexHelper.MakeIndexBuffer(indices);
                partsIndices[(string)meshPart["id"]] = iBuffer;
            }


            return new ModelParserCache(jobj, vBuffer, partsIndices);
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
                parts[partIbuffer.Key] = new DrawablePart(data.VBuffer, partIbuffer.Value);
            }
            foreach (JToken nodePart in ((JArray)data.jobj["nodes"])[0]["parts"])
            {
                if (nodePart["bones"] != null)
                    parts[(string)nodePart["meshpartid"]].effect = new CustomSkinnedEffect((nodePart["bones"]).ToList().ConvertAll(x => (string)x["node"]).ToArray());
                else
                    parts[(string)nodePart["meshpartid"]].effect = new SpectrumEffect();
            }
            return new SpecModel(parts, GetSkinningData(data.jobj));
        }
    }
}
