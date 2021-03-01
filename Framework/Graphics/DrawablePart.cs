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
    [Flags]
    public enum SamplerMode
    {
        Linear = 0b10,
        Wrap = 0b01,

        LinearClamp = 0b10,
        LinearWrap = 0b11,
        PointClamp = 0b00,
        PointWrap = 0b01,
    }
    public class MaterialData : IEquatable<MaterialData>
    {
        public static MaterialData Missing { get; } = new MaterialData() { DiffuseColor = "hotpink" };
        public string Id;
        public Color DiffuseColor = Color.White;
        public Texture2D DiffuseTexture;
        public SamplerMode DiffuseSampler = SamplerMode.LinearWrap;
        public Color SpecularColor = Color.Black;
        public Texture2D NormalMap;
        public Texture2D TransparencyMap;
        public bool DisableLighting;

        public override int GetHashCode()
        {
            var hashCode = 1496102582;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(DiffuseColor);
            hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(DiffuseTexture);
            hashCode = hashCode * -1521134295 + EqualityComparer<SamplerMode>.Default.GetHashCode(DiffuseSampler);
            hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(SpecularColor);
            hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(NormalMap);
            hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(TransparencyMap);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(DisableLighting);
            return hashCode;
        }
        public bool Equals(MaterialData data)
        {
            return data != null &&
                   Id == data.Id &&
                   DiffuseColor.Equals(data.DiffuseColor) &&
                   EqualityComparer<Texture2D>.Default.Equals(DiffuseTexture, data.DiffuseTexture) &&
                   EqualityComparer<SamplerMode>.Default.Equals(DiffuseSampler, data.DiffuseSampler) &&
                   SpecularColor.Equals(data.SpecularColor) &&
                   EqualityComparer<Texture2D>.Default.Equals(NormalMap, data.NormalMap) &&
                   EqualityComparer<Texture2D>.Default.Equals(TransparencyMap, data.TransparencyMap) &&
                   EqualityComparer<bool>.Default.Equals(DisableLighting, data.DisableLighting);

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
        internal Matrix permanentTransform = Matrix.Identity;
        public Matrix transform = Matrix.Identity;
        public SpectrumEffect effect;
        public VertexBuffer VBuffer;
        public IndexBuffer IBuffer;
        public PrimitiveType primType = PrimitiveType.TriangleList;
        List<ICommonTex> vertices = null;
        List<uint> indices = null;
        public JBBox Bounds
        {
            get
            {
                JBBox output = JBBox.SmallBox;
                var selectedVerts = indices == null ? vertices : indices.Select(i => vertices[(int)i % vertices.Count]);
                foreach (var vert in selectedVerts)
                    output.AddPoint(permanentTransform * transform * vert.Position);
                return output;
            }
        }
        public static DrawablePart From<T>(List<T> vertices) where T : struct, ICommonTex
        {
            DrawablePart part = new DrawablePart
            {
                effect = new SpectrumEffect(),
                VBuffer = VertexHelper.MakeVertexBuffer(vertices),
                primType = PrimitiveType.TriangleStrip,
                vertices = vertices.Cast<ICommonTex>().ToList(),
            };
            return part;
        }
        public static DrawablePart From<T>(List<T> vertices, List<uint> indices) where T : struct, ICommonTex
        {
            DrawablePart part = new DrawablePart
            {
                effect = new SpectrumEffect(),
                VBuffer = VertexHelper.MakeVertexBuffer(vertices),
                IBuffer = VertexHelper.MakeIndexBuffer(indices),
                vertices = vertices.Cast<ICommonTex>().ToList(),
                indices = indices,
            };
            return part;
        }
        public static DrawablePart From<T>(List<T> vertices, List<ushort> indices) where T : struct, ICommonTex
        {
            DrawablePart part = new DrawablePart
            {
                effect = new SpectrumEffect(),
                VBuffer = VertexHelper.MakeVertexBuffer(vertices),
                IBuffer = VertexHelper.MakeIndexBuffer(indices),
                vertices = vertices.Cast<ICommonTex>().ToList(),
                indices = indices.Select<ushort, uint>(i => i).ToList(),
            };
            return part;
        }
        // TODO: some of these fields should be readonly, since creating a reference and modifying them will break batching
        public DrawablePart CreateReference()
        {
            return new DrawablePart(VBuffer, IBuffer)
            {
                ReferenceID = ReferenceID,
                effect = effect.Clone() as SpectrumEffect,
                primType = primType,
                material = material,
                permanentTransform = permanentTransform,
                vertices = vertices,
                indices = indices,
            };
        }
        public DrawablePart(VertexBuffer vBuffer, IndexBuffer iBuffer)
            : this()
        {
            VBuffer = vBuffer;
            IBuffer = iBuffer;
        }
        public DrawablePart() { ReferenceID = LastReferenceID++; }
    }
}
