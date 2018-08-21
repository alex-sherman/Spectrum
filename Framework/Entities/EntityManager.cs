using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spectrum.Framework.Physics;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Graphics;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Network;
using System.Collections;
using Spectrum.Framework.Network.Surrogates;
using Spectrum.Framework.Physics.Collision;
using Spectrum.Framework.Physics.LinearMath;

namespace Spectrum.Framework.Entities
{
    using RenderDict = DefaultDict<RenderProperties, RenderCall>;
    public delegate void EntityMessageHandler(NetID peerID, Entity entity, NetMessage message);
    public class EntityManager : IEnumerable<Entity>
    {
        public event Action<Entity> OnEntityAdded;
        public event Action<Entity> OnEntityRemoved;
        private DefaultDict<string, HashSet<Entity>> initDataLookup = new DefaultDict<string, HashSet<Entity>>(() => new HashSet<Entity>(), true);
        private float tickTenthTimer = 100;
        private float tickOneTimer = 1000;
        public EntityCollection Entities;
        private MultiplayerService mpService;
        public readonly PhysicsEngine Physics;
        public bool Paused = false;
        public EntityManager(MultiplayerService mpService)
        {
            Entities = new EntityCollection();
            Physics = new PhysicsEngine(new CollisionSystemPersistentSAP());
            OnEntityAdded += (entity) => { if (entity is GameObject go) Physics.AddBody(go); };
            OnEntityRemoved += (entity) => { if (entity is GameObject go) Physics.RemoveBody(go); };
            this.mpService = mpService;
            RegisterCallbacks();
        }
        private void RegisterCallbacks()
        {
            HandshakeHandler entitySender = new HandshakeHandler(
                    delegate (NetID peerGuid, NetMessage message)
                    {
                        IEnumerable<Entity> replicateable = Entities.UpdateSorted.Where(x => x.AllowReplicate && x.OwnerGuid != peerGuid);
                        message.Write(replicateable.Count());
                        foreach (Entity entity in replicateable)
                        {
                            message.Write(entity.InitData);
                        }
                        foreach (Entity entity in replicateable)
                        {
                            entity.ReplicationData?.WriteReplicationData(message);
                        }
                    },
                    delegate (NetID peerGuid, NetMessage message)
                    {
                        int count = message.Read<int>();
                        Entity[] entities = new Entity[count];
                        for (int i = 0; i < count; i++)
                        {
                            entities[i] = HandleEntityCreation(peerGuid, message);
                        }
                        for (int i = 0; i < count; i++)
                        {
                            entities[i].ReplicationData?.ReadReplicationData(message);
                        }
                    }
                );
            Handshake.RegisterHandshakeHandler(HandshakeStage.PartialResponse, entitySender);
            Handshake.RegisterHandshakeHandler(HandshakeStage.Completed, entitySender);
            mpService.RegisterMessageCallback(FrameworkMessages.EntityCreation, (peer, message) => HandleEntityCreation(peer, message));
            mpService.RegisterMessageCallback(FrameworkMessages.ShowCreate, HandleShowCreate);
            mpService.RegisterMessageCallback(FrameworkMessages.EntityReplication, HandleEntityReplication);
        }

        #region Network Functions

        public void SendEntityCreation(Entity entity, NetID peerDestination = default(NetID))
        {
            if (!entity.AllowReplicate) { return; }

            NetMessage eData = new NetMessage();
            eData.Write(entity.InitData);
            mpService.SendMessage(FrameworkMessages.EntityCreation, eData);

            SendEntityReplication(entity, peerDestination);
        }
        public Entity HandleEntityCreation(NetID peerGuid, NetMessage message)
        {
            InitData entityData = message.Read<InitData>();
            Guid id = (Guid)entityData.Fields["ID"].Object;
            if (!Entities.Map.ContainsKey(id))
                return CreateEntity(entityData);
            else
                return Entities.Map[id];
        }

        public void RequestShowCreate(Guid entityID, NetID peerDestination = default(NetID))
        {
            NetMessage message = new NetMessage();
            message.Write(entityID);
            mpService.SendMessage(FrameworkMessages.ShowCreate, message, peerDestination);
        }
        public void HandleShowCreate(NetID peerGuid, NetMessage message)
        {
            Guid entityID = message.Read<Guid>();
            if (!Entities.Map.ContainsKey(entityID)) { return; }
            Entity entity = Entities.Map[entityID];
            SendEntityCreation(entity, peerGuid);
        }
        public void SendFunctionReplication(Entity entity, string method, params object[] args)
        {
            NetMessage replicationMessage = new NetMessage();
            replicationMessage.Write(entity.ID);
            replicationMessage.Write(0);
            replicationMessage.Write(method);
            replicationMessage.Write(args.Select(obj => new Primitive(obj)).ToArray());
            mpService.SendMessage(FrameworkMessages.EntityReplication, replicationMessage);
        }
        public void SendEntityReplication(Entity entity, NetID peer)
        {
            NetMessage replicationMessage = new NetMessage();
            replicationMessage.Write(entity.ID);
            replicationMessage.Write(1);
            entity.ReplicationData?.WriteReplicationData(replicationMessage);
            mpService.SendMessage(FrameworkMessages.EntityReplication, replicationMessage);
        }
        public void HandleEntityReplication(NetID peerGuid, NetMessage message)
        {
            Guid entityID = message.Read<Guid>();
            if (Entities.Map.ContainsKey(entityID))
            {
                int type = message.Read<int>();
                Entity entity = Entities.Map[entityID];
                if (type == 0)
                {
                    string method = message.Read<string>();
                    Primitive[] args = message.Read<Primitive[]>();
                    entity.ReplicationData.HandleRPC(method, args.Select(prim => prim.Object).ToArray());
                }
                else if (type == 1)
                {
                    entity.ReplicationData?.ReadReplicationData(message);
                }
            }
            else
            {
                RequestShowCreate(entityID, peerGuid);
            }
        }
        #endregion

        public void Update(GameTime gameTime)
        {
            if (Paused) { return; }
            using (DebugTiming.Main.Time("Physics"))
                Physics.Update(gameTime);

            tickTenthTimer += gameTime.ElapsedGameTime.Milliseconds;
            tickOneTimer += gameTime.ElapsedGameTime.Milliseconds;

            using (DebugTiming.Main.Time("Entity Update"))
            {
                List<Entity> updateables = Entities.UpdateSorted;
                for (int i = 0; i < updateables.Count; i++)
                {
                    using (DebugTiming.Update.Time(updateables[i].GetType().Name))
                    {
                        if (updateables[i].Enabled)
                        {
                            updateables[i].Update(gameTime);
                            if (tickOneTimer >= 1000)
                                updateables[i].TickOne();
                            if (tickTenthTimer >= 100)
                                updateables[i].TickTenth();
                        }
                        else
                            updateables[i].DisabledUpdate(gameTime);
                        if (updateables[i].Destroying)
                        {
                            updateables[i].CallOnDestroy();
                            Remove(updateables[i].ID);
                        }
                    }
                }
            }
            if (tickOneTimer >= 1000) tickOneTimer = 0;
            if (tickTenthTimer >= 100) tickTenthTimer = 0;
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
        public void DrawJBBox(JBBox box, Color color, Matrix? world = null)
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
            DrawLine(corner1, corner2, color);
            DrawLine(corner1, corner3, color);
            DrawLine(corner4, corner2, color);
            DrawLine(corner4, corner3, color);

            //Top
            DrawLine(corner5, corner6, color);
            DrawLine(corner5, corner7, color);
            DrawLine(corner8, corner6, color);
            DrawLine(corner8, corner7, color);

            //Sides
            DrawLine(corner1, corner5, color);
            DrawLine(corner2, corner6, color);
            DrawLine(corner3, corner7, color);
            DrawLine(corner4, corner8, color);
        }
        public void DrawLine(Vector3 start, Vector3 end, Color color, Matrix? world = null)
        {
            var properties = new RenderProperties(PrimitiveType.LineStrip, GraphicsEngine.lineVBuffer, GraphicsEngine.lineIBuffer, GraphicsEngine.lineEffect);
            properties.Material = new MaterialData() { DiffuseColor = color };
            var diff = end - start;
            var World = (world ?? Matrix.Identity) * Matrix.CreateScale(diff.Length()) * MatrixHelper.RotationFromDirection(diff) * Matrix.CreateTranslation(start);
            //dynamicNonBatched.Add(new RenderCall(properties, World, new MaterialData() { DiffuseColor = color }));
            UpdateRenderDict(properties, World, properties.Material, dynamicBatched);
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
            var drawables = Entities.DrawSorted.Where(e => e.DrawEnabled).ToList();
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
        public T Create<T>(params object[] args) where T : Entity
        {
            T output = CreateEntity(new InitData(typeof(T).Name, args)) as T;
            return output;
        }
        public T CreateEntity<T>(InitData<T> data) where T : Entity
        {
            return CreateEntity((InitData)data) as T;
        }
        public Entity CreateEntity(InitData data)
        {
            Entity e = Construct(data);
            AddEntity(e);
            return e;
        }
        public Entity CreateEntityType(string typeName)
        {
            return CreateEntity(new InitData(typeName));
        }
        public Entity CreatePrefab(string prefab)
        {
            return CreateEntity(InitData.Prefabs[prefab]);
        }
        /// <summary>
        /// Calls the constructor for the Entity, but does not initialize it or add it to the EntityManager
        /// </summary>
        /// <param name="entityData">Creation data for the Entity</param>
        /// <returns>An initialized</returns>
        public Entity Construct(InitData entityData)
        {
            if (entityData.TypeData == null) { throw new ArgumentException(String.Format("Replication occured for a class {0} not found as a loadable type.", entityData.TypeName)); }

            entityData = entityData.Clone();
            if (!entityData.Fields.ContainsKey("OwnerGuid"))
                entityData.Set("OwnerGuid", mpService.ID);
            if (!entityData.Fields.ContainsKey("ID"))
                entityData.Set("ID", Guid.NewGuid());

            Entity output = entityData.Construct() as Entity;
            return output;
        }
        public Entity AddEntity(Entity entity)
        {
            entity.Manager = this;
            Entities.Add(entity);
            entity.Initialize();
            if (mpService.ID == entity.OwnerGuid)
                SendEntityCreation(entity);
            OnEntityAdded?.Invoke(entity);
            if (entity.InitData.Name != null)
                initDataLookup[entity.InitData.Name].Add(entity);
            return entity;
        }
        public Entity Remove(Guid ID)
        {
            Entity removed = Entities.Remove(ID);
            if (removed == null) return null;
            OnEntityRemoved?.Invoke(removed);
            if (removed.InitData.Name != null)
                initDataLookup[removed.InitData.Name].Remove(removed);
            return removed;
        }
        public void ClearEntities(Func<Entity, bool> predicate = null)
        {
            foreach (Entity entity in Entities.UpdateSorted)
            {
                if (predicate == null || predicate(entity))
                    Remove(entity.ID);
            }
        }

        public Entity Find(Guid id)
        {
            if (Entities.Map.ContainsKey(id))
                return Entities.Map[id];
            return null;
        }

        public IEnumerable<T> FindAll<T>()
        {
            return Entities.Map.Values.Where(e => e is T).Cast<T>();
        }

        public IEnumerable<Entity> FindByPrefab(string prefab)
        {
            return initDataLookup[prefab];
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return Entities.UpdateSorted.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
