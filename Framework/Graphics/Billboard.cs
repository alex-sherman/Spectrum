using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            DisableInstancing = true;
            IsStatic = true;
            NoCollide = true;
        }
        public override void Initialize()
        {
            base.Initialize();
            Shape = new BoxShape(new Vector3(Size.X, Size.Y, 0));
        }
        public void Draw(Matrix world)
        {
            Manager.DrawPart(
                BillboardPart,
                Matrix.CreateScale(Size.X, Size.Y, 0) * world,
                Material,
                disableDepthBuffer: DisableDepthBuffer,
                disableInstancing: DisableInstancing
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
