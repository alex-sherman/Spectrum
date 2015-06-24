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

namespace Spectrum.Framework.Entities
{
    public delegate void EntityMessageHandler(NetID peerID, Entity entity, NetMessage message);
    public delegate void EntityReplicationCallback(Entity entity);
    public delegate void FunctionReplicationcallback(Entity entity, int function, NetMessage parameters);
    public class EntityManager
    {
        private float tickTenthTimer = 100;
        private float tickOneTimer = 1000;
        private EntityCollection ECollection;
        public MultiplayerService mpService;
        public bool Paused = false;
        public EntityManager(EntityCollection ECollection, MultiplayerService mpService)
        {
            this.ECollection = ECollection;
            this.mpService = mpService;
            NetMessage.ECollection = ECollection;
            RegisterCallbacks();
        }
        private void RegisterCallbacks()
        {
            HandshakeHandler entitySender = new HandshakeHandler(
                    delegate(NetID peerGuid, NetMessage message)
                    {
                        IEnumerable<Entity> replicateable = ECollection.updateables.Where(x => x.AllowReplicate && x.OwnerGuid != peerGuid);
                        message.Write(replicateable.Count());
                        foreach (Entity entity in replicateable)
                        {
                            new EntityData(entity).WriteTo(message);
                        }
                    },
                    delegate(NetID peerGuid, NetMessage message)
                    {
                        int count = message.ReadInt();
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
            new EntityData(entity).WriteTo(eData);
            mpService.SendMessage(FrameworkMessages.EntityCreation, eData);
        }
        public void HandleEntityCreation(NetID peerGuid, NetMessage message)
        {
            EntityData entityData = new EntityData(message);
            if (!ECollection.Contains(entityData.guid))
                CreateEntityFromData(entityData);
        }

        public void SendShowCreate(Guid entityID, NetID peerDestination = default(NetID))
        {
            NetMessage message = new NetMessage();
            message.Write(entityID);
            mpService.SendMessage(FrameworkMessages.ShowCreate, message, peerDestination);
        }
        public void HandleShowCreate(NetID peerGuid, NetMessage message)
        {
            Guid entityID = message.ReadGuid();
            if (!ECollection.Contains(entityID)) { return; }
            Entity entity = ECollection.Find(entityID);
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
            Guid entityID = message.ReadGuid();
            message = message.ReadMessage();
            if (ECollection.Contains(entityID))
            {
                ECollection.Find(entityID).HandleMessage(peerGuid, message.ReadInt(), message.ReadMessage());
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

            List<Entity> updateables = ECollection.updateables;
            for (int i = 0; i < updateables.Count; i++)
            {
                if (updateables[i].Enabled)
                {
                    updateables[i].Update(gameTime);
                    if (tickOneTimer >= 1000)
                        updateables[i].TickOne();
                    if (tickTenthTimer >= 100)
                        updateables[i].TickTenth();
                }
                if (updateables[i].Disposing)
                    ECollection.Remove(updateables[i].ID);
            }
            if (tickOneTimer >= 1000) tickOneTimer = 0;
            if (tickTenthTimer >= 100) tickTenthTimer = 0;
        }
        public T CreateEntity<T>(params object[] args) where T : Entity
        {
            T output = CreateEntityType(typeof(T), args) as T;
            return output;
        }
        public Entity CreateEntityFromData(EntityData data)
        {
            Type t = TypeHelper.Helper[data.type];
            Entity e = CreateEntityType(t, data.guid, data.owner, data.args);
            if (e is GameObject) { (e as GameObject).Position = data.position; }
            return e;
        }
        public Entity CreateEntityType(Type t, params object[] args)
        {
            Entity output = CreateEntityType(t, Guid.NewGuid(), mpService.ID, args);
            if (output != null)
                SendEntityCreation(output);
            return output;
        }
        public Entity CreateEntityType(Type t, Guid entityID, NetID ownerID, params object[] args)
        {
            if (t == null || (!t.IsSubclassOf(typeof(Entity)))) { return null; }
            Entity output = (Entity)TypeHelper.Instantiate(t, args);
            output.OwnerGuid = ownerID;
            output.ID = entityID;
            output.creationArgs = args;
            output.Manager = this;
            ECollection.Add(output);
            output.SendMessageCallback = SendEntityMessage;
            output.Initialize();
            return (Entity)output;
        }
        public void ClearEntities(Func<Entity, bool> predicate = null)
        {
            foreach (Entity entity in ECollection.updateables)
            {
                if (predicate == null || predicate(entity))
                    RemoveEntity(entity.ID);
            }
        }
        public IEnumerable<T> FindEntities<T>(Func<T, bool> predicate = null) where T : Entity
        {
            predicate = predicate ?? new Func<T, bool>((T f) => (true));
            if (typeof(T) == typeof(Entity))
                return ECollection.updateables.Where(predicate as Func<Entity, bool>).ToList() as List<T>;

            return ECollection.updateables
                .Where((Entity entity) => (entity is T))
                .Cast<T>()
                .Where(predicate);
        }
        public void RemoveEntity(Guid entityID)
        {
            ECollection.Remove(entityID);
        }

        public void Draw(GameTime gameTime)
        {
            GraphicsEngine.Render(ECollection.updateables, gameTime);
        }
    }
}
