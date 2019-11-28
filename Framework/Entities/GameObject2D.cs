using Microsoft.Xna.Framework;
using Replicate;
using Spectrum.Framework.Content;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Screens;
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
            }, new List<uint>() { 0, 1, 2, 1, 2, 3 });
            GameObject2DPart.effect = new SpectrumEffect() { LightingEnabled = false };
        }
        public ImageAsset Texture;
        public ElementColor Color = Microsoft.Xna.Framework.Color.White;
        public Rectangle Bounds;
        public Vector2 Position;
        public float Layer;
        public Matrix World => Matrix.CreateRotationZ(Rotation) * Matrix.CreateTranslation(Position.X, Position.Y, Layer);
        public float Rotation = 0;
        public override void Draw(float gameTime)
        {
            base.Draw(gameTime);
            if (Texture != null)
            {
                Batch3D.Current.DrawPart(GameObject2DPart, CreateTexTransform(Bounds) * World, new MaterialData() { DiffuseTexture = Texture.GetTexture(Bounds), DiffuseColor = Color });
            }
        }

        public static Matrix CreateTexTransform(Rectangle rect)
        {
            return Matrix.CreateScale(rect.Width, rect.Height, 0) * Matrix.CreateTranslation(rect.X, rect.Y, 0);
        }
    }
}
