using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Graphics
{
    public class Billboard : GameObject
    {
        public Vector2 Size;
        public override List<RenderTask> GetRenderTasks(RenderPhaseInfo phase)
        {
            if (RenderProperties.Material != null)
                return new List<RenderTask>() { Draw3D.Draw3DRectangle(
                    Quaternion.Concatenate(Quaternion.CreateFromAxisAngle(Vector3.Right, (float)Math.PI / 2), orientation),
                    position, Size, RenderProperties) };
            return null;
        }
    }
}
