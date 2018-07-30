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
    public class MaterialData : IEquatable<MaterialData>
    {
        public static MaterialData Missing { get; } = new MaterialData() { DiffuseColor = Color.HotPink };
        public string Id;
        public Color DiffuseColor = Color.White;
        public Texture2D DiffuseTexture;
        public Color SpecularColor = Color.Black;
        public Texture2D NormalMap;
        public Texture2D TransparencyMap;

        public override int GetHashCode()
        {
            var hashCode = 1496102582;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(DiffuseColor);
            hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(DiffuseTexture);
            hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(SpecularColor);
            hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(NormalMap);
            hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(TransparencyMap);
            return hashCode;
        }
        public bool Equals(MaterialData data)
        {
            return data != null &&
                   Id == data.Id &&
                   DiffuseColor.Equals(data.DiffuseColor) &&
                   EqualityComparer<Texture2D>.Default.Equals(DiffuseTexture, data.DiffuseTexture) &&
                   SpecularColor.Equals(data.SpecularColor) &&
                   EqualityComparer<Texture2D>.Default.Equals(NormalMap, data.NormalMap) &&
                   EqualityComparer<Texture2D>.Default.Equals(TransparencyMap, data.TransparencyMap);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as MaterialData);
        }
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
