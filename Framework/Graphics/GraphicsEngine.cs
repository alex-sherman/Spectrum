using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework;
using Spectrum.Framework.Physics;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Physics.LinearMath;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Content;
using System.Diagnostics;
using Valve.VR;
using System.Reflection;
using Spectrum.Framework.VR;


namespace Spectrum.Framework.Graphics
{
    using Screens;
    using System.Threading.Tasks;
    using RenderGroup = KeyValuePair<RenderGroupKey, List<RenderTask>>;
    using RenderGroups = IEnumerable<KeyValuePair<RenderGroupKey, List<RenderTask>>>;
    public struct RenderGroupKey
    {
        public RenderGroupKey(RenderTask task)
        {
            effect = task.EffectValue;
            partID = task.part.ReferenceID;
            material = task.MaterialValue;
        }
        public SpectrumEffect effect;
        public int partID;
        public MaterialData material;

        public override bool Equals(object obj)
        {
            if (obj is RenderGroupKey)
            {
                var other = (RenderGroupKey)obj;
                return other.effect == effect && other.partID == partID && other.material == material;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return effect.GetHashCode() + material?.GetHashCode() ?? 0 + partID.GetHashCode();
        }
    }
    public class RenderTask
    {
        public RenderTask(DrawablePart part, string tag = "Misc") { this.part = part; this.tag = tag; }
        public bool AllowInstance = true;
        public DrawablePart part;
        public string tag;
        public MaterialData material;
        public MaterialData MaterialValue { get { return material ?? part.material; } }
        public SpectrumEffect effect;
        public SpectrumEffect EffectValue { get { return effect ?? part.effect; } }
        public List<Matrix> instances = null;
        public DynamicVertexBuffer instanceBuffer;
        public void Merge()
        {
            instanceBuffer = VertexHelper.MakeInstanceBuffer(instances.ToArray());
            merged = true;
        }
        public Matrix InstanceWorld = Matrix.Identity;
        public Matrix? world;
        public Matrix WorldValue
        {
            get { return instanceBuffer != null ? InstanceWorld : (part.permanentTransform * part.transform * (world ?? Matrix.Identity)); }
        }
        public bool merged;
    }
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
        static List<RenderTask> renderTasks = new List<RenderTask>();
        static RenderPhaseInfo sceneRenderPhase = new RenderPhaseInfo();
        static RenderPhaseInfo shadowRenderPhase = new RenderPhaseInfo() { GenerateShadowMap = true };
        static RenderPhaseInfo reflectionRenderPhase = new RenderPhaseInfo();
        static Texture2D phaseShadowMap;
        static FieldInfo textureFieldInfo;
        public static float MultisampleFactor = 2;
        public static void Initialize(Camera camera)
        {
            textureFieldInfo = typeof(RenderTarget2D).GetField("_texture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Camera = camera;
            device = SpectrumGame.Game.GraphicsDevice;
            Settings.Init(device);
            PostProcessEffect.Initialize();
            PostProcessEffect.AAEnabled = true;
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
            if (device != null)
            {
                spriteBatch = new SpriteBatch(device);
                AATarget?.Dispose();
                AATarget = new RenderTarget2D(device, (int)(width * MultisampleFactor), (int)(height * MultisampleFactor), false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                DepthTarget?.Dispose();
                DepthTarget = new RenderTarget2D(device, (int)(width * MultisampleFactor), (int)(height * MultisampleFactor), false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
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
            return device.Viewport.Project(ViewPosition, SceneScreen.Projection, Matrix.Identity, Matrix.Identity);
        }
        public static Vector3 FullScreenPos(Vector3 WorldPos)
        {
            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            Vector3 screenPos = device.Viewport.Project(WorldPos, SceneScreen.Projection, Camera.View, world);
            return screenPos;
        }
        public static Vector2 ScreenPos(Vector3 WorldPos)
        {
            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            Vector3 screenPos = device.Viewport.Project(WorldPos, SceneScreen.Projection, Camera.View, world);
            return new Vector2(screenPos.X, screenPos.Y);
        }
        public static Ray GetCameraRay(Vector2 screenCoords)
        {
            Vector3 nearsource = new Vector3((float)screenCoords.X, (float)screenCoords.Y, 0f);
            Vector3 farsource = new Vector3((float)screenCoords.X, (float)screenCoords.Y, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            Vector3 nearPoint = SpectrumGame.Game.GraphicsDevice.Viewport.Unproject(nearsource, SceneScreen.Projection, Camera.View, world);

            Vector3 farPoint = SpectrumGame.Game.GraphicsDevice.Viewport.Unproject(farsource, SceneScreen.Projection, Camera.View, world);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            return new Ray(nearPoint, direction);
        }

        private static void setBuffers(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, DynamicVertexBuffer instanceBuffer)
        {
            device.SetVertexBuffers(new VertexBufferBinding(vertexBuffer, 0, 0), new VertexBufferBinding(instanceBuffer, 0, 1));
            device.Indices = indexBuffer;
        }
        public static void setBuffers(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;
        }
        public static void UpdateShadowMap(List<Entity> scene)
        {
            device.SetRenderTarget(shadowMap);
            device.Clear(Color.Black);
            renderTasks = scene.Select(drawable => drawable.GetRenderTasks(shadowRenderPhase))
                .Where(tasks => tasks != null)
                .SelectMany(t => t)
                .Where(task => task.part.ShadowEnabled)
            .ToList();
            var renderGroups = GroupTasks(renderTasks);
            shadowRenderPhase.Projection = SpectrumEffect.LightView * Settings.lightProjection;
            phaseShadowMap = null;
            RenderQueue(shadowRenderPhase, renderGroups);
            ClearRenderQueue();
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
                    if (instanceBuffer != null)
                    {
                        setBuffers(VBuffer, IBuffer, instanceBuffer);
                        if (primType == PrimitiveType.TriangleStrip)
                        {
                            device.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, VBuffer.VertexCount, 0, IBuffer.IndexCount - 2, instanceBuffer.VertexCount);
                        }
                        if (primType == PrimitiveType.TriangleList)
                        {
                            device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, VBuffer.VertexCount, 0, IBuffer.IndexCount / 3, instanceBuffer.VertexCount);
                        }
                    }
                    else
                    {
                        setBuffers(VBuffer, IBuffer);
                        if (primType == PrimitiveType.TriangleStrip)
                        {
                            device.DrawIndexedPrimitives(primType, 0, 0, IBuffer.IndexCount - 2);
                        }
                        if (primType == PrimitiveType.TriangleList)
                        {
                            device.DrawIndexedPrimitives(primType, 0, 0, IBuffer.IndexCount / 3);
                        }
                    }
                }
                else
                {
                    setBuffers(VBuffer, IBuffer);
                    if (primType == PrimitiveType.TriangleStrip)
                    {
                        device.DrawPrimitives(primType, 0, VBuffer.VertexCount - 2);
                    }
                }
            }
        }
        /// <summary>
        /// Render a single task, this call does not perform any batching and should be avoided if possible
        /// </summary>
        /// <param name="task"></param>
        /// <param name="phase"></param>
        public static void Render(RenderTask task, RenderPhaseInfo phase = null)
        {
            Render(new RenderGroup(new RenderGroupKey(task), new List<RenderTask>() { task }), phase);
        }
        private static void Render(RenderGroup group, RenderPhaseInfo phase)
        {
            phase = phase ?? sceneRenderPhase;
            RenderGroupKey key = group.Key;
            SpectrumEffect effect = key.effect;
            if (effect != null)
            {
                var technique = (phase.GenerateShadowMap ? "ShadowMap" : "Standard") + (group.Value.First().instanceBuffer == null ? "" : "Instance");
                effect.CurrentTechnique = effect.Techniques[technique];
                if (effect.CurrentTechnique != null)
                {
                    effect.View = phase.View;
                    effect.Projection = phase.Projection;
                    effect.ShadowMap = phaseShadowMap;
                    MaterialData material = key.material ?? MaterialData.Missing;
                    effect.MaterialDiffuse = material.diffuseColor;
                    if (material.diffuseTexture != null)
                        effect.Texture = material.diffuseTexture;
                    var timer = DebugTiming.Render.Time("Draw Call Time");
                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        foreach (var task in group.Value)
                        {
                            effect.World = task.WorldValue;
                            pass.Apply();
                            DrawablePart part = task.part;
                            Render(part.primType, part.VBuffer, part.IBuffer, task.instanceBuffer);
                        }
                    }
                    timer.Stop();
                }
            }
        }
        private static void RenderQueue(RenderPhaseInfo phase, RenderGroups renderTasks)
        {
            foreach (var group in renderTasks)
            {
                Render(group, phase);
            }
        }
        private static void ClearRenderQueue()
        {
            foreach (var task in renderTasks)
            {
                // TODO: Maybe don't create a new instance buffer every frame when merging tasks
                if (task.merged)
                {
                    task.instanceBuffer.Dispose();
                    task.instanceBuffer = null;
                    task.merged = false;
                }
            }
            renderTasks.Clear();
        }

        public static void BeginRender(GameTime gameTime)
        {
            device.DepthStencilState = new DepthStencilState();

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
        private static DefaultDict<RenderGroupKey, List<RenderTask>> groups = new DefaultDict<RenderGroupKey, List<RenderTask>>(() => new List<RenderTask>(), true);
        private static RenderGroups GroupTasks(List<RenderTask> renderTasks)
        {
            var time1 = DebugTiming.Render.Time("Grouping1");
            groups.Clear();
            foreach (var task in renderTasks)
            {
                task.instances = null;
                task.merged = false;
                var group = groups[new RenderGroupKey(task)];
                RenderTask mergeable = null;
                if (task.instanceBuffer == null && (mergeable = group.FirstOrDefault(mergeTask => mergeTask.part.ReferenceID == task.part.ReferenceID)) != null)
                {
                    if (mergeable.instances == null)
                    {
                        mergeable.instances = new List<Matrix>() { mergeable.WorldValue };
                    }
                    mergeable.instances.Add(task.WorldValue);
                }
                else
                    group.Add(task);
            }
            foreach (var task in groups.SelectMany(group => group.Value))
            {
                if (task.instances != null)
                {
                    task.Merge();
                }
            }
            time1.Stop();
            return groups;
        }
        private static VRTextureBounds_t bounds = new VRTextureBounds_t() { uMin = 0, uMax = 1f, vMax = 1f, vMin = 0 };
        private static void VRRender(Matrix camera, RenderGroups groups, EVREye eye, Matrix eye_offset)
        {
            GraphicsEngine.device.Clear(clearColor);
            RenderPhaseInfo vrPhase = new RenderPhaseInfo();
            vrPhase.View = camera * eye_offset;
            vrPhase.Projection = OpenVR.System.GetProjectionMatrix(eye, 0.1f, 10000);
            RenderQueue(vrPhase, groups);
        }

        public static void Render(List<Entity> drawables, GameTime gameTime, RenderTarget2D target)
        {
            BeginRender(gameTime);
            WaterEffect.ReflectionView = Camera.ReflectionView;
            WaterEffect.ReflectionProj = Camera.ReflectionProjection;
            SpectrumEffect.CameraPos = Camera.Position;
            WaterEffect.WaterTime += gameTime.ElapsedGameTime.Milliseconds / 20.0f;
            drawables = drawables.Where(e => e.Enabled).ToList();

            var preRenderTime = DebugTiming.Render.Time("Update Shadow");
            UpdateShadowMap(drawables);
            preRenderTime.Stop();

            PostProcessEffect.Technique = "PassThrough";
            //TODO: Draw spritebatch stuff to separate target, and superimpose over game
            //spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, effect: PostProcessEffect.effect);
            if (Settings.enableWater) { UpdateWater(drawables); }

            //Begin rendering this to the Anti Aliasing texture
            device.SetRenderTargets(AATarget, DepthTarget);
            GraphicsEngine.device.Clear(clearColor);
            device.Clear(ClearOptions.DepthBuffer, Color.Black, 1, 0);
            TimingResult timer;
            foreach (Entity drawable in drawables)
            {
                timer = DebugTiming.Render.Time(drawable.GetType().Name);
                drawable.Draw(gameTime, null);
                var getRenderTasksTimer = DebugTiming.Render.Time("Get Tasks");
                var tasks = drawable.GetRenderTasks(sceneRenderPhase);
                getRenderTasksTimer.Stop();
                if (tasks != null)
                    renderTasks.AddRange(tasks);
                timer.Stop();
            }
            var mainRenderTimer = DebugTiming.Render.Time("Main Render");

            var renderGroups = GroupTasks(renderTasks);
            sceneRenderPhase.View = Camera.View;
            sceneRenderPhase.Projection = SceneScreen.Projection;
            phaseShadowMap = shadowMap;
            if (!SpecVR.Running)
            {
                RenderQueue(sceneRenderPhase, renderGroups);
                //Clear the screen and perform anti aliasing
                device.SetRenderTarget(target);
                timer = DebugTiming.Render.Time("Post Process");
                device.Clear(clearColor);
                PostProcessEffect.Technique = "AAPP";
                spriteBatch.Begin(0, BlendState.Opaque, SamplerState.LinearClamp, null, null, PostProcessEffect.effect);
                spriteBatch.Draw(AATarget, new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height), Color.White);
                spriteBatch.End();
                timer.Stop();
            }
            else
            {
                Matrix left_offset = Matrix.Invert(SpecVR.HeadPose) * Matrix.Invert(OpenVR.System.GetEyeToHeadTransform(EVREye.Eye_Left));
                Matrix right_offset = Matrix.Invert(SpecVR.HeadPose) * Matrix.Invert(OpenVR.System.GetEyeToHeadTransform(EVREye.Eye_Right));
                var view = Camera.View;
                device.SetRenderTarget(VRTargetR);
                VRRender(view, renderGroups, EVREye.Eye_Right, right_offset);
                device.SetRenderTarget(VRTargetL);
                VRRender(view, renderGroups, EVREye.Eye_Left, left_offset);
                OpenVR.Compositor.Submit(EVREye.Eye_Left, ref textureL, ref bounds, EVRSubmitFlags.Submit_Default);
                OpenVR.Compositor.Submit(EVREye.Eye_Right, ref textureR, ref bounds, EVRSubmitFlags.Submit_Default);
                device.SetRenderTarget(target);
                device.Clear(clearColor);
                spriteBatch.Begin(0, BlendState.Opaque, SamplerState.LinearClamp, null, null, PostProcessEffect.effect);
                spriteBatch.Draw(VRTargetR, new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height), Color.White);
                spriteBatch.End();
            }
            ClearRenderQueue();
            mainRenderTimer.Stop();
        }
    }
}
