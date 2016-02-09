using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Graphics
{
    public delegate T VertexConstructor<T>(VertexArgs args) where T : struct, IVertexType;
    public struct VertexArgs
    {
        public Vector3 pos;
        public Vector3 normal;
        public Vector2 texturePos;
    }
    public class VertexHelper
    {
        public static VertexBuffer MakeVertexBuffer<T>(List<T> vertices) where T : struct, IVertexType
        {
            if (SpectrumGame.Game.GraphicsDevice == null) { throw new NullReferenceException("Graphics device must have been initialized before this operation can be performed"); }
            VertexBuffer vBuffer = new VertexBuffer(SpectrumGame.Game.GraphicsDevice, vertices[0].VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
            vBuffer.SetData(vertices.ToArray());
            return vBuffer;
        }
        public static IndexBuffer MakeIndexBuffer(List<ushort> indices)
        {
            if (SpectrumGame.Game.GraphicsDevice == null) { throw new NullReferenceException("Graphics device must have been initialized before this operation can be performed"); }
            if (indices.Count == 0) { throw new ArgumentException("Indices must have length greater than 0"); }
            IndexBuffer iBuffer = new IndexBuffer(SpectrumGame.Game.GraphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
            iBuffer.SetData(indices.ToArray());
            return iBuffer;
        }
        public static IndexBuffer MakeIndexBuffer(List<uint> indices)
        {
            if (SpectrumGame.Game.GraphicsDevice == null) { throw new NullReferenceException("Graphics device must have been initialized before this operation can be performed"); }
            if (indices.Count == 0 || SpectrumGame.Game.GraphicsDevice == null) { return null; }
            IndexBuffer iBuffer = new IndexBuffer(SpectrumGame.Game.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            iBuffer.SetData(indices.ToArray());
            return iBuffer;
        }
        public static ushort[] getIndexList(int squareSize)
        {
            return getIndexList(squareSize, squareSize);
        }
        public static ushort[] getIndexList(int meshWidth, int meshHeight)
        {
            ushort[] toReturn = new ushort[(meshWidth - 1) * (meshHeight - 1) * 6];
            for(int x = 0; x < meshWidth - 1; x++)
            {
                for (int y = 0; y < meshHeight - 1; y++)
                {
                    int i = x + y * (meshWidth - 1);
                    toReturn[i * 6] = (ushort)(x + y * meshWidth);
                    toReturn[i * 6 + 1] = (ushort)(toReturn[i * 6] + (meshWidth + 1));
                    toReturn[i * 6 + 2] = (ushort)(toReturn[i * 6] + 1);
                    toReturn[i * 6 + 3] = (ushort)(toReturn[i * 6]);
                    toReturn[i * 6 + 4] = (ushort)(toReturn[i * 6] + (meshWidth + 1));
                    toReturn[i * 6 + 5] = (ushort)(toReturn[i * 6] + (meshWidth));
                }
            }

            return toReturn;
        }
        public static T getVertex<T>(int x, int y, float[,] heights, float scaleXZ, VertexConstructor<T> constructor) where T : struct, IVertexType
        {
            int size = heights.GetUpperBound(0);
            int x1 = x;
            int y1 = y;
            if (x1 > size) { x1 = size; }
            if (y1 > size) { y1 = size; }
            VertexArgs args = new VertexArgs();
            args.pos = GetPosition(x1, y1, heights, scaleXZ);
            args.normal = GetNormal(x1, y1, heights, scaleXZ);
            args.texturePos = new Vector2(((x * 1.0f / size)), ((y * 1.0f / size)));
            return constructor(args);

        }

        public static Vector3 GetPosition(int x, int y, float[,] heights, float scaleXZ)
        {
            return new Vector3((x * scaleXZ), heights[x, y], (y * scaleXZ));
        }

        public static Vector3 GetNormal(int x, int y, float[,] heights, float scale)
        {
            float height = heights[x, y];
            int size = heights.GetUpperBound(0);
            float deriv = 0;
            Vector3 normal;
            normal.Y = 2 * scale;
            if (x > 0)
            {
                deriv = height - heights[x - 1, y];
                if (x < size - 1)
                {
                    deriv += height - heights[x + 1, y];
                    deriv /= 2;
                }
            }
            else
            {
                deriv = height - heights[x + 1, y];
            }
            normal.X = deriv;
            if (y > 0)
            {
                deriv = height - heights[x, y - 1];
                if (y < size - 1)
                {
                    deriv += height - heights[x, y - 1];
                    deriv /= 2;
                }
            }
            else
            {
                deriv = height - heights[x, y + 1];
            }
            normal.Z = deriv;
            normal.Normalize();
            return normal;
        }
    }
}
