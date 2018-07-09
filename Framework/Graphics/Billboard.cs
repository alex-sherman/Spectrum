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
        public override void Draw(float gameTime)
        {
            base.Draw(gameTime);
            if (Material != null)
                Draw3D.Draw3DRectangle(
                    Manager,
                    Quaternion.Concatenate(Quaternion.CreateFromAxisAngle(Vector3.Right, (float)Math.PI / 2), orientation),
                    position,
                    Size);
        }
    }
}
