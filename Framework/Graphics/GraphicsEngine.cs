using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Physics.LinearMath;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Content;
using Valve.VR;
using System.Reflection;
using Spectrum.Framework.VR;
using Spectrum.Framework.Screens;

namespace Spectrum.Framework.Graphics
{
    public class RenderPhaseInfo
    {
        public Matrix View = Matrix.Identity;
        public Matrix Projection = Matrix.Identity;
        public Texture2D ShadowMapSource;
        public Matrix ShadowViewProjection = Matrix.Identity;
        public bool GenerateShadowMap;
    }
    public class GraphicsEngine
    {
        private static GraphicsDevice device;
        public static float darkness = 1f;
        public static bool wireFrame = false;
        public static SpriteBatch spriteBatch;
        public static Color clearColor = Color.CornflowerBlue;
        public static Camera Camera { get; set; }
        public static Camera ShadowCamera { get; set; }
        public static RenderTarget2D shadowMap;

        static RenderTarget2D AATarget;
        static RenderTarget2D VRTargetL;
        static Texture_t textureL;
        static RenderTarget2D VRTargetR;
        static Texture_t textureR;
        static RenderTarget2D DepthTarget;
        static RenderPhaseInfo sceneRenderPhase = new RenderPhaseInfo();
        static RenderPhaseInfo shadowRenderPhase = new RenderPhaseInfo() { GenerateShadowMap = true };
        //static RenderPhaseInfo reflectionRenderPhase = new RenderPhaseInfo();
        static Texture2D phaseShadowMap;
        static FieldInfo textureFieldInfo;
        private static float _msFactor = 1;
        public static int Width { get; private set; }
        public static int Height { get; private set; }
        public static float MultisampleFactor
        {
            get { return _msFactor; }
            set { _msFactor = value; ResetOnResize(Width, Height); }
        }
        public static VertexBuffer lineVBuffer;
        public static IndexBuffer lineIBuffer;
        public static SpectrumEffect lineEffect;

        public static void Initialize()
        {
            textureFieldInfo = typeof(RenderTarget2D).GetField("_texture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            device = SpectrumGame.Game.GraphicsDevice;
            lineEffect = new SpectrumEffect();
            lineVBuffer = VertexHelper.MakeVertexBuffer(new List<CommonTex>() { new CommonTex(Vector3.Zero), new CommonTex(Vector3.Forward) });
            lineIBuffer = VertexHelper.MakeIndexBuffer(new ushort[] { 0, 1 });
            Settings.Init(device);
            PostProcessEffect.Initialize();
            shadowMap = new RenderTarget2D(device, 4096, 4096, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            if (SpecVR.Running)
            {
                uint width = 0, height = 0;
                OpenVR.System.GetRecommendedRenderTargetSize(ref width, ref height);
                VRTargetL = new RenderTarget2D(device, (int)width, (int)height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                SharpDX.Direct3D11.Texture2D nativeTexture = (SharpDX.Direct3D11.Texture2D)textureFieldInfo.GetValue(VRTargetL);
                textureL = new Texture_t() { eType = ETextureType.DirectX, eColorSpace = EColorSpace.Auto, handle = nativeTexture.NativePointer };
                VRTargetR = new RenderTarget2D(device, (int)width, (int)height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                nativeTexture = (SharpDX.Direct3D11.Texture2D)textureFieldInfo.GetValue(VRTargetR);
                textureR = new Texture_t() { eType = ETextureType.DirectX, eColorSpace = EColorSpace.Auto, handle = nativeTexture.NativePointer };
            }
        }
        public static void ResetOnResize(int width, int height)
        {
            Width = width; Height = height;
            if (device != null)
            {
                spriteBatch = new SpriteBatch(device);
                AATarget?.Dispose();
                AATarget = new RenderTarget2D(device, (int)(width * MultisampleFactor), (int)(height * MultisampleFactor), false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                DepthTarget?.Dispose();
                DepthTarget = new RenderTarget2D(device, (int)(width * MultisampleFactor), (int)(height * MultisampleFactor), false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
                PostProcessEffect.DepthTarget = DepthTarget;
                Water.ResetRenderTargets();
                PostProcessEffect.ResetViewPort(width, height);
            }
        }
        public static Vector3 ViewPosition(Vector3 WorldPosition)
        {
            return Vector3.Transform(WorldPosition, Camera.View);
        }
        public static Vector3 ViewToScreenPosition(Vector3 ViewPosition)
        {
            return device.Viewport.Project(ViewPosition, Camera.Projection, Matrix.Identity, Matrix.Identity);
        }
        public static Vector3 FullScreenPos(Vector3 WorldPos)
        {
            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            Vector3 screenPos = device.Viewport.Project(WorldPos, Camera.Projection, Camera.View, world);
            return screenPos;
        }
        public static Vector2 ScreenPos(Vector3 WorldPos)
        {
            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            Vector3 screenPos = device.Viewport.Project(WorldPos, Camera.Projection, Camera.View, world);
            return new Vector2(screenPos.X, screenPos.Y);
        }

        private static void SetBuffers(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, DynamicVertexBuffer instanceBuffer)
        {
            device.SetVertexBuffers(new VertexBufferBinding(vertexBuffer, 0, 0), new VertexBufferBinding(instanceBuffer, 0, 1));
            device.Indices = indexBuffer;
        }
        public static void SetBuffers(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;
        }
        private static void UpdateShadowMap(IEnumerable<RenderCall> renderGroups)
        {
            device.SetRenderTarget(shadowMap);
            device.Clear(Color.Black);
            renderGroups = renderGroups.Where(group => !group.Properties.DisableDepthBuffer);
            shadowRenderPhase.Projection = SpectrumEffect.LightView * Settings.lightProjection;
            phaseShadowMap = null;
            RenderQueue(shadowRenderPhase, renderGroups);
        }
        public static void UpdateWater(List<Entity> scene)
        {
            //device.SetRenderTarget(Water.refractionRenderTarget);
            //GraphicsEngine.device.Clear(clearColor);
            //foreach (GameObject drawable in scene)
            //{
            //    if (drawable.GetType() != typeof(Water))
            //    {
            //        GraphicsEngine.PushDrawable(drawable, new Graphics.RenderTask());
            //    }
            //}
            //RenderQueue(sceneRenderPhase, renderTasks);
            //ClearRenderQueue();
            //device.SetRenderTarget(Water.reflectionRenderTarget);
            //GraphicsEngine.device.Clear(clearColor);
            //SpectrumEffect.Clip = true;
            //SpectrumEffect.ClipPlane = new Vector4(0, 1, 0, -Water.waterHeight);
            //foreach (GameObject drawable in scene)
            //{
            //    if (drawable.GetType() != typeof(Water))
            //    {
            //        GraphicsEngine.PushDrawable(drawable, new Graphics.RenderTask());
            //    }
            //}
            //reflectionRenderPhase.View = Camera.ReflectionView;
            //reflectionRenderPhase.Projection = Camera.ReflectionProjection;
            //RenderQueue(reflectionRenderPhase, renderTasks);
            //ClearRenderQueue();
            //SpectrumEffect.Clip = false;
            //device.SetRenderTarget(null);
        }
        public static void ToggleWireFrame()
        {
            wireFrame = !wireFrame;
            UpdateRasterizer();
        }
        public static void UpdateRasterizer()
        {
            RasterizerState foo = new RasterizerState();
            if (wireFrame)
            {
                foo.FillMode = FillMode.WireFrame;
            }
            else
            {
                foo.FillMode = FillMode.Solid;
            }
            foo.MultiSampleAntiAlias = false;
            foo.CullMode = CullMode.None;
            device.RasterizerState = foo;
            device.BlendState = BlendState.AlphaBlend;
        }
        public static void Render(PrimitiveType primType, VertexBuffer VBuffer, IndexBuffer IBuffer, DynamicVertexBuffer instanceBuffer)
        {
            //Draw vertex component
            if (VBuffer != null)
            {
                if (IBuffer != null)
                {
                    // Instance draws apparently MUST have an IndexBuffer or SharpDX throws an exception
                    if (instanceBuffer != null)
                    {
                        SetBuffers(VBuffer, IBuffer, instanceBuffer);
                        if (primType == PrimitiveType.TriangleStrip)
                            device.DrawInstancedPrimitives(primType, 0, 0, IBuffer.IndexCount - 2, instanceBuffer.VertexCount);
                        if (primType == PrimitiveType.TriangleList)
                            device.DrawInstancedPrimitives(primType, 0, 0, IBuffer.IndexCount / 3, instanceBuffer.VertexCount);
                        if (primType == PrimitiveType.LineList)
                            device.DrawInstancedPrimitives(primType, 0, 0, IBuffer.IndexCount / 2, instanceBuffer.VertexCount);
                        if (primType == PrimitiveType.LineStrip)
                            device.DrawInstancedPrimitives(primType, 0, 0, IBuffer.IndexCount - 1, instanceBuffer.VertexCount);
                    }
                    else
                    {
                        SetBuffers(VBuffer, IBuffer);
                        if (primType == PrimitiveType.TriangleStrip)
                            device.DrawIndexedPrimitives(primType, 0, 0, IBuffer.IndexCount - 2);
                        if (primType == PrimitiveType.TriangleList)
                            device.DrawIndexedPrimitives(primType, 0, 0, IBuffer.IndexCount / 3);
                        if (primType == PrimitiveType.LineList)
                            device.DrawIndexedPrimitives(primType, 0, 0, IBuffer.IndexCount / 2);
                        if (primType == PrimitiveType.LineStrip)
                            device.DrawIndexedPrimitives(primType, 0, 0, IBuffer.IndexCount - 1);
                    }
                }
                else
                {
                    SetBuffers(VBuffer, IBuffer);
                    if (primType == PrimitiveType.TriangleStrip)
                        device.DrawPrimitives(primType, 0, VBuffer.VertexCount - 2);
                    if (primType == PrimitiveType.LineStrip)
                        device.DrawPrimitives(primType, 0, VBuffer.VertexCount - 1);
                }
            }
        }
        private static void Render(RenderCall group, RenderPhaseInfo phase)
        {
            var depthStencil = group.Properties.DisableDepthBuffer ? DepthStencilState.None : DepthStencilState.Default;
            if (device.DepthStencilState != depthStencil)
                device.DepthStencilState = depthStencil;
            phase = phase ?? sceneRenderPhase;
            SpectrumEffect effect = group.Properties.Effect;
            RenderProperties props = group.Properties;
            if (effect != null)
            {
                var technique = (phase.GenerateShadowMap ? "ShadowMap" : "Standard") + (group.InstanceBuffer == null ? "" : "Instance");
                effect.CurrentTechnique = effect.Techniques[technique];
                if (effect.CurrentTechnique != null)
                {
                    effect.View = phase.View;
                    effect.Projection = phase.Projection;
                    effect.ShadowMap = phaseShadowMap;
                    MaterialData material = group.Material ?? MaterialData.Missing;
                    effect.MaterialDiffuse = material.DiffuseColor;
                    effect.Texture = material.DiffuseTexture;
                    using (DebugTiming.Render.Time("Draw Call Time"))
                    {
                        foreach (var pass in effect.CurrentTechnique.Passes)
                        {
                            if (group.InstanceBuffer == null)
                            {
                                foreach (var instance in group.InstanceData)
                                {
                                    effect.World = instance.World;
                                    pass.Apply();
                                    Render(props.PrimitiveType, props.VertexBuffer, props.IndexBuffer, null);
                                }
                            }
                            else
                            {
                                effect.World = Matrix.Identity;
                                // TODO (MAYBE): pass.Apply() is kind of expensive here, before doing fixed render tasks there was some batching over effect
                                // consider maybe doing that again? Could sort render tasks and only pass.Apply() when changing properties
                                pass.Apply();
                                Render(props.PrimitiveType, props.VertexBuffer, props.IndexBuffer, group.InstanceBuffer);
                            }
                        }
                    }
                }
            }
        }

        private static void RenderQueue(RenderPhaseInfo phase, IEnumerable<RenderCall> renderTasks)
        {
            foreach (var group in renderTasks)
            {
                Render(group, phase);
            }
        }

        public static void BeginRender(GameTime gameTime)
        {
            device.DepthStencilState = DepthStencilState.Default;
            device.PresentationParameters.PresentationInterval = PresentInterval.Immediate;
            UpdateRasterizer();
        }
        public static void DrawJBBox(JBBox box, Color color, SpriteBatch spriteBatch)
        {
            Vector3 corner1 = new Vector3(box.Min.X, box.Min.Y, box.Min.Z);
            Vector3 corner2 = new Vector3(box.Min.X, box.Min.Y, box.Max.Z);
            Vector3 corner3 = new Vector3(box.Max.X, box.Min.Y, box.Min.Z);
            Vector3 corner4 = new Vector3(box.Max.X, box.Min.Y, box.Max.Z);
            Vector3 corner5 = new Vector3(box.Min.X, box.Max.Y, box.Min.Z);
            Vector3 corner6 = new Vector3(box.Min.X, box.Max.Y, box.Max.Z);
            Vector3 corner7 = new Vector3(box.Max.X, box.Max.Y, box.Min.Z);
            Vector3 corner8 = new Vector3(box.Max.X, box.Max.Y, box.Max.Z);

            //Bottom
            DrawLine(corner1, corner2, color, spriteBatch);
            DrawLine(corner1, corner3, color, spriteBatch);
            DrawLine(corner4, corner2, color, spriteBatch);
            DrawLine(corner4, corner3, color, spriteBatch);

            //Top
            DrawLine(corner5, corner6, color, spriteBatch);
            DrawLine(corner5, corner7, color, spriteBatch);
            DrawLine(corner8, corner6, color, spriteBatch);
            DrawLine(corner8, corner7, color, spriteBatch);

            //Sides
            DrawLine(corner1, corner5, color, spriteBatch);
            DrawLine(corner2, corner6, color, spriteBatch);
            DrawLine(corner3, corner7, color, spriteBatch);
            DrawLine(corner4, corner8, color, spriteBatch);
        }
        public static void DrawLine(Vector3 P1, Vector3 P2, Color color, SpriteBatch spriteBatch)
        {
            if (FullScreenPos(P1).Z >= 1 || FullScreenPos(P2).Z >= 1) { return; }

            Vector2 start = ScreenPos(P1);
            Vector2 edge = ScreenPos(P2) - start;
            // calculate angle to rotate line
            float angle =
                (float)Math.Atan2(edge.Y, edge.X);
            spriteBatch.Draw(ContentHelper.Blank, start, null, color, angle, Vector2.Zero, new Vector2(edge.Length(), 1.2f), SpriteEffects.None, 0);
        }
        public static void DrawCircle(Vector3 P1, float radius, Color color, SpriteBatch spriteBatch)
        {
            if (FullScreenPos(P1).Z >= 1) { return; }

            Vector2 start = ScreenPos(P1);
            spriteBatch.Draw(ContentHelper.Blank, start, null, color, 0, Vector2.Zero, radius, SpriteEffects.None, 0);
        }
        public static void DrawSprite(Texture2D tex, Vector3 P1, Vector3 P2, Color color, SpriteBatch batch, int height = -1)
        {
            Vector2 start = ScreenPos(P1);
            Vector2 toDraw = start - ScreenPos(P2);
            float length = toDraw.Length();
            Vector2 scale = new Vector2(length / tex.Width, 1);
            if (height != -1)
            {
                scale.Y = 1.0f * height / tex.Width;
            }
            float rotate = (float)Math.Atan((double)(toDraw.Y / toDraw.X));
            if (toDraw.X > 0) { rotate += (float)Math.PI; }
            batch.Draw(tex, start, null, color, rotate,
                new Vector2(0, tex.Height / 2), scale, SpriteEffects.None, 0f);
        }

        private static VRTextureBounds_t bounds = new VRTextureBounds_t() { uMin = 0, uMax = 1f, vMax = 1f, vMin = 0 };
        private static void VRRender(Matrix camera, IEnumerable<RenderCall> groups, EVREye eye, Matrix eye_offset)
        {
            device.Clear(clearColor);
            RenderPhaseInfo vrPhase = new RenderPhaseInfo
            {
                View = camera * eye_offset,
                Projection = OpenVR.System.GetProjectionMatrix(eye, 0.1f, 10000)
            };
            RenderQueue(vrPhase, groups);
        }

        public static void Render(IEnumerable<RenderCall> renderGroups, GameTime gameTime, RenderTarget2D target)
        {
            BeginRender(gameTime);
            WaterEffect.ReflectionView = Camera.ReflectionView;
            WaterEffect.ReflectionProj = Camera.ReflectionProjection;
            SpectrumEffect.CameraPos = Camera.Position;
            WaterEffect.WaterTime += gameTime.ElapsedGameTime.Milliseconds / 20.0f;

            using (DebugTiming.Render.Time("Update Shadow"))
                UpdateShadowMap(renderGroups);

            PostProcessEffect.Technique = "PassThrough";
            //TODO: Draw spritebatch stuff to separate target, and superimpose over game
            //spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, effect: PostProcessEffect.effect);
            //if (Settings.enableWater) { UpdateWater(drawables); }

            //Begin rendering this to the Anti Aliasing texture
            device.SetRenderTargets(AATarget, DepthTarget);
            device.Clear(clearColor);
            device.Clear(ClearOptions.DepthBuffer, Color.Black, 1, 0);
            var mainRenderTimer = DebugTiming.Render.Time("Main Render");
            sceneRenderPhase.View = Camera.View;
            sceneRenderPhase.Projection = Camera.Projection;
            phaseShadowMap = shadowMap;
            if (!SpecVR.Running)
            {
                RenderQueue(sceneRenderPhase, renderGroups);
                //Clear the screen and perform anti aliasing
                device.SetRenderTarget(target);
                using (DebugTiming.Render.Time("Post Process"))
                {
                    device.Clear(clearColor);
                    PostProcessEffect.Technique = "AAPP";
                    spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null, PostProcessEffect.effect);
                    spriteBatch.Draw(AATarget, new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height), Color.White);
                    spriteBatch.End();
                }
            }
            else
            {
                Matrix left_offset = Matrix.Invert(OpenVR.System.GetEyeToHeadTransform(EVREye.Eye_Left));
                Matrix right_offset = Matrix.Invert(OpenVR.System.GetEyeToHeadTransform(EVREye.Eye_Right));
                var view = Camera.View;
                device.SetRenderTarget(VRTargetR);
                VRRender(view, renderGroups, EVREye.Eye_Right, right_offset);
                device.SetRenderTarget(VRTargetL);
                VRRender(view, renderGroups, EVREye.Eye_Left, left_offset);
                OpenVR.Compositor.Submit(EVREye.Eye_Right, ref textureR, ref bounds, EVRSubmitFlags.Submit_Default);
                OpenVR.Compositor.Submit(EVREye.Eye_Left, ref textureL, ref bounds, EVRSubmitFlags.Submit_Default);
                device.SetRenderTarget(target);
                device.Clear(clearColor);
                spriteBatch.Begin(0, BlendState.Opaque, SamplerState.LinearClamp, null, null, PostProcessEffect.effect);
                spriteBatch.Draw(VRTargetR, new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height), Color.White);
                spriteBatch.End();
            }
            mainRenderTimer?.Stop();
        }
    }
}
