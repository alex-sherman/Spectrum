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
    public interface ICommonTex : IVertexType
    {
        Vector3 Position { get; set; }
        Vector2 TextureCoordinate { get; set; }
        Vector3 Normal { get; set; }
        Vector3 Tangent { get; set; }
    }
    public struct CommonTex : ICommonTex
    {
        public Vector3 position;
        public Vector3 Position { get { return position; } set { position = value; } }
        public Vector2 textureCoordinate;
        public Vector2 TextureCoordinate { get { return textureCoordinate; } set { textureCoordinate = value; } }
        public Vector3 normal;
        public Vector3 Normal { get { return normal; } set { normal = value; } }
        public Vector3 tangent;
        public Vector3 Tangent { get { return tangent; } set { tangent = value; } }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0)
        );
        public CommonTex(Vector3 pos, Vector3 normal, Vector2 texPos)
        {
            position = pos;
            textureCoordinate = texPos;
            this.normal = normal;
            tangent = Vector3.Zero;
        }

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    };
    public struct MultiTex : ICommonTex
    {
        public Vector3 position;
        public Vector3 Position { get { return position; } set { position = value; } }
        public Vector2 textureCoordinate;
        public Vector2 TextureCoordinate { get { return textureCoordinate; } set { textureCoordinate = value; } }
        public Vector3 normal;
        public Vector3 Normal { get { return normal; } set { normal = value; } }
        public Vector3 tangent;
        public Vector3 Tangent { get { return tangent; } set { tangent = value; } }
        public Vector4 BlendWeight;
        public Vector3 Position2;
        public Vector4 BlendWeight2;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(44, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(60, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(72, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3)
        );

        public MultiTex(Vector3 pos, Vector3 normal, Vector2 texPos, Vector4 blendWeights)
        {
            position = pos;
            textureCoordinate = texPos;
            this.normal = normal;
            tangent = Vector3.Zero;
            BlendWeight = blendWeights;
            Position2 = pos;
            BlendWeight2 = blendWeights;
        }

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    };
    public struct SkinnedVertex : ICommonTex
    {
        public Vector3 position;
        public Vector3 Position { get { return position; } set { position = value; } }
        public Vector2 textureCoordinate;
        public Vector2 TextureCoordinate { get { return textureCoordinate; } set { textureCoordinate = value; } }
        public Vector3 normal;
        public Vector3 Normal { get { return normal; } set { normal = value; } }
        public Vector3 tangent;
        public Vector3 Tangent { get { return tangent; } set { tangent = value; } }
        public Vector4 BlendIndices;
        public Vector4 Blendweight0;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(44, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0),
            new VertexElement(60, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    };
}