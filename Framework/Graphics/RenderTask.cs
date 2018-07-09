﻿using Microsoft.Xna.Framework;
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
        public RenderProperties(DrawablePart part, SpectrumEffect effect = null, bool disableDepthBuffer = false, bool disableShadow = false)
        {
            PartID = part.ReferenceID;
            PrimitiveType = part.primType;
            VertexBuffer = part.VBuffer;
            IndexBuffer = part.IBuffer;
            Effect = effect ?? part.effect;
            DisableDepthBuffer = disableDepthBuffer;
            DisableShadow = disableShadow;
        }
        public int PartID;
        public PrimitiveType PrimitiveType;
        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;
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
                   EqualityComparer<SpectrumEffect>.Default.Equals(Effect, properties.Effect) &&
                   DisableDepthBuffer == properties.DisableDepthBuffer &&
                   DisableShadow == properties.DisableShadow;
        }

        public override int GetHashCode()
        {
            var hashCode = 656074006;
            hashCode = hashCode * -1521134295 + PartID.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<SpectrumEffect>.Default.GetHashCode(Effect);
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
        public List<InstanceData> InstanceData;

        public DynamicVertexBuffer InstanceBuffer;
        public MaterialData Material;

        public RenderCall(RenderProperties key)
        {
            Properties = key;
        }
        public bool merged;
        public void Squash()
        {
            if (InstanceData.Any())
            {
                InstanceBuffer = VertexHelper.MakeInstanceBuffer(InstanceData.Select(instance => instance.World).ToArray());
                merged = true;
            }
        }
    }
    public struct InstanceData
    {
        public MaterialData Material;
        public Matrix World;
    }
}
