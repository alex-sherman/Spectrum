using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Physics;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Physics.LinearMath;

namespace Spectrum.Framework.Graphics
{
    public class MaterialData
    {
        public static MaterialData Missing { get; } = new MaterialData() { DiffuseColor = Color.HotPink };
        public string Id;
        public Color DiffuseColor = Color.White;
        public Texture2D DiffuseTexture;
        public Color SpecularColor = Color.Black;
        public Texture2D NormalMap;
        public Texture2D TransparencyMap;
    }
    public class DrawablePart
    {
        private static int LastReferenceID = 0;
        public int ReferenceID { get; private set; }
        public MaterialData material = new MaterialData();
        public Matrix permanentTransform = Matrix.Identity;
        public Matrix transform = Matrix.Identity;
        public SpectrumEffect effect;
        public VertexBuffer VBuffer;
        public IndexBuffer IBuffer;
        public PrimitiveType primType = PrimitiveType.TriangleList;
        public JBBox Bounds { get; private set; }
        public static JBBox GetBounds<T>(List<T> vertices) where T : struct, ICommonTex
        {
            JBBox output = JBBox.SmallBox;
            foreach (var vert in vertices)
                output.AddPoint(vert.Position);

            return output;
        }
        public static DrawablePart From<T>(List<T> vertices) where T : struct, ICommonTex
        {
            DrawablePart part = new DrawablePart();
            part.VBuffer = VertexHelper.MakeVertexBuffer(vertices);
            part.primType = PrimitiveType.TriangleStrip;
            part.Bounds = GetBounds(vertices);
            return part;
        }
        public static DrawablePart From<T>(List<T> vertices, List<uint> indices) where T : struct, ICommonTex
        {
            DrawablePart part = new DrawablePart();
            part.VBuffer = VertexHelper.MakeVertexBuffer(vertices);
            part.IBuffer = VertexHelper.MakeIndexBuffer(indices);
            part.Bounds = GetBounds(vertices);
            return part;
        }
        public static DrawablePart From<T>(List<T> vertices, List<ushort> indices) where T : struct, ICommonTex
        {
            DrawablePart part = new DrawablePart();
            part.VBuffer = VertexHelper.MakeVertexBuffer(vertices);
            part.IBuffer = VertexHelper.MakeIndexBuffer(indices);
            part.Bounds = GetBounds(vertices);
            return part;
        }
        public DrawablePart CreateReference()
        {
            return new DrawablePart(VBuffer, IBuffer, Bounds) { ReferenceID = ReferenceID, effect = effect, material = material, permanentTransform = permanentTransform };
        }
        public DrawablePart(VertexBuffer vBuffer, IndexBuffer iBuffer, JBBox bounds = default(JBBox))
            : this()
        {
            this.VBuffer = vBuffer;
            this.IBuffer = iBuffer;
            Bounds = bounds;
        }
        public DrawablePart() { ReferenceID = LastReferenceID++; }
    }
}
