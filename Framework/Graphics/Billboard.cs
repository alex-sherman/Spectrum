using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct2D1.Effects;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Input;
using Spectrum.Framework.Physics.Collision.Shapes;
using Spectrum.Framework.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Graphics
{
    public static class Billboard
    {
        public static readonly DrawablePart BillboardPart;
        static Billboard()
        {
            BillboardPart = DrawablePart.From(new List<CommonTex>()
            {
                new CommonTex(new Vector3(-0.5f, -0.5f, 0), Vector3.Backward, new Vector2(0,1)),
                new CommonTex(new Vector3(0.5f, -0.5f, 0), Vector3.Backward, new Vector2(1, 1)),
                new CommonTex(new Vector3(-0.5f,  0.5f, 0), Vector3.Backward, new Vector2(0, 0)),
                new CommonTex(new Vector3(0.5f,  0.5f, 0), Vector3.Backward, new Vector2(1, 0))
            });
        }
        public static void Draw(Matrix world, Vector2 size, MaterialData material)
        {
            Batch3D.Current.DrawPart(
                BillboardPart,
                Matrix.CreateScale(size.X, size.Y, 1) * world,
                material
            );
        }
        public static void Draw(Vector3 position, Vector2 size,
            MaterialData material, Vector3 offset = default)
        {
            Batch3D.Current.DrawPart(
                BillboardPart,
                Matrix.Create(offset, new Vector3(size.X, size.Y, 1)),
                material,
                options: new Batch3D.DrawOptions()
                {
                    DisableInstancing = true,
                    DynamicDraw = (args) =>
                    {
                        args.Group.Properties.Effect.World = args.World *
                            Matrix.CreateRotationY(-args.Phase.View.ToQuaternion().Yaw()) *
                            Matrix.CreateTranslation(position);
                    }
                }
            );
        }
    }
}
