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
    public static class Draw3D
    {
        public static readonly DrawablePart BillboardPart;
        static Draw3D()
        {
            BillboardPart = DrawablePart.From(new List<CommonTex>()
            {
                new CommonTex(new Vector3(-1, 0, -1), Vector3.UnitY, new Vector2(0,0)),
                new CommonTex(new Vector3(1, 0, -1), Vector3.UnitY, new Vector2(1, 0)),
                new CommonTex(new Vector3(-1, 0, 1), Vector3.UnitY, new Vector2(0, 1)),
                new CommonTex(new Vector3(1, 0, 1), Vector3.UnitY, new Vector2(1, 1))
            });
            BillboardPart.effect = new SpectrumEffect();
        }
        public static Matrix GetBillboardTransform(Quaternion rotation, Vector3 position, Vector2 size)
            => Matrix.CreateScale(size.X, 0, size.Y) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);
    }
}
