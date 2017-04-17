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


namespace Spectrum.Framework.Graphics
{
    public struct RenderTaskArgs
    {
        public string tag;
        public float? z;
        public SpectrumEffect effect;
        public Matrix? world;
        public Matrix? view;
        public Matrix? projection;
        public Texture2D shadowMap;
        public Matrix? shadowViewProjection;
    }
    public class GraphicsEngine
    {
        private class RenderTask
        {
            public RenderTask(DrawablePart part, RenderTaskArgs args)
            {
                this.part = part;
                world = part.permanentTransform * part.transform * (args.world ?? Matrix.Identity);
                z = args.z ?? FullScreenPos(Vector4.Transform(Vector4.UnitW, world).Homogeneous()).Z;
                tag = args.tag ?? "Misc";
                effect = args.effect ?? part.effect;
                view = args.view ?? Camera.View;
                projection = args.projection ?? Camera.Projection;
                shadowMap = args.shadowMap;
                shadowViewProjection = args.shadowViewProjection ?? Matrix.Identity;
            }
            public string tag;
            public float z;
            public DrawablePart part;
            public SpectrumEffect effect;
            public Matrix world;
            public Matrix view;
            public Matrix projection;
            public Texture2D shadowMap;
            public Matrix shadowViewProjection;
        }
        private static GraphicsDevice device;
        public static float darkness = 1f;
        public static bool wireFrame = false;
        public static SpriteBatch spriteBatch;
        public static Color clearColor = Color.CornflowerBlue;
        public static Camera Camera { get; set; }
        public static Camera ShadowCamera { get; set; }
        public static RenderTarget2D shadowMap;
        private static RenderTarget2D AATarget;
        private static RenderTarget2D DepthTarget;
        private static List<RenderTask> renderTasks = new List<RenderTask>();
        //TODO: Add a settings thing for multisample count
        public static void Initialize(Camera camera)
        {
            Camera = camera;
            GraphicsEngine.device = SpectrumGame.Game.GraphicsDevice;
            Settings.Init(device);
            PostProcessEffect.Initialize();
            PostProcessEffect.AAEnabled = true;
            shadowMap = new RenderTarget2D(device, 4096, 4096, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            ResetOnResize(SpectrumGame.Game, EventArgs.Empty);
            SpectrumGame.Game.OnScreenResize += ResetOnResize;
        }
        public static void ResetOnResize(object sender, EventArgs args)
        {
            if (device != null)
            {
                spriteBatch = new SpriteBatch(device);
                AATarget = new RenderTarget2D(GraphicsEngine.device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                DepthTarget = new RenderTarget2D(GraphicsEngine.device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                PostProcessEffect.DepthTarget = DepthTarget;
                Water.ResetRenderTargets();
                PostProcessEffect.ResetViewPort();
            }
        }
        public static Vector3 ViewPosition(Vector3 WorldPosition)
        {
            return Vector3.Transform(WorldPosition, Camera.View);
        }
        public static Vector3 ViewToScreenPosition(Vector3 ViewPosition)
        {
            return device.Viewport.Project(ViewPosition, Settings.projection, Matrix.Identity, Matrix.Identity);
        }
        public static Vector3 FullScreenPos(Vector3 WorldPos)
        {
            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            Vector3 screenPos = GraphicsEngine.device.Viewport.Project(WorldPos, Settings.projection, Camera.View, world);
            return screenPos;
        }
        public static Vector2 ScreenPos(Vector3 WorldPos)
        {
            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            Vector3 screenPos = GraphicsEngine.device.Viewport.Project(WorldPos, Settings.projection, Camera.View, world);
            return new Vector2(screenPos.X, screenPos.Y);
        }
        public static Ray GetCameraRay(Vector2 screenCoords)
        {
            Vector3 nearsource = new Vector3((float)screenCoords.X, (float)screenCoords.Y, 0f);
            Vector3 farsource = new Vector3((float)screenCoords.X, (float)screenCoords.Y, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            Vector3 nearPoint = SpectrumGame.Game.GraphicsDevice.Viewport.Unproject(nearsource, Settings.projection, Camera.View, world);

            Vector3 farPoint = SpectrumGame.Game.GraphicsDevice.Viewport.Unproject(farsource, Settings.projection, Camera.View, world);

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
        private static SpectrumEffect shadowEffect = new SpectrumEffect();
        public static void UpdateShadowMap(List<GameObject> scene)
        {
            device.SetRenderTarget(shadowMap);
            GraphicsEngine.device.Clear(Color.Black);
            shadowEffect.CurrentTechnique = shadowEffect.Techniques["ShadowMap"];
            foreach (GameObject drawable in scene)
            {
                if (drawable.Parts == null) continue;
                foreach (DrawablePart part in drawable.Parts.Where((p) => p.ShadowEnabled))
                {
                    QueuePart(part, new RenderTaskArgs()
                    {
                        world = drawable.World,
                        view = Matrix.Identity,
                        projection = SpectrumEffect.LightView * Settings.lightProjection,
                        tag = "Shadow",
                        effect = shadowEffect
                    });
                }
            }
            RenderQueue();
            device.SetRenderTarget(null);
        }
        public static void UpdateWater(List<GameObject> scene)
        {
            device.SetRenderTarget(Water.refractionRenderTarget);
            GraphicsEngine.device.Clear(clearColor);
            foreach (GameObject drawable in scene)
            {
                if (drawable.GetType() != typeof(Water))
                {
                    GraphicsEngine.PushDrawable(drawable, new RenderTaskArgs() { view = Camera.View, projection = Camera.ReflectionProjection });
                }
            }
            RenderQueue();
            device.SetRenderTarget(Water.reflectionRenderTarget);
            GraphicsEngine.device.Clear(clearColor);
            SpectrumEffect.Clip = true;
            SpectrumEffect.ClipPlane = new Vector4(0, 1, 0, -Water.waterHeight);
            foreach (GameObject drawable in scene)
            {
                if (drawable.GetType() != typeof(Water))
                {
                    GraphicsEngine.PushDrawable(drawable, new RenderTaskArgs() { view = Camera.ReflectionView, projection = Camera.ReflectionProjection });
                }
            }
            RenderQueue();
            SpectrumEffect.Clip = false;
            device.SetRenderTarget(null);
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
        private static void Render(RenderTask task)
        {
            DrawablePart part = task.part;
            SpectrumEffect effect = task.effect;
            Matrix view = task.view;
            Matrix projection = task.projection;
            if (task.effect != null)
            {
                MaterialData material = part.material ?? MaterialData.Missing;
                effect.MaterialDiffuse = material.diffuseColor;
                effect.View = view;
                effect.Projection = projection;
                effect.World = task.world;
                effect.ShadowMap = task.shadowMap;
                //Draw vertex component
                if (effect.CurrentTechnique != null && part.VBuffer != null)
                {
                    if (part.IBuffer != null)
                    {
                        ///TODO: Upgrade monogame to a version that supports hardware instancing
                        if (part.InstanceBuffer != null)
                        {
                            setBuffers(part.VBuffer, part.IBuffer, part.InstanceBuffer);
                            foreach (var pass in effect.CurrentTechnique.Passes)
                            {
                                pass.Apply();
                                if (part.primType == PrimitiveType.TriangleStrip)
                                {
                                    device.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, part.VBuffer.VertexCount, 0, part.IBuffer.IndexCount - 2, part.InstanceBuffer.VertexCount);
                                }
                                if (part.primType == PrimitiveType.TriangleList)
                                {
                                    device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, part.VBuffer.VertexCount, 0, part.IBuffer.IndexCount / 3, part.InstanceBuffer.VertexCount);
                                }
                            }
                        }
                        else
                        {
                            setBuffers(part.VBuffer, part.IBuffer);
                            foreach (var pass in effect.CurrentTechnique.Passes)
                            {
                                pass.Apply();
                                if (part.primType == PrimitiveType.TriangleStrip)
                                {
                                    device.DrawIndexedPrimitives(part.primType, 0, 0, part.IBuffer.IndexCount - 2);
                                }
                                if (part.primType == PrimitiveType.TriangleList)
                                {
                                    device.DrawIndexedPrimitives(part.primType, 0, 0, part.IBuffer.IndexCount / 3);
                                }
                            }
                        }
                    }
                    else
                    {
                        setBuffers(part.VBuffer, part.IBuffer);
                        foreach (var pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            if (part.primType == PrimitiveType.TriangleStrip)
                            {
                                device.DrawPrimitives(part.primType, 0, part.VBuffer.VertexCount - 2);
                            }
                        }
                    }
                }
            }
        }
        public static void QueuePart(DrawablePart part, Matrix World)
        {
            QueuePart(part, new RenderTaskArgs() { world = World });
        }
        public static void QueuePart(DrawablePart part, RenderTaskArgs args)
        {
            RenderTask task = new RenderTask(part, args);
            if (task.effect != null)
            {
                renderTasks.Add(task);
            }
        }
        public static void QueueParts(IEnumerable<DrawablePart> parts, RenderTaskArgs args)
        {
            foreach (DrawablePart part in parts)
            {
                QueuePart(part, args);
            }
        }
        public static void PushDrawable(GameObject drawable, RenderTaskArgs args = default(RenderTaskArgs))
        {
            args.tag = drawable.GetType().Name;
            args.world = drawable.World;
            if (drawable != null && drawable.Parts != null)
            {
                QueueParts(drawable.Parts, args);
            }
        }
        private static void RenderQueue()
        {
            foreach (var task in renderTasks)
            {
                var timer = DebugTiming.Render.Time("Render " + task.tag);
                Render(task);
                timer.Stop();
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
        public static void Render(List<Entity> drawables, GameTime gameTime)
        {
            BeginRender(gameTime);
            WaterEffect.ReflectionView = Camera.ReflectionView;
            WaterEffect.ReflectionProj = Camera.ReflectionProjection;
            SpectrumEffect.CameraPos = Camera.Position;
            SpectrumEffect.AboveWater = Camera.Position.Y > Water.waterHeight;
            WaterEffect.WaterTime += gameTime.ElapsedGameTime.Milliseconds / 20.0f;
            drawables = drawables.Where(e => e.DrawEnabled).ToList();
            List<GameObject> drawable3D = drawables.Where(e => e is GameObject).Cast<GameObject>().ToList();

            UpdateShadowMap(drawable3D);

            PostProcessEffect.Technique = "PassThrough";
            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, effect: PostProcessEffect.effect);
            if (Settings.enableWater) { UpdateWater(drawable3D); }

            //Begin rendering this to the Anti Aliasing texture
            device.SetRenderTargets(AATarget, DepthTarget);
            GraphicsEngine.device.Clear(clearColor);
            TimingResult timer;
            foreach (Entity drawable in drawables)
            {
                timer = DebugTiming.Render.Time(drawable.GetType().Name);
                drawable.Draw(gameTime, spriteBatch);
                if (drawable is GameObject)
                    GraphicsEngine.PushDrawable(drawable as GameObject, new RenderTaskArgs() { shadowMap = shadowMap });
                timer.Stop();
            }
            renderTasks.Sort((a, b) =>
                a.effect.HasTransparency && b.effect.HasTransparency ? Math.Sign(b.z - a.z) : (a.effect.HasTransparency ? 1 : -1) - (b.effect.HasTransparency ? 1 : -1));
            RenderQueue();
            timer = DebugTiming.Render.Time("Sprite batch end");
            spriteBatch.End();
            timer.Stop();
            //Clear the screen and perform anti aliasing
            device.SetRenderTarget(null);
            timer = DebugTiming.Render.Time("Post Process");
            GraphicsEngine.device.Clear(clearColor);
            PostProcessEffect.Technique = "AAPP";
            spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null, PostProcessEffect.effect);
            spriteBatch.Draw(AATarget, new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height), Color.White);
            spriteBatch.End();
            timer.Stop();
        }
    }
}
