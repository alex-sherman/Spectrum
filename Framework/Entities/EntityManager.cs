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
using Spectrum.Framework.Input;
using System.Linq.Expressions;

namespace Spectrum.Framework.Entities
{
    public delegate void EntityMessageHandler(NetID peerID, Entity entity, NetMessage message);
    public class EntityManager
    {
        public static EntityManager Current => Context<EntityManager>.Current;
        private DefaultDict<string, HashSet<Entity>> initDataLookup = new DefaultDict<string, HashSet<Entity>>(() => new HashSet<Entity>(), true);
        private float tickTenthTimer = 100;
        private float tickOneTimer = 1000;
        public EntityCollection Entities = new EntityCollection();
        public readonly PhysicsEngine Physics = new PhysicsEngine(new CollisionSystemPersistentSAP());
        public bool Paused = false;
        public EntityManager()
        {
            Entities.OnEntityAdded += (entity) => { if (entity is GameObject go) Physics.AddBody(go); };
            Entities.OnEntityRemoved += (entity) =>
            {
                if (entity is GameObject go)
                    Physics.RemoveBody(go);
            };
        }
        public void RegisterCallbacks()
        {
            HandshakeHandler entitySender = new HandshakeHandler(
                    delegate (NetID peerGuid, NetMessage message)
                    {
                        IEnumerable<Entity> replicateable = Entities.UpdateSorted
                            .Where(x => x.AllowReplicate && x.OwnerGuid != peerGuid).ToList();
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
                            entities[i] = HandleEntityCreation(peerGuid, message);
                        for (int i = 0; i < count; i++)
                            entities[i].ReplicationData?.ReadReplicationData(message);
                    }
                );
            Handshake.RegisterHandshakeHandler(HandshakeStage.PartialResponse, entitySender);
            Handshake.RegisterHandshakeHandler(HandshakeStage.Completed, entitySender);
            MultiplayerService.Current.RegisterMessageCallback(FrameworkMessages.EntityCreation, (peer, message) => HandleEntityCreation(peer, message));
            MultiplayerService.Current.RegisterMessageCallback(FrameworkMessages.ShowCreate, HandleShowCreate);
            MultiplayerService.Current.RegisterMessageCallback(FrameworkMessages.EntityReplication, HandleEntityReplication);
        }

        #region Network Functions

        public void SendEntityCreation(Entity entity, NetID peerDestination = default(NetID))
        {
            if (!entity.AllowReplicate) { return; }

            NetMessage eData = new NetMessage();
            eData.Write(entity.InitData);
            //mpService.SendMessage(FrameworkMessages.EntityCreation, eData);

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
            //mpService.SendMessage(FrameworkMessages.ShowCreate, message, peerDestination);
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
            //mpService.SendMessage(FrameworkMessages.EntityReplication, replicationMessage);
        }
        public void SendEntityReplication(Entity entity, NetID peer)
        {
            NetMessage replicationMessage = new NetMessage();
            replicationMessage.Write(entity.ID);
            replicationMessage.Write(1);
            entity.ReplicationData?.WriteReplicationData(replicationMessage);
            MultiplayerService.Current.SendMessage(FrameworkMessages.EntityReplication, replicationMessage);
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
        public void Update(float gameTime)
        {
            if (Paused) return;
            Step(gameTime);
        }
        public void Step(float gameTime)
        {
            var fullUpdateables = Entities.UpdateSorted.Where(e => e.Enabled && e is IFullUpdate).Cast<IFullUpdate>().ToList();
            var updateables = Entities.UpdateSorted.Where(e => e.Enabled).ToList();
            using (DebugTiming.Main.Time("Physics"))
                Physics.Update(gameTime);

            tickTenthTimer += gameTime;
            tickOneTimer += gameTime;

            using (DebugTiming.Main.Time("Entity Update"))
            {
                fullUpdateables.ForEach(e => e.PreStep(gameTime));
                foreach (var updateable in Entities.UpdateSorted.ToList())
                {
                    using (DebugTiming.Update.Time(updateable.GetType().Name))
                    {
                        if (updateable.Enabled)
                        {
                            updateable.Update(gameTime);
                            if (tickOneTimer >= 1)
                                updateable.TickOne();
                            if (tickTenthTimer >= .1f)
                                updateable.TickTenth();
                        }
                        else
                            updateable.DisabledUpdate(gameTime);
                    }
                }
                fullUpdateables.ForEach(e => e.PostStep(gameTime));
                foreach (var destroy in Entities.Destroying.ToList())
                {
                    destroy.CallOnDestroy();
                    Remove(destroy.ID);
                }
            }
            if (tickOneTimer >= 1) tickOneTimer = 0;
            if (tickTenthTimer >= .1f) tickTenthTimer = 0;
        }
        public void Draw(float gameTime)
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
        }
        public T Create<T>(Expression<Func<T>> exp) where T : Entity => CreateEntity<T>(new InitData<T>(exp));
        [Obsolete]
        public T Create<T>(params object[] args) where T : Entity
        {
            return CreateEntity(InitData.Get<T>().SetArgs(args)) as T;
        }
        public T CreateEntity<T>(InitData<T> data) where T : Entity
        {
            return CreateEntity((InitData)data) as T;
        }
        public Entity CreateEntity(InitData data)
        {
            Entity e = Construct(data);
            if (e != null)
                AddEntity(e);
            return e;
        }
        /// <summary>
        /// Calls the constructor for the Entity, but does not initialize it or add it to the EntityManager
        /// </summary>
        /// <param name="entityData">Creation data for the Entity</param>
        /// <returns>An initialized</returns>
        public Entity Construct(InitData entityData)
        {
            if (entityData.TypeData == null) { throw new ArgumentException(string.Format("Replication occured for a class {0} not found as a loadable type.", entityData.TypeName)); }

            entityData = entityData.Clone();
            if (!entityData.Fields.ContainsKey("ID"))
                entityData.Set("ID", Guid.NewGuid());

            Entity output = entityData.Construct() as Entity;
            return output;
        }
        public Entity AddEntity(Entity entity)
        {
            entity.Manager = this;
            entity.Initialize();
            if (entity.Compacted)
                Entities.Compact(entity);
            else
                Entities.Add(entity);
            if (entity.InitData?.Name != null)
                initDataLookup[entity.InitData.Name].Add(entity);
            return entity;
        }
        public Entity Remove(Guid ID)
        {
            Entity removed = Entities.Remove(ID);
            if (removed == null) return null;
            if (removed.InitData?.Name != null)
                initDataLookup[removed.InitData.Name].Remove(removed);
            return removed;
        }
        public void ClearEntities(Func<Entity, bool> predicate = null)
        {
            foreach (Entity entity in Entities.All.ToList())
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
            return Entities.UpdateSorted.Where(e => e is T).Cast<T>();
        }
        public IEnumerable<Entity> FindByPrefab(string prefab)
        {
            return initDataLookup[prefab];
        }
        // TODO: Use an octree or something
        public IEnumerable<T> FindNearest<T>(Vector3 position, Func<T, bool> predicate = null) where T : GameObject
        {
            return Entities.All
                .Select(e => e as T)
                .Where(go => go != null && (predicate?.Invoke(go) ?? true))
                .OrderBy(go => (go.Position - position).LengthSquared);
        }
    }
}
