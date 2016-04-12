using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    public class HeightmapParser : CachedContentParser<float[,], float[,]>
    {
        protected override float[,] LoadData(string path, string name)
        {
            Texture2D image = ContentHelper.ContentParsers[typeof(Texture2D)].Load(path, name) as Texture2D;
            Color[] colors = new Color[image.Width * image.Height];
            image.GetData(colors);
            float[,] output = new float[image.Width, image.Height];
            for (int i = 0; i < colors.Count(); i++)
            {
                output[i % image.Width, i / image.Height] = colors[i].B / 255.0f;
            }
            return output;
        }

        protected override float[,] SafeCopy(float[,] data)
        {
            return (float[,])data.Clone();
        }
        public static float[,] ScaleHeightmap(float[,] heightmap, float scaleY)
        {
            heightmap = (float[,])heightmap.Clone();
            for (int x = 0; x < heightmap.GetLength(0); x++)
            {
                for (int y = 0; y < heightmap.GetLength(1); y++)
                {
                    heightmap[x, y] *= scaleY;
                }
            }
            return heightmap;
        }
        public static List<DrawablePart> CreateParts<T>(float[,] heightmap, float scaleXZ, VertexConstructor<T> vertexConstructor, int sectionCountX, int sectionCountZ) where T : struct, IVertexType
        {
            List<DrawablePart> output = new List<DrawablePart>();
            int width = heightmap.GetLength(0);
            int sectionWidth = width / sectionCountX;
            int height = heightmap.GetLength(1);
            int sectionHeight = height / sectionCountZ;

            for (int x = 0; x < sectionCountX; x++)
            {
                for (int y = 0; y < sectionCountZ; y++)
                {
                    output.Add(CreatePart<T>(heightmap, scaleXZ, vertexConstructor,
                        sectionWidth * x, Math.Min(sectionWidth * (x + 1) + 1, width),
                        sectionHeight * y, Math.Min(sectionHeight * (y + 1) + 1, height)));
                }
            }
            return output;
        }
        public static DrawablePart CreatePart<T>(float[,] heightmap, float scaleXZ, VertexConstructor<T> vertexConstructor) where T : struct, IVertexType
        {
            return CreatePart<T>(heightmap, scaleXZ, vertexConstructor, 0, heightmap.GetLength(0), 0, heightmap.GetLength(1));
        }
        public static DrawablePart CreatePart<T>(float[,] heightmap, float scaleXZ, VertexConstructor<T> vertexConstructor, int minX, int maxX, int minZ, int maxZ) where T : struct, IVertexType
        {
            List<T> vertices = new List<T>();
            for (int y = minZ; y < maxZ; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    vertices.Add(VertexHelper.getVertex<T>(x, y, heightmap, scaleXZ, vertexConstructor));
                }
            }
            VertexBuffer vBuffer = VertexHelper.MakeVertexBuffer<T>(vertices);
            IndexBuffer iBuffer = VertexHelper.MakeIndexBuffer(VertexHelper.getIndexList(maxX - minX, maxZ - minZ).ToList());
            return new DrawablePart(vBuffer, iBuffer);
        }
    }
}
