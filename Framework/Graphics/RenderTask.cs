using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Graphics
{
    public struct RenderProperties
    {
        public RenderProperties(PrimitiveType primitiveType, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, SpectrumEffect effect)
        {
            PartID = -1;
            World = null;
            PrimitiveType = primitiveType;
            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;
            Material = null;
            Effect = effect;
            DisableDepthBuffer = false;
            DisableShadow = true;
        }
        public RenderProperties(DrawablePart part, Matrix? world = null, MaterialData material = null, SpectrumEffect effect = null,
            bool disableDepthBuffer = false, bool disableShadow = false)
        {
            PartID = part.ReferenceID;
            World = world;
            PrimitiveType = part.primType;
            VertexBuffer = part.VBuffer;
            IndexBuffer = part.IBuffer;
            Material = material ?? part.material;
            Effect = effect ?? part.effect;
            DisableDepthBuffer = disableDepthBuffer;
            DisableShadow = disableShadow;
        }
        public int PartID;
        public Matrix? World;
        public PrimitiveType PrimitiveType;
        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;
        public MaterialData Material;
        public SpectrumEffect Effect;
        public bool DisableDepthBuffer;
        public bool DisableShadow;
        public static bool operator !=(RenderProperties a, RenderProperties b) => !a.Equals(b);
        public static bool operator ==(RenderProperties a, RenderProperties b) => a.Equals(b);

        public override bool Equals(object obj)
        {
            if (!(obj is RenderProperties))
            {
                return false;
            }

            var properties = (RenderProperties)obj;
            return PartID == properties.PartID &&
                   EqualityComparer<Matrix?>.Default.Equals(World, properties.World) &&
                   EqualityComparer<MaterialData>.Default.Equals(Material, properties.Material) &&
                   EqualityComparer<SpectrumEffect>.Default.Equals(Effect, properties.Effect) &&
                   DisableDepthBuffer == properties.DisableDepthBuffer &&
                   DisableShadow == properties.DisableShadow;
        }

        public override int GetHashCode()
        {
            var hashCode = 656074006;
            hashCode = hashCode * -1521134295 + PartID.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Matrix?>.Default.GetHashCode(World);
            hashCode = hashCode * -1521134295 + EqualityComparer<SpectrumEffect>.Default.GetHashCode(Effect);
            hashCode = hashCode * -1521134295 + EqualityComparer<MaterialData>.Default.GetHashCode(Material);
            hashCode = hashCode * -1521134295 + DisableDepthBuffer.GetHashCode();
            hashCode = hashCode * -1521134295 + DisableShadow.GetHashCode();
            return hashCode;
        }
    }
    public class RenderCall
    {
        public readonly RenderProperties Properties;
        // Two kinds of RenderCalls, either InstanceData is null or not.
        // If InstanceData is not null means it was auto batched, otherwise it was manually batched.
        // If InstanceData is null, InstanceBuffer and Material MUST NOT BE NULL.
        public HashSet<InstanceData> InstanceData = new HashSet<InstanceData>();

        public DynamicVertexBuffer InstanceBuffer;

        public RenderCall(RenderProperties key)
        {
            Properties = key;
        }
        public RenderCall(RenderProperties key, Matrix world)
            : this(key)
        {
            InstanceData.Add(new InstanceData() { Material = key.Material, World = world });
        }
        public void Squash()
        {
            if (InstanceData.Skip(1).Any())
                InstanceBuffer = VertexHelper.MakeInstanceBuffer(InstanceData.Select(instance => instance.World).ToArray());
        }
    }
    public struct RenderCallKey
    {
        public RenderProperties Properties;
        public InstanceData Instance;

        public RenderCallKey(RenderProperties properties, InstanceData instance)
        {
            Properties = properties;
            Instance = instance;
        }
    }
    public class InstanceData
    {
        public MaterialData Material;
        public Matrix World;
    }
}
