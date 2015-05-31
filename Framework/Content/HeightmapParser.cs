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
        protected override float[,] LoadData(string path)
        {
            Texture2D image = ContentHelper.ContentParsers[typeof(Texture2D)].Load(path) as Texture2D;
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
        public static DrawablePart CreatePart<T>(float[,] heightmap, float scaleXZ, VertexConstructor<T> vertexConstructor) where T : struct, IVertexType
        {
            List<T> vertices = new List<T>();
            for (int x = 0; x < heightmap.GetLength(0); x++)
            {
                for(int y = 0; y < heightmap.GetLength(1); y++)
                {
                    vertices.Add(VertexHelper.getVertex<T>(x, y, heightmap, scaleXZ, vertexConstructor));
                }
            }
            VertexBuffer vBuffer = VertexHelper.MakeVertexBuffer<T>(vertices);
            IndexBuffer iBuffer = VertexHelper.MakeIndexBuffer(VertexHelper.getIndexList(heightmap.GetLength(0)).ToList());
            return new DrawablePart(vBuffer, iBuffer);
        }
    }
}
