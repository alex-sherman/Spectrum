using Microsoft.Xna.Framework;
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
        static Vector3 v3(string[] split, int offset = 1)
        {
            return new Vector3(float.Parse(split[offset]), float.Parse(split[offset + 1]), float.Parse(split[offset + 2]));
        }
        static Vector2 v2(string[] split, int offset = 1)
        {
            return new Vector2(float.Parse(split[offset]), float.Parse(split[offset + 1]));
        }
        void FinishPart(ref int meshPartId, ModelParserCache modelData, string groupName, List<CommonTex> vertices, List<ushort> indices)
        {
            var part = DrawablePart.From(vertices, indices);
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
            List<ushort> indices = new List<ushort>();
            string groupName = "Mesh";
            int meshPartId = 0;

            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                        continue;
                    var splitted = line.Split(' ');
                    switch (splitted[0])
                    {
                        case "usemtl":
                            if(vertices.Count != 0)
                            {
                                FinishPart(ref meshPartId, modelData, groupName, vertices, indices);
                            }
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
                                    .Select(si => string.IsNullOrWhiteSpace(si) ? null : (ushort?)(ushort.Parse(si) - 1)).ToArray());
                            var startIndex = (ushort)vertices.Count;
                            for (int i = 2; i < vertexIndices.Count(); i++)
                            {
                                indices.Add(startIndex);
                                for (int j = -1; j <= 0; j++)
                                    indices.Add((ushort)(startIndex + j + i));
                            }
                            vertices.AddRange(vertexIndices.Select(group =>
                                new CommonTex(
                                    group[0].HasValue ? positions[group[0].Value] : Vector3.Zero,
                                    group[2].HasValue ? normals[group[2].Value] : Vector3.Zero,
                                    group[1].HasValue ? textureUVs[group[1].Value] : Vector2.Zero)
                            ));

                            break;
                        default:
                            break;
                    }
                }
                if (vertices.Count != 0)
                {
                    FinishPart(ref meshPartId, modelData, groupName, vertices, indices);
                }
            }


            return modelData;
        }
    }
}
