using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Spectrum.Framework.Graphics
{
    public struct CommonTex : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector3 Normal;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );
        public CommonTex(Vector3 pos, Vector3 normal, Vector2 texPos)
        {
            Position = pos;
            TextureCoordinate = texPos;
            Normal = normal;
        }

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    };
    public struct MultiTex : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector3 Normal;
        public Vector4 BlendWeight;
        public Vector3 Position2;
        public Vector4 BlendWeight2;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(48, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(60, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3)
        );

        public MultiTex(Vector3 pos, Vector3 normal, Vector2 texPos, Vector4 blendWeights)
        {
            Position = pos;
            TextureCoordinate = texPos;
            Normal = normal;
            BlendWeight = blendWeights;
            Position2 = pos;
            BlendWeight2 = blendWeights;
        }

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    };
    public struct SkinnedVertex : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector3 Normal;
        public Vector4 BlendIndices;
        public Vector4 Blendweight0;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    };
}