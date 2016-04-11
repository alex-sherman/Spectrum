using Spectrum.Framework.Graphics;
using Spectrum.Framework.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Entities
{
    public delegate void EntityCollectionUpdated(Entity updated);

    public class EntityCollection
    {
        Dictionary<Guid, Entity> entities = new Dictionary<Guid, Entity>();
        private List<Entity> _updateables = new List<Entity>();
        public List<Entity> updateables { get { lock (this) { return _updateables.ToList(); } } }

        public void Add(Entity entity)
        {
            lock (this)
            {
                if (OnEntityAdded != null) { OnEntityAdded(entity); }
                entities[entity.ID] = entity;
                int i = 0;
                while (i < _updateables.Count - 1 && _updateables[i].UpdateOrder < entity.UpdateOrder) { i++; }
                _updateables.Insert(i, entity);
            }
        }
        public event EntityCollectionUpdated OnEntityAdded;
        public void Remove(Guid entityID)
        {
            lock (this)
            {
                if (entities.ContainsKey(entityID))
                {
                    Entity entity = entities[entityID];
                    if (OnEntityRemoved != null) { OnEntityRemoved(entity); }
                    entities.Remove(entityID);
                    _updateables.Remove(entity);
                    entity.Dispose();
                }
            }
        }
        public event EntityCollectionUpdated OnEntityRemoved;

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
        public IEnumerator<Entity> GetEnumerator()
        {
            return entities.Select(keyvalue => keyvalue.Value).GetEnumerator();
        }
    }
}
