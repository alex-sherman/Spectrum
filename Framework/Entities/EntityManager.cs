using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spectrum.Framework.Physics;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Reflection;
using Spectrum.Framework.Network;
using System.Diagnostics;
using Spectrum.Framework.Input;
using System.Collections;
using Spectrum.Framework.Network.Surrogates;

namespace Spectrum.Framework.Entities
{
    public delegate void EntityMessageHandler(NetID peerID, Entity entity, NetMessage message);
    public class EntityManager : IEnumerable<Entity>
    {
        public event Action<Entity> OnEntityAdded;
        public event Action<Entity> OnEntityRemoved;
        private float tickTenthTimer = 100;
        private float tickOneTimer = 1000;
        public EntityCollection Entities;
        private MultiplayerService mpService;
        public bool Paused = false;
        private static Stopwatch timer = new Stopwatch();
        public EntityManager(MultiplayerService mpService)
        {
            Entities = new EntityCollection();
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
                            message.Write(entity.ReplicationData.InitData);
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
            eData.Write(entity.ReplicationData.InitData);
            mpService.SendMessage(FrameworkMessages.EntityCreation, eData);

            SendEntityReplication(entity, peerDestination);
        }
        public Entity HandleEntityCreation(NetID peerGuid, NetMessage message)
        {
            InitData entityData = message.Read<InitData>();
            Guid id = (Guid)entityData.fields["ID"].Object;
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
            tickTenthTimer += gameTime.ElapsedGameTime.Milliseconds;
            tickOneTimer += gameTime.ElapsedGameTime.Milliseconds;

            List<Entity> updateables = Entities.UpdateSorted;
            for (int i = 0; i < updateables.Count; i++)
            {
                var timer = DebugTiming.Update.Time(updateables[i].GetType().Name);
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
                if (updateables[i].Disposing)
                    Remove(updateables[i].ID);
                timer.Stop();
            }
            if (tickOneTimer >= 1000) tickOneTimer = 0;
            if (tickTenthTimer >= 100) tickTenthTimer = 0;
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
        /// <summary>
        /// Calls the constructor for the Entity, but does not initialize it or add it to the EntityManager
        /// </summary>
        /// <param name="entityData">Creation data for the Entity</param>
        /// <returns>An initialized</returns>
        public Entity Construct(InitData entityData)
        {
            if (entityData.TypeData == null) { throw new ArgumentException(String.Format("Replication occured for a class {0} not found as a loadable type.", entityData.TypeName)); }

            entityData = entityData.Clone();
            if (!entityData.fields.ContainsKey("OwnerGuid"))
                entityData.Set("OwnerGuid", mpService.ID);
            if (!entityData.fields.ContainsKey("ID"))
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
            if (OnEntityAdded != null) { OnEntityAdded(entity); }
            return entity;
        }
        public Entity Remove(Guid ID)
        {
            Entity removed = Entities.Remove(ID);
            if (removed == null) return null;
            if (OnEntityRemoved != null) { OnEntityRemoved(removed); }
            return removed;
        }
        public void ClearEntities(Func<Entity, bool> predicate = null)
        {
            foreach (Entity entity in Entities.UpdateSorted)
            {
                if (predicate == null || predicate(entity))
                    Entities.Remove(entity.ID);
            }
        }

        public Entity Find(Guid id)
        {
            if (Entities.Map.ContainsKey(id))
                return Entities.Map[id];
            return null;
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
