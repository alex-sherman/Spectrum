using Microsoft.Xna.Framework;
using Replicate;
using Spectrum.Framework.Content;
using Spectrum.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Entities
{
    public class GameObject2D : Entity
    {
        public static readonly DrawablePart GameObject2DPart;
        static GameObject2D()
        {
            GameObject2DPart = DrawablePart.From(new List<CommonTex>()
            {
                new CommonTex(new Vector3(0, 1, 0), Vector3.UnitZ, new Vector2(0, 0)),
                new CommonTex(new Vector3(1, 1, 0), Vector3.UnitZ, new Vector2(1, 0)),
                new CommonTex(new Vector3(0, 0, 0), Vector3.UnitZ, new Vector2(0, 1)),
                new CommonTex(new Vector3(1, 0, 0), Vector3.UnitZ, new Vector2(1, 1))
            });
            GameObject2DPart.effect = new SpectrumEffect() { LightingEnabled = false };
        }
        public ImageAsset Texture;
        public Rectangle Bounds;
        public Vector2 Position;
        public Matrix World => Matrix.CreateScale(Bounds.Width, Bounds.Height, 0) * Matrix.CreateTranslation(Bounds.X, Bounds.Y, 0)
            * Matrix.CreateRotationZ(Rotation) * Matrix.CreateTranslation(Position.X, Position.Y, 0);
        public float Rotation = 0;
        public override void Draw(float gameTime)
        {
            base.Draw(gameTime);
            if (Texture != null)
            {
                Batch3D.Current.DrawPart(GameObject2DPart, World, new MaterialData() { DiffuseTexture = Texture.GetTexture(Bounds) });
            }
        }
    }
}
