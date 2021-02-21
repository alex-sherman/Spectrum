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
    public class Billboard : GameObject
    {
        public static readonly DrawablePart BillboardPart;
        static Billboard()
        {
            BillboardPart = DrawablePart.From(new List<CommonTex>()
            {
                new CommonTex(new Vector3(-0.5f, -0.5f, 0), Vector3.UnitY, new Vector2(0,1)),
                new CommonTex(new Vector3(0.5f, -0.5f, 0), Vector3.UnitY, new Vector2(1, 1)),
                new CommonTex(new Vector3(-0.5f,  0.5f, 0), Vector3.UnitY, new Vector2(0, 0)),
                new CommonTex(new Vector3(0.5f,  0.5f, 0), Vector3.UnitY, new Vector2(1, 0))
            });
            BillboardPart.effect = new SpectrumEffect() { LightingEnabled = false };
        }
        public Vector2 Size = Vector2.One;
        public Billboard()
        {
            DrawOptions.DisableInstancing = true;
            isStatic = true;
            NoCollide = true;
        }
        public override void Initialize()
        {
            base.Initialize();
            Shape = new BoxShape(new Vector3(Size.X, Size.Y, 0));
        }
        public void Draw(Matrix world)
        {
            Batch3D.Current.DrawPart(
                BillboardPart,
                Matrix.CreateScale(Size.X, Size.Y, 0) * world,
                Material,
                options: DrawOptions
            );
        }
        public static void DrawBillboard(Vector3 position, Vector2 size, MaterialData material, Vector3 offset = default)
        {
            Batch3D.Current.DrawPart(
                BillboardPart,
                Matrix.Create(offset, new Vector3(size.X, size.Y, 0)),
                material,
                options: new Batch3D.DrawOptions()
                {
                    DisableInstancing = true,
                    DynamicDraw = (args) =>
                    {
                        var view = args.Phase.View;
                        args.Group.Properties.Effect.World = args.World *
                            Matrix.CreateRotationY(-view.ToQuaternion().Yaw()) *
                            Matrix.CreateTranslation(position);
                    }

                }
            );
        }
        public override void Draw(float gameTime)
        {
            base.Draw(gameTime);
            if (Material != null)
                Draw(World);
        }
    }
}
