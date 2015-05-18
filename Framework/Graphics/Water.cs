using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Graphics;
using Microsoft.Xna.Framework;
using Spectrum.Framework;
using Spectrum.Framework.Network;
using Spectrum.Framework.Physics;
using Spectrum.Framework.Entities;

namespace Spectrum.Framework.Graphics
{
    public class Water : GameObject
    {
        public const float waterHeight = 0;
        public static RenderTarget2D refractionRenderTarget;
        public static RenderTarget2D reflectionRenderTarget;
        public VertexBuffer waterV;
        private int size;
        int numVertices = 32;
        public Water(int size, string waterBump1, string waterBump2)
            : base()
        {
            IsStatic = true;
            this.Ignore = true;
            this.AllowReplicate = false;
            this.size = size;
            waterV = new VertexBuffer(SpectrumGame.Game.GraphicsDevice, VertexPositionTexture.VertexDeclaration, numVertices * numVertices, BufferUsage.WriteOnly);
            float[,] heights = new float[numVertices, numVertices];
            VertexPositionTexture[] verts = new VertexPositionTexture[numVertices * numVertices];
            for (int x = 0; x < numVertices; x++)
            {
                for (int y = 0; y < numVertices; y++)
                {
                    verts[x + y * numVertices] = (VertexPositionTexture)VertexHelper.getVertex(x, y, heights, size * 1.0f / (numVertices - 1), Constructor);
                }
            }
            waterV.SetData(verts);
            IndexBuffer iBuffer = VertexHelper.MakeIndexBuffer(VertexHelper.getIndexList(numVertices, 1, 1).ToList());
            DrawablePart p = new DrawablePart(waterV, iBuffer);
            p.effect = new WaterEffect(waterBump1, waterBump2);
            Parts.Add(p);
        }
        public static void ResetRenderTargets()
        {
            refractionRenderTarget = new RenderTarget2D(SpectrumGame.Game.GraphicsDevice, (int)(2048.0 / Math.Pow(2, Settings.waterQuality)),
                (int)(2048.0 / Math.Pow(2, Settings.waterQuality)), false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            reflectionRenderTarget = new RenderTarget2D(SpectrumGame.Game.GraphicsDevice, (int)(2048.0 / Math.Pow(2, Settings.waterQuality)),
                (int)(2048.0 / Math.Pow(2, Settings.waterQuality)), false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        }
        private static IVertexType Constructor(VertexArgs args)
        {
            return new VertexPositionTexture(args.pos + Vector3.Up * waterHeight, args.texturePos);
        }
    }
}
