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
        public Vector3? HitPosition { get; protected set; }
        public Matrix CameraTransform = Matrix.Identity;
        public KeyBind? ToggleButton;
        public ITransform Attached;
        private ITransform frozen;
        public ITransform UsedTransform => FreezePosition ? frozen : Attached;
        public Transform Offset;
        public ITransform Cursor;
        public VRHand Hand = VRHand.Right;
        public RootElement Root = new RootElement();
        public RenderTarget2D Target;
        public Point RenderTargetSize = new Point(1024, 1024);
        public InputState InputState = new InputState(true);
        public bool FreezePosition = false;
        public override bool DrawEnabled
        {
            get => base.DrawEnabled;
            set
            {
                if (value && FreezePosition)
                {
                    frozen = new Transform(Attached.Position,
                        Quaternion.CreateFromYawPitchRoll((float)Attached.Orientation.Yaw(), (float)Attached.Orientation.Pitch(), 0));
                }
                base.DrawEnabled = value;
                Root.Display = value;
            }
        }
        public VRMenu()
        {
            AllowReplicate = false;
            NoCollide = true;
            Root.OnDisplayChanged += (display) => DrawEnabled = display;
            Root.AddTagsFromType(GetType());
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
            if (UsedTransform != null)
            {
                Position = UsedTransform.Apply(Offset?.Apply(Vector3.Zero) ?? Vector3.Zero);
                Orientation = UsedTransform.Orientation;
                if (Offset != null)
                    Orientation *= Offset.Orientation;
                PhysicsUpdate(dt);
            }
            if (Cursor != null)
                InputState.CursorState = GetCursorState(InputState);
            else
                InputState.CursorState = new CursorState() { X = -1, Y = -1, buttons = new bool[16] };
            Root.Update(dt, InputState);
            Root.Draw(dt);
            Draw(World * CameraTransform);
            if (Cursor != null && HitPosition.HasValue)
            {
                var basePosition = Vector3.Transform(Cursor.Position, CameraTransform);
                Manager.DrawLine(basePosition, Vector3.Transform(HitPosition.Value, CameraTransform), Color.Black);
            }
        }
        public CursorState GetCursorState(InputState input)
        {
            var buttons = new bool[16];
            var camera = GraphicsEngine.Camera;
            int X = -1;
            int Y = -1;
            int ScrollY = 0;
            if (camera != null)
            {
                var _position = Cursor.Position;
                var direction = Vector3.Transform(Vector3.Forward, Cursor.Orientation);
                if (Manager.Physics.CollisionSystem.Raycast(this, _position, direction, out Vector3 normal, out float fraction))
                {
                    var controller = input.VRFromHand(Hand);
                    var lastController = input.Last.VRFromHand(Hand);
                    buttons[0] = input.IsKeyDown(new VRBinding(VRButton.SteamVR_Trigger, Hand));
                    buttons[1] = input.IsKeyDown(new VRBinding(VRButton.SteamVR_Touchpad, Hand));
                    HitPosition = _position + direction * fraction;
                    Vector3 localPos = Vector3.Transform(HitPosition.Value, Matrix.Invert(World));
                    Vector2 cursorPos = new Vector2(localPos.X / Size.X, -localPos.Y / Size.Y) + Vector2.One / 2;
                    X = (int)(cursorPos.X * RenderTargetSize.X);
                    Y = (int)(cursorPos.Y * RenderTargetSize.Y);
                    var touched = new VRBinding(VRButton.SteamVR_Touchpad, Hand, VRPressType.Touched);
                    if (controller.IsButtonPressed(touched) && lastController.IsButtonPressed(touched))
                    {
                        switch (SpecVR.HardwareType)
                        {
                            case VRHardwareType.Vive:
                                ScrollY = (int)((controller.Axis[0].Y - lastController.Axis[0].Y) * 1200);
                                break;
                            default:
                                ScrollY = (int)(controller.Axis[0].Y * 120);
                                break;
                        }
                    }
                }
                else
                    HitPosition = null;
            }
            return new CursorState()
            {
                DX = 0,
                DY = 0,
                buttons = buttons,
                ScrollY = -ScrollY,
                X = X,
                Y = Y,
            };
        }
    }
}
