using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content.ModelParsing
{
    class OBJReader : IModelReader
    {
        private void ParseFile(string path, Action<string> linehandler)
        {
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                        continue;
                    linehandler(line);
                }
            }
        }
        static Vector3 v3(string[] split, int offset = 1)
        {
            return new Vector3(float.Parse(split[offset]), float.Parse(split[offset + 1]), float.Parse(split[offset + 2]));
        }
        static Vector2 v2(string[] split, int offset = 1)
        {
            return new Vector2(float.Parse(split[offset]), 1 - float.Parse(split[offset + 1]));
        }
        public void LoadMaterial(string path, Dictionary<string, MaterialData> materialLookup)
        {
            MaterialData data = null;
            var dir = Path.GetDirectoryName(path);
            ParseFile(path, line =>
            {
                var splitted = line.Split(' ');
                switch (splitted[0])
                {
                    case "newmtl":
                        if (data != null)
                            materialLookup[data.Id] = data;
                        data = new MaterialData() { Id = splitted[1] };
                        break;
                    case "Ka":
                        break;
                    case "Kd":
                        data.diffuseColor = new Color(v3(splitted));
                        break;
                    case "map_Kd":
                        data.diffuseTexture = ContentHelper.Load<Texture2D>(Path.Combine(dir, splitted[1]), false);
                        break;
                    case "Ks":
                        data.specularColor = new Color(v3(splitted));
                        break;
                    case "map_Ks":
                        //data.specularTexture = ContentHelper.Load<Texture2D>(Path.Combine(dir, splitted[1]), false);
                        break;
                    default:
                        break;
                }
            });
            materialLookup[data.Id] = data;
        }
        void FinishPart(ref int meshPartId, ModelParserCache modelData, string groupName, List<CommonTex> vertices, List<uint> indices, MaterialData material)
        {
            var part = DrawablePart.From(vertices, indices);
            part.material = material;
            part.effect = new SpectrumEffect();
            modelData.parts[groupName + "_" + meshPartId] = part;
            meshPartId++;
        }
        public ModelParserCache LoadData(string path, string name)
        {
            var modelData = new ModelParserCache(name);
            List<Vector3> positions = new List<Vector3>();
            List<Vector2> textureUVs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<CommonTex> vertices = new List<CommonTex>();
            List<uint> indices = new List<uint>();
            Dictionary<string, MaterialData> materialData = new Dictionary<string, MaterialData>();
            MaterialData material = MaterialData.Missing;
            string groupName = "Mesh";
            int meshPartId = 0;

            ParseFile(path, line =>
            {
                var splitted = line.Split(' ');
                switch (splitted[0])
                {
                    case "mtllib":
                        LoadMaterial(Path.Combine(Path.GetDirectoryName(path), splitted[1]), materialData);
                        break;
                    case "usemtl":
                        if (vertices.Count != 0)
                        {
                            FinishPart(ref meshPartId, modelData, groupName, vertices, indices, material);
                            indices.Clear();
                        }
                        material = materialData[splitted[1]];
                        break;
                    case "v":
                        positions.Add(v3(splitted));
                        break;
                    case "vt":
                        textureUVs.Add(v2(splitted));
                        break;
                    case "vn":
                        normals.Add(v3(splitted));
                        break;
                    case "f":
                        var vertexIndices =
                            splitted.Skip(1).Where(group => !string.IsNullOrWhiteSpace(group))
                            .Select(group => (group + "//").Split('/').Take(3)
                                .Select(si => string.IsNullOrWhiteSpace(si) ? null : (uint?)(uint.Parse(si) - 1)).ToArray());
                        var startIndex = (uint)vertices.Count;
                        for (int i = 2; i < vertexIndices.Count(); i++)
                        {
                            indices.Add(startIndex);
                            for (int j = -1; j <= 0; j++)
                                indices.Add((uint)(startIndex + j + i));
                        }
                        vertices.AddRange(vertexIndices.Select(group =>
                            new CommonTex(
                                group[0].HasValue ? positions[(int)group[0].Value] : Vector3.Zero,
                                group[2].HasValue ? normals[(int)group[2].Value] : Vector3.Zero,
                                group[1].HasValue ? textureUVs[(int)group[1].Value] : Vector2.Zero)
                        ));

                        break;
                    default:
                        break;
                }
            });
            if (vertices.Count != 0)
            {
                FinishPart(ref meshPartId, modelData, groupName, vertices, indices, material);
            }
            return modelData;
        }
    }
}
