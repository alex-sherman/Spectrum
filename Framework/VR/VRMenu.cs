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
        public KeyBind? ToggleButton;
        public ITransform Attached;
        public Vector3 Offset = Vector3.Forward * 2;
        public ITransform Cursor;
        public RootElement Root = new RootElement();
        public RenderTarget2D Target;
        public Point RenderTargetSize = new Point(1024, 1024);
        public InputState InputState = new InputState(true);
        public override bool DrawEnabled
        {
            get => base.DrawEnabled; set
            {
                base.DrawEnabled = value;
                Root.Display = value;
            }
        }
        public VRMenu()
        {
            AllowReplicate = false;
            NoCollide = true;
        }
        public override void Initialize()
        {
            base.Initialize();
            Texture = Root.Target = (Target = new RenderTarget2D(SpectrumGame.Game.GraphicsDevice, RenderTargetSize.X, RenderTargetSize.Y));
        }
        public override void Update(float dt)
        {
            InputState.Update(dt);
            if (ToggleButton.HasValue && InputState.IsNewKeyPress(ToggleButton.Value))
                DrawEnabled ^= true;
        }
        public override void Draw(float dt)
        {
            if (Attached != null)
            {
                Position = Attached.Position + Vector3.Transform(Offset, Attached.Orientation);
                Orientation = Attached.Orientation;
            }
            if (Cursor != null)
                InputState.CursorState = GetCursorState(InputState);
            else
                InputState.CursorState = new CursorState() { X = -1, Y = -1, buttons = new bool[16] };
            Root.Update(dt, InputState);
            Root.Draw(dt);
            base.Draw(dt);
            if (Cursor != null && hitPosition.HasValue)
            {
                var basePosition = Cursor.Position;
                Manager.DrawLine(basePosition, hitPosition.Value, Color.Black);
            }
        }
        protected Vector3? hitPosition;
        public CursorState GetCursorState(InputState input)
        {
            var buttons = new bool[16];
            var camera = GraphicsEngine.Camera;
            int X = -1;
            int Y = -1;
            if (camera != null)
            {
                var _position = Cursor.Position;
                var direction = Vector3.Transform(Vector3.Forward, Cursor.Orientation);
                if (Manager.Physics.CollisionSystem.Raycast(this, _position, direction, out Vector3 normal, out float fraction))
                {
                    buttons[0] = input.IsKeyDown(new VRBinding(VRButton.BetterTrigger, VRHand.Right));
                    buttons[1] = input.IsKeyDown(new VRBinding(VRButton.SteamVR_Touchpad, VRHand.Right));
                    hitPosition = _position + direction * fraction;
                    Vector3 localPos = Vector3.Transform(hitPosition.Value, Matrix.Invert(World));
                    Vector2 cursorPos = new Vector2(localPos.X / Size.X, -localPos.Y / Size.Y) + Vector2.One / 2;
                    X = (int)(cursorPos.X * RenderTargetSize.X);
                    Y = (int)(cursorPos.Y * RenderTargetSize.Y);
                }
                else
                    hitPosition = null;
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
