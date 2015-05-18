using Spectrum.Framework.Graphics;
using Spectrum.Framework.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Entities
{
    public delegate void EntityCollectionUpdated(Entity updated);
    enum EntityCollectionAction
    {
        Remove,
        Add
    }
    struct ActionQueueItem
    {
        public Entity entity;
        public EntityCollectionAction action;
        public ActionQueueItem(EntityCollectionAction action, Entity entity)
        {
            this.entity = entity;
            this.action = action;
        }
    }
    public class EntityCollection
    {
        Dictionary<Guid, Entity> entities = new Dictionary<Guid, Entity>();
        public List<Entity> updateables = new List<Entity>();
        public List<GameObject> gameObjects = new List<GameObject>();
        private List<ActionQueueItem> actionQueue = new List<ActionQueueItem>();

        public void Add(Entity entity)
        {
            lock (this)
            {
                actionQueue.Add(new ActionQueueItem(EntityCollectionAction.Add, entity));
            }
        }
        public event EntityCollectionUpdated OnEntityAdded;
        public void Remove(Entity entity)
        {
            lock (this)
            {
                actionQueue.Add(new ActionQueueItem(EntityCollectionAction.Remove, entity));
            }
        }
        public event EntityCollectionUpdated OnEntityRemoved;
        public void Update()
        {
            lock (this)
            {
                foreach (ActionQueueItem actionItem in actionQueue)
                {
                    Entity entity = actionItem.entity;
                    switch (actionItem.action)
                    {
                        case EntityCollectionAction.Remove:
                            if (entities.ContainsKey(entity.ID))
                            {
                                if (OnEntityRemoved != null) { OnEntityRemoved(entity); }
                                entities.Remove(entity.ID);
                                updateables.Remove(entity);
                                if (entity is GameObject)
                                {
                                    gameObjects.Remove(entity as GameObject);
                                }
                            }
                            break;
                        case EntityCollectionAction.Add:
                            if (OnEntityAdded != null) { OnEntityAdded(entity); }
                            entities[entity.ID] = entity;
                            int i = 0;
                            while (i < updateables.Count - 1 && updateables[i].UpdateOrder < entity.UpdateOrder) { i++; }
                            updateables.Insert(i, entity);
                            if (entity is GameObject)
                            {
                                gameObjects.Add(entity as GameObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                actionQueue = new List<ActionQueueItem>();
            }
        }

        public Entity Find(Guid key)
        {
            return entities[key];
        }
        public bool Contains(Guid id)
        {
            return entities.ContainsKey(id);
        }
        public bool Contains(Entity item)
        {
            return entities.ContainsKey(item.ID);
        }
    }
}
