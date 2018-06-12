using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Graphics
{
    public static class Draw3D
    {
        static readonly DrawablePart quad;
        static Draw3D()
        {
            quad = DrawablePart.From(new List<CommonTex>()
            {
                new CommonTex(new Vector3(-1, 0, -1), Vector3.UnitY, new Vector2(0,0)),
                new CommonTex(new Vector3(1, 0, -1), Vector3.UnitY, new Vector2(1, 0)),
                new CommonTex(new Vector3(-1, 0, 1), Vector3.UnitY, new Vector2(0, 1)),
                new CommonTex(new Vector3(1, 0, 1), Vector3.UnitY, new Vector2(1, 1))
            });
            quad.effect = new SpectrumEffect();
        }
        public static RenderTask Draw3DRectangle(Texture2D texture, Quaternion rotation, Vector3 position, Vector2 size)
        {
            RenderTask output = new RenderTask(quad);
            output.world = Matrix.CreateScale(size.X, 0, size.Y) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);

            output.material = new MaterialData() { diffuseTexture = texture };
            output.AllowInstance = false;
            return output;
        }
    }
}
