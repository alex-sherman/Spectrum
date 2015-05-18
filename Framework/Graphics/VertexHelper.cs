﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Graphics
{
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
            if (SpectrumGame.Game.GraphicsDevice == null) { return null; }
            VertexBuffer vBuffer = new VertexBuffer(SpectrumGame.Game.GraphicsDevice, (new T()).VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
            vBuffer.SetData(vertices.ToArray());
            return vBuffer;
        }
        public static IndexBuffer MakeIndexBuffer(List<ushort> indices)
        {
            if (indices.Count == 0) { return null; }
            IndexBuffer iBuffer = new IndexBuffer(SpectrumGame.Game.GraphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
            iBuffer.SetData(indices.ToArray());
            return iBuffer;
        }
        public static IndexBuffer MakeIndexBuffer(List<uint> indices)
        {
            if (indices.Count == 0 || SpectrumGame.Game.GraphicsDevice == null) { return null; }
            IndexBuffer iBuffer = new IndexBuffer(SpectrumGame.Game.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            iBuffer.SetData(indices.ToArray());
            return iBuffer;
        }
        public delegate IVertexType VertexConstructor(VertexArgs args);
        public static ushort[] getIndexList(int squareSize, int scale, int numParts = 2)
        {
            squareSize /= numParts;
            //If there are extra parts, there are extra vertices on the ends,
            //so that the meshes will line up. If there is only one part,
            //the following algorithm should operate on square size -1,
            //as it will already include the vertices on the edges.
            if (numParts == 1)
            {
                squareSize -= 1;
            }
            ushort[] toReturn = new ushort[(squareSize) * (squareSize) * 6 / scale / scale];
            for (int i = 0; i < toReturn.Length / 6; i++)
            {
                toReturn[i * 6] = (ushort)(scale * (i + (i) / ((squareSize) / scale)) + ((i) / ((squareSize) / scale)) * (scale - 1) * squareSize);
                toReturn[i * 6 + 1] = (ushort)(toReturn[i * 6] + (squareSize + 2) * scale);
                toReturn[i * 6 + 2] = (ushort)(toReturn[i * 6] + scale);
                toReturn[i * 6 + 3] = toReturn[i * 6];
                toReturn[i * 6 + 4] = (ushort)(toReturn[i * 6] + (squareSize + 2) * scale);
                toReturn[i * 6 + 5] = (ushort)(toReturn[i * 6] + (squareSize + 1) * scale);
            }

            return toReturn;
        }
        public static IVertexType getVertex(int x, int y, float[,] heights, float scale, VertexConstructor constructor)
        {
            float a = scale;
            int size = heights.GetUpperBound(0);
            float b = 1 / 4.0f;
            int x1 = x;
            int y1 = y;
            if (x1 > size) { x1 = size; }
            if (y1 > size) { y1 = size; }
            VertexArgs args = new VertexArgs();
            args.pos = GetPosition(x1, y1, heights, scale);
            args.normal = GetNormal(x1, y1, heights, scale);
            args.texturePos = new Vector2(((x * b)), ((y * b)));
            return constructor(args);

        }

        public static Vector3 GetPosition(int x, int y, float[,] heights, float scale)
        {
            return new Vector3((x * scale), heights[x, y], (y * scale));
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
