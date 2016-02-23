using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Physics;
using Microsoft.Xna.Framework;

namespace Spectrum.Framework.Graphics
{
    public class DrawablePart
    {
        public Matrix transform = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        public SpectrumEffect effect;
        public VertexBuffer VBuffer;
        public IndexBuffer IBuffer;
        public DynamicVertexBuffer InstanceBuffer;
        public PrimitiveType primType = PrimitiveType.TriangleList;
        public DrawablePart(List<CommonTex> vertices)
        {
            VBuffer = VertexHelper.MakeVertexBuffer(vertices);
            primType = PrimitiveType.TriangleStrip;
        }
        public DrawablePart(List<CommonTex> vertices, List<ushort> indices)
        {
            VBuffer = VertexHelper.MakeVertexBuffer(vertices);
            IBuffer = VertexHelper.MakeIndexBuffer(indices);
        }
        public DrawablePart(List<MultiTex> vertices, List<ushort> indices)
        {

            VBuffer = VertexHelper.MakeVertexBuffer(vertices);
            IBuffer = VertexHelper.MakeIndexBuffer(indices);
        }
        public DrawablePart(VertexBuffer vBuffer, IndexBuffer iBuffer)
        {

            this.VBuffer = vBuffer;
            this.IBuffer = iBuffer;
        }
        public DrawablePart() { }
    }
}
