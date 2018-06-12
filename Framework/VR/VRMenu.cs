using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectrum.Framework.Graphics;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Content;

namespace Spectrum.Framework.VR
{
    public class VRMenu : GameObject
    {
        public RootElement Root;
        public RenderTarget2D Target;
        public Vector2 Size;
        public Point RenderTargetSize;
        public VRMenu()
        {
            AllowReplicate = false;
            NoCollide = true;
            DrawOrder = 10;
        }
        public override void Initialize()
        {
            base.Initialize();
            Root = new RootElement();
            Root.Target = (Target = new RenderTarget2D(SpectrumGame.Game.GraphicsDevice, RenderTargetSize.X, RenderTargetSize.Y));
        }
        public override void Update(GameTime gameTime)
        {
            Root.Update(gameTime, new Input.InputState(), false);
            Root.Draw(gameTime);
        }
        public override List<RenderTask> GetRenderTasks(RenderPhaseInfo phase)
        {
            return new List<RenderTask>() { Draw3D.Draw3DRectangle(Target,
                Quaternion.Concatenate(Quaternion.CreateFromAxisAngle(Vector3.Right, (float)Math.PI / 2), Quaternion.CreateFromRotationMatrix(orientation)),
                position, Size) };
        }
    }
}
