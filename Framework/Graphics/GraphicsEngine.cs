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
    public class GraphicsEngine
    {
        private static GraphicsDevice device;
        public static float darkness = 1f;
        public static bool wireFrame = false;
        public static SpriteBatch spriteBatch;
        public static Color clearColor = Color.CornflowerBlue;
        public static Camera Camera { get; set; }
        private static RenderTarget2D shadowMap;
        private static RenderTarget2D AATarget;
        private static RenderTarget2D DepthTarget;
        private static Stopwatch timer;
        public static Dictionary<string, long> renderTimes = new Dictionary<string, long>();
        //TODO: Add a settings thing for multisample count
        public static void Initialize(Camera camera)
        {
            timer = new Stopwatch();
            Camera = camera;
            GraphicsEngine.device = SpectrumGame.Game.GraphicsDevice;
            Settings.Init(device);
            PostProcessEffect.Initialize();
            PostProcessEffect.AAEnabled = true;
            shadowMap = new RenderTarget2D(device, 2048, 2048, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
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
        public static void UpdateDepthBuffer(List<GameObject> scene)
        {
            device.SetRenderTarget(DepthTarget);
            GraphicsEngine.device.Clear(clearColor);
            PostProcessEffect.effect.CurrentTechnique = PostProcessEffect.effect.Techniques["ShadowMap"];
            foreach (GameObject drawable in scene)
            {
                if (drawable.GetType() != typeof(Water))
                {
                    GraphicsEngine.Render(drawable, Camera.View, Camera.Projection, PostProcessEffect.effect);
                }
            }
            PostProcessEffect.effect.CurrentTechnique = PostProcessEffect.effect.Techniques["AAPP"];
        }
        public static void UpdateWater(List<GameObject> scene)
        {
            device.SetRenderTarget(Water.refractionRenderTarget);
            GraphicsEngine.device.Clear(clearColor);
            foreach (GameObject drawable in scene)
            {
                if (drawable.GetType() != typeof(Water))
                {
                    GraphicsEngine.Render(drawable, Camera.View, Camera.ReflectionProjection);
                }
            }
            device.SetRenderTarget(Water.reflectionRenderTarget);
            GraphicsEngine.device.Clear(clearColor);
            SpectrumEffect.Clip = true;
            SpectrumEffect.ClipPlane = new Vector4(0, 1, 0, -Water.waterHeight);
            foreach (GameObject drawable in scene)
            {
                if (drawable.GetType() != typeof(Water))
                {
                    GraphicsEngine.Render(drawable, Camera.ReflectionView, Camera.ReflectionProjection);
                }
            }
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
        public static void Render(DrawablePart part, SpectrumEffect effect, Matrix world, Matrix? View = null, Matrix? projection = null)
        {
            if (effect != null)
            {
                effect.View = View ?? Camera.View;
                effect.Projection = projection ?? Camera.Projection;
                effect.World = part.transform * world;
                //Draw vertex component
                if (part.VBuffer != null)
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
        public static void Render(GameObject drawable, Matrix View, Matrix Projection, SpectrumEffect effect = null)
        {
            if (drawable != null && drawable.Parts != null)
            {
                foreach (DrawablePart part in drawable.Parts)
                {
                    SpectrumEffect partEffect = effect ?? part.effect;
                    if (partEffect != null)
                        GraphicsEngine.Render(part, partEffect, drawable.World, View, Projection);
                }
            }
        }

        public static void BeginRender(GameTime gameTime)
        {
            device.DepthStencilState = new DepthStencilState();
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
            renderTimes.Clear();
            BeginRender(gameTime);
            WaterEffect.ReflectionView = Camera.ReflectionView;
            WaterEffect.ReflectionProj = Camera.ReflectionProjection;
            SpectrumEffect.CameraPos = Camera.Position;
            SpectrumEffect.AboveWater = Camera.Position.Y > Water.waterHeight;
            WaterEffect.WaterTime += gameTime.ElapsedGameTime.Milliseconds / 20.0f;
            drawables = drawables.Where(e => e.DrawEnabled).ToList();
            List<GameObject> drawable3D = drawables.Where(e => e is GameObject).Cast<GameObject>().ToList();


            //if (PostProcessEffect.ShadowMapEnabled)
            //{
            //    PostProcessEffect.LightViewProj = Matrix.CreateLookAt(SpectrumEffect.LightPos, Player.LocalPlayer.Position, Vector3.Up) * Settings.lightProjection;
            //    UpdateShadowMap(drawables);
            //}
            PostProcessEffect.Technique = "PassThrough";
            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, effect: PostProcessEffect.effect);
            if (Settings.enableWater) { UpdateWater(drawable3D); }

            //Begin rendering this to the Anti Aliasing texture
            device.SetRenderTargets(AATarget, DepthTarget);
            GraphicsEngine.device.Clear(clearColor);
            foreach (Entity drawable in drawables)
            {
                timer.Restart();
                drawable.Draw(gameTime, spriteBatch);
                if (drawable is GameObject)
                    GraphicsEngine.Render(drawable as GameObject, Camera.View, Camera.Projection);
                timer.Stop();
                string itemName = drawable.GetType().Name;
                DebugPrinter.time("render", itemName, timer.Elapsed.Ticks);
            }
            spriteBatch.End();
            //Clear the screen and perform anti aliasing
            device.SetRenderTarget(null);
            timer.Restart();
            GraphicsEngine.device.Clear(clearColor);
            PostProcessEffect.Technique = "AAPP";
            spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null, PostProcessEffect.effect);
            spriteBatch.Draw(AATarget, new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height), Color.White);
            spriteBatch.End();
            timer.Stop();
            renderTimes["Post Process"] = timer.ElapsedTicks;
        }
    }
}
