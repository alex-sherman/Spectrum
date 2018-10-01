using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Physics.LinearMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Graphics
{
    using RenderDict = DefaultDict<RenderProperties, RenderCall>;
    public class Batch3D
    {
        EntityManager Manger;
        static DrawablePart linePart;
        static Batch3D()
        {
            linePart = DrawablePart.From(new List<CommonTex>()
            {
                new CommonTex(new Vector3(-0.5f, -0.5f, -1), new Vector3(-1, 0, 0), new Vector2(0, 1)),
                new CommonTex(new Vector3(    0,  0.5f, -1), new Vector3(0, 1, 0), new Vector2(1.0f/3, 1)),
                new CommonTex(new Vector3( 0.5f, -0.5f, -1), new Vector3(1, 0, 0), new Vector2(2.0f/3, 1)),
                new CommonTex(new Vector3(-0.5f, -0.5f, 0),  new Vector3(-1, 0, 0), new Vector2(0, 0)),
                new CommonTex(new Vector3(    0,  0.5f, 0),  new Vector3(0, 1, 0), new Vector2(1.0f/3, 0)),
                new CommonTex(new Vector3( 0.5f, -0.5f, 0),  new Vector3(1, 0, 0), new Vector2(2.0f/3, 0)),
            }, new List<uint>() {
                0, 1, 2,
                0, 1, 4,
                0, 4, 3,
                1, 2, 5,
                1, 4, 5,
                2, 0, 3,
                2, 3, 5,
                3, 4, 5
            });
            linePart.primType = PrimitiveType.TriangleList;
        }
        public Batch3D(EntityManager manager)
        {
            Manger = manager;
        }
        private RenderDict fixedBatched = new RenderDict((key) => new RenderCall(key) { InstanceData = new HashSet<InstanceData>() }, true);
        private RenderDict dynamicBatched = new RenderDict((key) => new RenderCall(key) { InstanceData = new HashSet<InstanceData>() }, true);
        private List<RenderCall> dynamicNonBatched = new List<RenderCall>();
        public RenderCallKey RegisterDraw(
            DrawablePart part, Matrix world, MaterialData material = null, SpectrumEffect effect = null,
            bool disableDepthBuffer = false, bool disableShadow = false)
        {
            RenderProperties properties = new RenderProperties(part, material, effect, disableDepthBuffer, disableShadow);
            var value = UpdateRenderDict(properties, world, material ?? part.material, fixedBatched);
            return new RenderCallKey(properties, value);
        }
        public bool UnregisterDraw(RenderCallKey key)
        {
            var call = fixedBatched[key.Properties];
            // TODO: Maybe don't dispose?
            call.InstanceBuffer?.Dispose();
            call.InstanceBuffer = null;
            return call.InstanceData?.Remove(key.Instance) ?? false;
        }
        public void DrawJBBox(JBBox box, Color color, float width = 0.1f)
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
            DrawLine(corner1, corner2, color, width);
            DrawLine(corner1, corner3, color, width);
            DrawLine(corner4, corner2, color, width);
            DrawLine(corner4, corner3, color, width);

            //Top
            DrawLine(corner5, corner6, color, width);
            DrawLine(corner5, corner7, color, width);
            DrawLine(corner8, corner6, color, width);
            DrawLine(corner8, corner7, color, width);

            //Sides
            DrawLine(corner1, corner5, color, width);
            DrawLine(corner2, corner6, color, width);
            DrawLine(corner3, corner7, color, width);
            DrawLine(corner4, corner8, color, width);
        }
        public void DrawLine(Vector3 start, Vector3 end, Color color, float width = 0.1f)
        {
            DrawPart(linePart, Matrix.CreateScale(width, width,
                (end - start).Length()) * MatrixHelper.RotationFromDirection(end - start) * Matrix.CreateTranslation(start),
                new MaterialData() { DiffuseColor = color });
        }
        public void DrawModel(SpecModel model, Matrix world, MaterialData material = null,
            bool disableDepthBuffer = false, bool disableShadow = false, bool disableInstancing = false)
        {
            foreach (var part in model)
            {
                DrawPart(part, world, material, disableDepthBuffer: disableDepthBuffer, disableShadow: disableShadow, disableInstancing: disableInstancing);
            }
        }
        public void DrawPart(DrawablePart part, Matrix world, MaterialData material = null, SpectrumEffect effect = null,
            bool disableDepthBuffer = false, bool disableShadow = false, bool disableInstancing = false)
        {
            RenderProperties properties = new RenderProperties(part, material, effect, disableDepthBuffer, disableShadow);
            material = material ?? part.material;
            world = part.permanentTransform * part.transform * world;
            if (disableInstancing)
                dynamicNonBatched.Add(new RenderCall(properties, world, material));
            else
                UpdateRenderDict(properties, world, material, dynamicBatched);
        }
        private InstanceData UpdateRenderDict(RenderProperties properties, Matrix world, MaterialData material, RenderDict dict)
        {
            var call = dict[properties];
            // TODO: This should get partially instanced?
            call.Material = material;
            var output = new InstanceData() { World = world };
            call.InstanceData.Add(output);
            call.InstanceBuffer?.Dispose();
            call.InstanceBuffer = null;
            return output;
        }
        public void DrawPart(DrawablePart part, DynamicVertexBuffer instanceBuffer,
            MaterialData material = null, SpectrumEffect effect = null,
            bool disableDepthBuffer = false, bool disableShadow = false)
        {
            RenderProperties properties = new RenderProperties(part, material, effect, disableDepthBuffer, disableShadow);
            dynamicNonBatched.Add(new RenderCall(properties) { InstanceBuffer = instanceBuffer, Material = material ?? part.material });
        }
        public IEnumerable<RenderCall> GetRenderTasks(float gameTime)
        {
            var drawables = Manger.Entities.DrawSorted.Where(e => e.DrawEnabled).ToList();
            foreach (Entity drawable in drawables)
            {
                using (DebugTiming.Render.Time(drawable.GetType().Name))
                {
                    using (DebugTiming.Render.Time("Get Tasks"))
                    {
                        drawable.Draw(gameTime);
                    }

                    if (SpectrumGame.Game.DebugDraw)
                    {
                        if (drawable is GameObject gameObject)
                            gameObject.DebugDraw(gameTime);
                    }
                }
            }
            foreach (var group in dynamicBatched.Values)
                group.Squash();
            foreach (var group in fixedBatched.Values.Where(group => group.InstanceBuffer == null))
                group.Squash();
            return fixedBatched.Values.Union(dynamicBatched.Values).Union(dynamicNonBatched);
        }
        public void ClearRenderTasks()
        {
            foreach (var batch in dynamicBatched.Values)
                batch.InstanceBuffer?.Dispose();
            dynamicBatched.Clear();
            dynamicNonBatched.Clear();
        }
    }
}
