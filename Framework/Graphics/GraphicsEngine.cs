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


namespace Spectrum.Framework.Graphics
{
    public class GraphicsEngine
    {
        private static GraphicsDevice device;
        public static float darkness = 1f;
        public static bool wireFrame = false;
        public static SpriteBatch spriteBatch;
        public static Color clearColor = Color.CornflowerBlue;
        public static Camera Camera { get; private set; }
        private static RenderTarget2D shadowMap;
        private static RenderTarget2D AATarget;
        //TODO: Add a settings thing for multisample count
        public static void Initialize(Camera camera)
        {
            Camera = camera;
            GraphicsEngine.device = SpectrumGame.Game.GraphicsDevice;
            Settings.Init(device);
            PostProcessEffect.Initialize();
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
                Water.ResetRenderTargets();
                PostProcessEffect.ResetViewPort();
            }
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
        public static void UpdateShadowMap(List<GameObject> scene)
        {
            device.SetRenderTarget(shadowMap);
            GraphicsEngine.device.Clear(Color.White);
            foreach (GameObject drawable in scene)
            {
                Render(Camera.View, Camera.Projection, drawable, true);
            }
            device.SetRenderTarget(null);
            PostProcessEffect.ShadowMap = shadowMap;
        }
        public static void setBuffers(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;
        }
        public static void UpdateWater(List<GameObject> scene, GameTime time, SpriteBatch batch)
        {
            Color transparentClear = clearColor;
            device.SetRenderTarget(Water.refractionRenderTarget);
            GraphicsEngine.device.Clear(transparentClear);
            foreach (GameObject drawable in scene)
            {
                if (drawable.GetType() != typeof(Water))
                {
                    drawable.Draw(time, batch, true);
                    Render(Camera.View, Camera.ReflectionProjection, drawable, false);
                }
            }
            device.SetRenderTarget(Water.reflectionRenderTarget);
            GraphicsEngine.device.Clear(transparentClear);
            SpectrumEffect.Clip = true;
            SpectrumEffect.ClipPlane = new Vector4(0, 1, 0, -Water.waterHeight);
            foreach (GameObject drawable in scene)
            {
                if (drawable.GetType() != typeof(Water))
                {
                    drawable.Draw(time, batch, true);
                    Render(Camera.ReflectionView, Camera.ReflectionProjection, drawable, false);
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
            device.BlendState = BlendState.AlphaBlend;
            foo.MultiSampleAntiAlias = true;
            foo.CullMode = CullMode.None;
            device.RasterizerState = foo;
            DepthStencilState poo = new DepthStencilState();
            device.DepthStencilState = poo;
        }
        public static void Render(Matrix View, Matrix world, Matrix projection, DrawablePart part)
        {
            if (part.effect != null)
            {
                part.effect.View = View;
                part.effect.Projection = projection;
                part.effect.World = part.transform * world;
                //Draw vertex component
                if (part.VBuffer != null)
                {
                    if (part.IBuffer != null)
                    {
                        setBuffers(part.VBuffer, part.IBuffer);
                        part.effect.CurrentTechnique.Passes[0].Apply();
                        if (part.primType == PrimitiveType.TriangleStrip)
                        {
                            device.DrawIndexedPrimitives(part.primType, 0, 0,
                                                                 part.VBuffer.VertexCount, 0, part.IBuffer.IndexCount - 2);
                        }
                        if (part.primType == PrimitiveType.TriangleList)
                        {
                            device.DrawIndexedPrimitives(part.primType, 0, 0,
                                                                 part.VBuffer.VertexCount, 0, part.IBuffer.IndexCount / 3);
                        }
                    }
                    else
                    {
                        setBuffers(part.VBuffer, part.IBuffer);
                        part.effect.CurrentTechnique.Passes[0].Apply();
                        if (part.primType == PrimitiveType.TriangleStrip)
                        {
                            device.DrawPrimitives(part.primType, 0, part.VBuffer.VertexCount - 2);
                        }
                    }
                }
            }
        }
        static void Render(Matrix View, Matrix Projection, GameObject drawable, bool DoShadowMap)
        {
            if (drawable != null && drawable.Parts != null)
            {
                device.DepthStencilState = drawable.DepthStencil;
                foreach (DrawablePart part in drawable.Parts)
                {
                    //if (drawable.Model != null && (part.effect as CustomSkinnedEffect) != null)
                    //{
                    //    (part.effect as CustomSkinnedEffect).Bones = drawable.Model.AnimationPlayer.GetSkinTransforms();
                    //}
                    GraphicsEngine.Render(View, drawable.World, Projection, part);
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
        public static void Render(List<GameObject> drawables, GameTime gameTime)
        {
            BeginRender(gameTime);
            WaterEffect.ReflectionView = Camera.ReflectionView;
            WaterEffect.ReflectionProj = Camera.ReflectionProjection;
            SpectrumEffect.CameraPos = Camera.Position;
            SpectrumEffect.AboveWater = Camera.Position.Y > Water.waterHeight;
            WaterEffect.WaterTime += gameTime.ElapsedGameTime.Milliseconds / 20.0f;


            //if (PostProcessEffect.ShadowMapEnabled)
            //{
            //    PostProcessEffect.LightViewProj = Matrix.CreateLookAt(SpectrumEffect.LightPos, Player.LocalPlayer.Position, Vector3.Up) * Settings.lightProjection;
            //    UpdateShadowMap(drawables);
            //}
            spriteBatch.Begin();
            if (Settings.enableWater) { UpdateWater(drawables, gameTime, spriteBatch); }
            //Begin rendering this to the Anti Aliasing texture
            device.SetRenderTarget(AATarget);
            GraphicsEngine.device.Clear(clearColor);
            foreach (GameObject drawable in drawables)
            {
                drawable.Draw(gameTime, spriteBatch, false);
                Render(Camera.View, Camera.Projection, drawable, false);
            }
            spriteBatch.End();
            //Clear the screen and perform anti aliasing
            device.SetRenderTarget(null);
            GraphicsEngine.device.Clear(clearColor);
            PostProcessEffect.Technique = "AAPP";
            spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null, PostProcessEffect.effect);
            spriteBatch.Draw(AATarget, new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height), Color.White);
            spriteBatch.End();
        }
    }
}
