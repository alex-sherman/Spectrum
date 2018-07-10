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
        public Billboard()
        {
            DisableInstancing = true;
        }
        public override void Draw(float gameTime)
        {
            base.Draw(gameTime);
            if (Material != null)
            {
                Manager.DrawPart(
                    Draw3D.BillboardPart,
                    Draw3D.GetBillboardTransform(
                        Quaternion.Concatenate(Quaternion.CreateFromAxisAngle(Vector3.Right, (float)Math.PI / 2), orientation),
                        position, Size),
                    Material,
                    disableDepthBuffer: DisableDepthBuffer,
                    disableInstancing: DisableInstancing
                );
            }
        }
    }
}
