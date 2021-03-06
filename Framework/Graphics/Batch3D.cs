﻿using Microsoft.Xna.Framework;
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
        public static Batch3D Current { get => Context<Batch3D>.Current; }
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
        private RenderDict fixedBatched = new RenderDict((key) => new RenderCall(key) { InstanceData = new HashSet<InstanceData>() }, true);
        private RenderDict dynamicBatched = new RenderDict((key) => new RenderCall(key) { InstanceData = new HashSet<InstanceData>() }, true);
        private List<RenderCall> dynamicNonBatched = new List<RenderCall>();
        private List<Batch3D> subBatches = new List<Batch3D>();
        public RenderCallKey RegisterDraw(
            DrawablePart part, Matrix world, MaterialData material = null, SpectrumEffect effect = null, DrawOptions options = default)
        {
            RenderProperties properties = new RenderProperties(part, null, material,
                effect, options.DisableDepthBuffer, options.DisableShadow, options.DynamicDraw);
            world = part.Transform * world;
            var value = UpdateRenderDict(properties, world, fixedBatched);
            return new RenderCallKey(properties, value);
        }
        public List<RenderCallKey> RegisterModel(SpecModel model, Matrix world, DrawOptions options = default)
        {
            if (model == null) return new List<RenderCallKey>();
            return model.MeshParts.Values.Select(p => RegisterDraw(p, world, options: options)).ToList();
        }
        public bool UnregisterDraw(RenderCallKey key)
        {
            var call = fixedBatched[key.Properties];
            // TODO: Maybe don't dispose?
            call.InstanceBuffer?.Dispose();
            call.InstanceBuffer = null;
            return call.InstanceData?.Remove(key.Instance) ?? false;
        }
        public bool UnregisterDraws(List<RenderCallKey> keys)
        {
            bool removed = false;
            foreach (var key in keys)
                removed |= UnregisterDraw(key);
            return removed;
        }
        public void DrawJBBox(JBBox box, Color color, float width = 0.02f)
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
        public void DrawLine(Vector3 start, Vector3 end, Color color, float width = 0.02f)
        {
            DrawPart(linePart, Matrix.CreateScale(width, width,
                (end - start).Length) * Matrix.CreateRotationFromDirection(end - start) * Matrix.CreateTranslation(start),
                new MaterialData() { DiffuseColor = color });
        }
        public struct DynamicDrawArgs
        {
            public RenderCall Group;
            public RenderPhaseInfo Phase;
            public Matrix World;
        }
        public struct DrawOptions
        {
            public bool DisableDepthBuffer;
            public bool DisableShadow;
            public bool DisableInstancing;
            public Action<DynamicDrawArgs> DynamicDraw;
        }
        public void DrawModel(SpecModel model, Matrix world, MaterialData material = null,
            DrawOptions options = default)
        {
            foreach (var part in model)
            {
                DrawPart(part, world, material, options: options);
            }
        }
        public void DrawBatch(Batch3D batch) => subBatches.Add(batch);
        public void DrawPart(DrawablePart part, Matrix world, MaterialData material = null, SpectrumEffect effect = null, DrawOptions options = default)
        {
            RenderProperties properties = new RenderProperties(part, null, material, effect,
                options.DisableDepthBuffer, options.DisableShadow, options.DynamicDraw);
            world = part.Transform * world;
            if (options.DisableInstancing)
                dynamicNonBatched.Add(new RenderCall(properties, world));
            else
                UpdateRenderDict(properties, world, dynamicBatched);
        }
        private InstanceData UpdateRenderDict(RenderProperties properties, Matrix world, RenderDict dict)
        {
            var call = dict[properties];
            // TODO: This should get partially instanced?
            var output = new InstanceData() { World = world };
            call.InstanceData.Add(output);
            call.InstanceBuffer?.Dispose();
            call.InstanceBuffer = null;
            return output;
        }
        public void DrawPart(DrawablePart part, DynamicVertexBuffer instanceBuffer,
            Matrix? world = null, MaterialData material = null, SpectrumEffect effect = null,
            DrawOptions options = default)
        {
            RenderProperties properties = new RenderProperties(part, world, material, effect, options.DisableDepthBuffer, options.DisableShadow, options.DynamicDraw);
            dynamicNonBatched.Add(new RenderCall(properties) { InstanceBuffer = instanceBuffer });
        }
        public IEnumerable<RenderCall> GetRenderTasks(float gameTime)
        {
            foreach (var group in dynamicBatched.Values)
                group.Squash();
            foreach (var group in fixedBatched.Values.Where(group => group.InstanceBuffer == null))
                group.Squash();
            return fixedBatched.Values.Union(dynamicBatched.Values).Union(dynamicNonBatched)
                .Union(subBatches.SelectMany(b => b.GetRenderTasks(gameTime)));
        }
        public void ClearRenderTasks()
        {
            foreach (var batch in dynamicBatched.Values)
                batch.InstanceBuffer?.Dispose();
            dynamicBatched.Clear();
            dynamicNonBatched.Clear();
            subBatches.Clear();
        }
    }
}
