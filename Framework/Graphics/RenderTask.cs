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
        public MaterialData Material;
        public SpectrumEffect Effect;
        public bool DisableDepthBuffer;
        public bool DisableInstance;
        public override bool Equals(object obj)
        {
            if (obj is RenderProperties other)
                return Material == other.Material && Effect == other.Effect && DisableDepthBuffer == other.DisableDepthBuffer && DisableInstance == other.DisableInstance;
            return false;
        }
        public static bool operator !=(RenderProperties a, RenderProperties b) => !a.Equals(b);
        public static bool operator ==(RenderProperties a, RenderProperties b) => a.Equals(b);
        public override int GetHashCode()
        {
            var hashCode = 128918506;
            hashCode = hashCode * -1521134295 + EqualityComparer<MaterialData>.Default.GetHashCode(Material);
            hashCode = hashCode * -1521134295 + EqualityComparer<SpectrumEffect>.Default.GetHashCode(Effect);
            hashCode = hashCode * -1521134295 + DisableDepthBuffer.GetHashCode();
            hashCode = hashCode * -1521134295 + DisableInstance.GetHashCode();
            return hashCode;
        }
    }
    public class RenderTask
    {
        public RenderTask(DrawablePart part, string tag = "Misc") { this.part = part; this.tag = tag; }
        public DrawablePart part;
        public bool DisableInstance { get => Properties.DisableInstance; set => Properties.DisableInstance = value; }
        public bool DisableDepthBuffer { get => Properties.DisableDepthBuffer; set => Properties.DisableDepthBuffer = value; }
        public RenderProperties Properties;
        public string tag;
        public MaterialData Material { get => Properties.Material ?? part.material; set => Properties.Material = value; }
        public SpectrumEffect Effect { get => Properties.Effect ?? part.effect; }
        public List<Matrix> instances = null;
        public DynamicVertexBuffer instanceBuffer;
        public bool Dirty = true;
        public void Merge()
        {
            if (instances.Any())
                instanceBuffer = VertexHelper.MakeInstanceBuffer(instances.ToArray());
            merged = true;
        }
        public Matrix InstanceWorld = Matrix.Identity;
        public Matrix? world;
        public Matrix WorldValue
        {
            get { return instanceBuffer != null ? InstanceWorld : (part.permanentTransform * part.transform * (world ?? Matrix.Identity)); }
        }
        public bool merged;
    }
}
