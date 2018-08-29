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
using Spectrum.Framework.Input;

namespace Spectrum.Framework.VR
{
    public class VRMenu : Billboard
    {
        public RootElement Root;
        public RenderTarget2D Target;
        public Point RenderTargetSize = new Point(1024, 1024);
        public InputState InputState = new InputState(true);
        public bool Interactive;
        public VRMenu()
        {
            AllowReplicate = false;
            NoCollide = true;
        }
        public override void Initialize()
        {
            base.Initialize();
            Root = new RootElement();
            Texture = Root.Target = (Target = new RenderTarget2D(SpectrumGame.Game.GraphicsDevice, RenderTargetSize.X, RenderTargetSize.Y));
        }
        public override void Update(GameTime gameTime)
        {
            InputState.Update(gameTime.DT());
            if (Interactive)
                InputState.CursorState = GetCursorState(InputState);
            else
                InputState.CursorState = new CursorState() { X = -1, Y = -1, buttons = new bool[16] };
            Root.Update(gameTime, InputState);
        }
        public override void Draw(float gameTime)
        {
            Root.Draw(gameTime);
            base.Draw(gameTime);
            if(InputState.CursorState.X != -1 || InputState.CursorState.Y != -1)
            {
                var basePosition = GraphicsEngine.Camera.Position - InputState.VRHMD.Position + InputState.VRControllers[1].Position;
                Manager.DrawLine(basePosition, basePosition + Vector3.Transform(Vector3.Forward, InputState.VRControllers[1].Rotation), Color.Black);
            }
        }
        public CursorState GetCursorState(InputState input)
        {
            var buttons = new bool[16];
            var camera = GraphicsEngine.Camera;
            int X = -1;
            int Y = -1;
            if (camera != null)
            {
                var position = camera.Position - input.VRHMD.Position + input.VRControllers[1].Position;
                var direction = Vector3.Transform(Vector3.Forward, input.VRControllers[1].Rotation);
                if (Manager.Physics.CollisionSystem.Raycast(this, position, direction, out Vector3 normal, out float fraction))
                {
                    buttons[0] = input.IsKeyDown(new VRBinding(VRButton.BetterTrigger));
                    buttons[1] = input.IsKeyDown(new VRBinding(VRButton.SteamVR_Touchpad));
                    Vector3 localPos = Vector3.Transform(position + direction * fraction, Matrix.Invert(World));
                    Vector2 cursorPos = new Vector2(localPos.X / Size.X, -localPos.Y / Size.Y) + Vector2.One / 2;
                    X = (int)(cursorPos.X * RenderTargetSize.X);
                    Y = (int)(cursorPos.Y * RenderTargetSize.Y);
                }
            }
            return new CursorState()
            {
                DX = 0,
                DY = 0,
                buttons = buttons,
                Scroll = 0,
                X = X,
                Y = Y,
            };
        }
    }
}
