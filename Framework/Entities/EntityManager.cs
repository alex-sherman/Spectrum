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

namespace Spectrum.Framework.Entities
{
    public delegate void EntityMessageHandler(NetID peerID, Entity entity, NetMessage message);
    public delegate void EntityReplicationCallback(Entity entity);
    public delegate void FunctionReplicationcallback(Entity entity, int function, NetMessage parameters);
    public class EntityManager : IEnumerable<Entity>
    {
        public event Action<Entity> OnEntityAdded;
        public event Action<Entity> OnEntityRemoved;
        private float tickTenthTimer = 100;
        private float tickOneTimer = 1000;
        private EntityCollection Entities;
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
                            message.Write(entity.CreationData);
                        }
                    },
                    delegate (NetID peerGuid, NetMessage message)
                    {
                        int count = message.Read<int>();
                        for (int i = 0; i < count; i++)
                        {
                            HandleEntityCreation(peerGuid, message);
                        }
                    }
                );
            Handshake.RegisterHandshakeHandler(HandshakeStage.PartialResponse, entitySender);
            Handshake.RegisterHandshakeHandler(HandshakeStage.Completed, entitySender);
            mpService.RegisterMessageCallback(FrameworkMessages.EntityCreation, HandleEntityCreation);
            mpService.RegisterMessageCallback(FrameworkMessages.ShowCreate, HandleShowCreate);
            mpService.RegisterMessageCallback(FrameworkMessages.EntityMessage, HandleEntityMessage);
        }

        #region Network Functions

        public void SendEntityCreation(Entity entity, NetID peerDestination = default(NetID))
        {
            if (!entity.AllowReplicate) { return; }

            NetMessage eData = new NetMessage();
            eData.Write(entity.CreationData);
            mpService.SendMessage(FrameworkMessages.EntityCreation, eData);
        }
        public void HandleEntityCreation(NetID peerGuid, NetMessage message)
        {
            InitData entityData = message.Read<InitData>();
            if (!Entities.Map.ContainsKey((Guid)entityData.fields["ID"].Object))
                CreateEntity(entityData);
        }

        public void SendShowCreate(Guid entityID, NetID peerDestination = default(NetID))
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

        public void SendEntityMessage(NetID peerID, Entity entity, NetMessage message)
        {
            if (!entity.CanReplicate) { return; }
            NetMessage toSend = new NetMessage();
            toSend.Write(entity.ID);
            toSend.Write(message);
            mpService.SendMessage(FrameworkMessages.EntityMessage, toSend);
        }
        public void HandleEntityMessage(NetID peerGuid, NetMessage message)
        {
            Guid entityID = message.Read<Guid>();
            message = message.Read<NetMessage>();
            if (Entities.Map.ContainsKey(entityID))
            {
                Entities.Map[entityID].HandleMessage(peerGuid, message.Read<int>(), message.Read<NetMessage>());
            }
            else
            {
                SendShowCreate(entityID, peerGuid);
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
                timer.Restart();
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
                string itemName = updateables[i].GetType().Name;
                DebugPrinter.time("Update", itemName, timer.Elapsed.TotalMilliseconds);
            }
            if (tickOneTimer >= 1000) tickOneTimer = 0;
            if (tickTenthTimer >= 100) tickTenthTimer = 0;
        }
        public T Create<T>(params object[] args) where T : Entity
        {
            T output = CreateEntity(new InitData(typeof(T).Name, args)) as T;
            return output;
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
            if (entityData.TypeData == null) { throw new ArgumentException(String.Format("Replication occured for a class {0} not found as a loadable type.", entityData.type)); }

            entityData = entityData.Clone();
            if (!entityData.fields.ContainsKey("OwnerGuid"))
                entityData.Set("OwnerGuid", mpService.ID);
            if (!entityData.fields.ContainsKey("ID"))
                entityData.Set("ID", Guid.NewGuid());

            Entity output = entityData.Construct() as Entity;

            output.CreationData = entityData;
            output.SendMessageCallback = SendEntityMessage;
            return output;
        }
        public Entity AddEntity(Entity entity)
        {
            entity.Manager = this;
            Entities.Add(entity);
            if (mpService.ID == entity.OwnerGuid)
                SendEntityCreation(entity);
            entity.Initialize();
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

        public void Draw(GameTime gameTime)
        {
            GraphicsEngine.Render(Entities.DrawSorted, gameTime);
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
