﻿using System;
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
using System.Threading.Tasks;

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
        static FieldInfo textureFieldInfo;
        private static float _msFactor = 1;
        public static int Width { get; private set; }
        public static int Height { get; private set; }
        public static float MultisampleFactor
        {
            get { return _msFactor; }
            set { _msFactor = value; if (Width != 0 && Height != 0) ResetOnResize(Width, Height); }
        }
        public static VertexBuffer lineVBuffer;
        public static IndexBuffer lineIBuffer;
        public static SpectrumEffect lineEffect;

        public static void Initialize()
        {
            textureFieldInfo = typeof(RenderTarget2D).GetField("_texture", BindingFlags.Instance | BindingFlags.NonPublic);
            device = SpectrumGame.Game.GraphicsDevice;
            lineEffect = new SpectrumEffect();
            lineVBuffer = VertexHelper.MakeVertexBuffer(new List<CommonTex>() { new CommonTex(Vector3.Zero), new CommonTex(Vector3.Forward) });
            lineIBuffer = VertexHelper.MakeIndexBuffer(new ushort[] { 0, 1 });
            Settings.Init(device);
            PostProcessEffect.Initialize();
            shadowMap = new RenderTarget2D(device, 4096, 4096, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            if (SpecVR.Running)
                GenerateVRTargets();
        }
        static void GenerateVRTargets()
        {
            uint width = 0, height = 0;
            OpenVR.System.GetRecommendedRenderTargetSize(ref width, ref height);
            width = (uint)(width * MultisampleFactor);
            height = (uint)(height * MultisampleFactor);
            VRTargetL = new RenderTarget2D(device, (int)width, (int)height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            SharpDX.Direct3D11.Texture2D nativeTexture = (SharpDX.Direct3D11.Texture2D)textureFieldInfo.GetValue(VRTargetL);
            textureL = new Texture_t() { eType = ETextureType.DirectX, eColorSpace = EColorSpace.Auto, handle = nativeTexture.NativePointer };
            VRTargetR = new RenderTarget2D(device, (int)width, (int)height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            nativeTexture = (SharpDX.Direct3D11.Texture2D)textureFieldInfo.GetValue(VRTargetR);
            textureR = new Texture_t() { eType = ETextureType.DirectX, eColorSpace = EColorSpace.Auto, handle = nativeTexture.NativePointer };
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
                if (SpecVR.Running)
                    GenerateVRTargets();
            }
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
            if (instanceBuffer != null)
                device.SetVertexBuffers(new VertexBufferBinding(vertexBuffer, 0, 0), new VertexBufferBinding(instanceBuffer, 0, 1));
            else
                device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;
        }
        private static void UpdateShadowMap(IEnumerable<RenderCall> renderGroups)
        {
            device.SetRenderTarget(shadowMap);
            device.Clear(Color.Black);
            renderGroups = renderGroups.Where(group => !group.Properties.DisableDepthBuffer);
            shadowRenderPhase.Projection = SpectrumEffect.LightView * Settings.lightProjection;
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
            device.RasterizerState = new RasterizerState()
            {
                FillMode = wireFrame ? FillMode.WireFrame : FillMode.Solid,
                MultiSampleAntiAlias = false,
                CullMode = CullMode.None,
            };
            device.BlendState = BlendState.AlphaBlend;
        }
        public static void Render(PrimitiveType primType, VertexBuffer VBuffer, IndexBuffer IBuffer, DynamicVertexBuffer instanceBuffer)
        {
            SetBuffers(VBuffer, IBuffer, instanceBuffer);
            //Draw vertex component
            if (VBuffer != null)
            {
                // Instance draws apparently MUST have an IndexBuffer or SharpDX throws an exception
                if (IBuffer != null)
                {
                    if (instanceBuffer != null)
                    {
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
                    // TODO: Fix shadow map
                    effect.ShadowMap = null;
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

        private static VRTextureBounds_t bounds = new VRTextureBounds_t() { uMin = 0, uMax = 1f, vMax = 1f, vMin = 0 };
        private static void VRRender(Matrix camera, IEnumerable<RenderCall> groups, EVREye eye, Matrix eye_offset, RenderTarget2D target)
        {
            device.SetRenderTarget(target);
            device.Clear(clearColor);
            RenderPhaseInfo vrPhase = new RenderPhaseInfo
            {
                View = camera * eye_offset,
                Projection = OpenVR.System.GetProjectionMatrix(eye, 0.1f, 10000)
            };
            RenderQueue(vrPhase, groups);
        }

        public static void BeginRender(Camera camera)
        {
            device.DepthStencilState = DepthStencilState.Default;
            // Disables frame rate limiting
            device.PresentationParameters.PresentationInterval = PresentInterval.Immediate;
            UpdateRasterizer();
            SpectrumEffect.CameraPos = camera.Position;
        }

        /// <summary>
        /// Renders the provided groups to the render target with no post processing
        /// </summary>
        public static void RenderSimple(Camera camera, IEnumerable<RenderCall> renderGroups, RenderTarget2D target)
        {
            BeginRender(camera);
            device.SetRenderTarget(target);
            device.Clear(clearColor);
            RenderPhaseInfo simplePhase = new RenderPhaseInfo
            {
                View = camera.View,
                Projection = camera.Projection,
            };
            RenderQueue(simplePhase, renderGroups);
        }

        /// <summary>
        /// Renders the provided groups to the render target with post processing requiring
        /// that the post process targets be configured accordingly.
        /// </summary>
        public static void RenderScene(Camera camera, IEnumerable<RenderCall> renderGroups, RenderTarget2D target)
        {
            BeginRender(camera);

            using (DebugTiming.Render.Time("Update Shadow"))
                UpdateShadowMap(renderGroups);

            //Begin rendering this to the Anti Aliasing texture
            var mainRenderTimer = DebugTiming.Render.Time("Main Render");
            sceneRenderPhase.View = camera.View;
            sceneRenderPhase.Projection = camera.Projection;
            device.SetRenderTargets(AATarget, DepthTarget);
            device.Clear(clearColor);
            device.Clear(ClearOptions.DepthBuffer, Color.Black, 1, 0);
            RenderQueue(sceneRenderPhase, renderGroups);
            //Clear the screen and perform anti aliasing
            device.SetRenderTarget(target);
            using (DebugTiming.Render.Time("Post Process"))
            {
                device.Clear(clearColor);
                PostProcessEffect.Technique = "AAPP";
                spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null, PostProcessEffect.effect);
                spriteBatch.Draw(AATarget, new Rectangle(0, 0, target.Width, target.Height), Color.White);
                spriteBatch.End();
            }
            mainRenderTimer?.Stop();
        }
        static float vrRenderFraction = 0.25f;
        public static void RenderVRScene(Camera camera, IEnumerable<RenderCall> renderGroups, RenderTarget2D target)
        {
            BeginRender(camera);

            var vrRenderTimer = DebugTiming.Render.Time("VR Render");
            Matrix left_offset = Matrix.Invert(OpenVR.System.GetEyeToHeadTransform(EVREye.Eye_Left));
            Matrix right_offset = Matrix.Invert(OpenVR.System.GetEyeToHeadTransform(EVREye.Eye_Right));
            VRRender(camera.View, renderGroups, EVREye.Eye_Right, right_offset, VRTargetR);
            VRRender(camera.View, renderGroups, EVREye.Eye_Left, left_offset, VRTargetL);
            if (target != null)
            {
                device.SetRenderTarget(target);
                device.Clear(clearColor);
                spriteBatch.Begin(0, BlendState.Opaque, SamplerState.LinearClamp, null, null, PostProcessEffect.effect);
                var widthCut = (int)(VRTargetR.Width * vrRenderFraction);
                var heightCut = (int)(VRTargetR.Height * vrRenderFraction);
                spriteBatch.Draw(VRTargetR, new Rectangle(0, 0, target.Width, target.Height),
                    new Rectangle(widthCut / 2, heightCut / 2, VRTargetR.Width - widthCut, VRTargetR.Height - heightCut), Color.White);
                spriteBatch.End();
            }
            vrRenderTimer?.Stop();
        }
        public static void EndDraw()
        {
            if (SpecVR.Running)
            {
                using (DebugTiming.Render.Time("VR Submit"))
                {
                    var error = OpenVR.Compositor.Submit(EVREye.Eye_Right, ref textureR, ref bounds, EVRSubmitFlags.Submit_Default);
                    error = OpenVR.Compositor.Submit(EVREye.Eye_Left, ref textureL, ref bounds, EVRSubmitFlags.Submit_Default);
                }
            }
        }
    }
}
