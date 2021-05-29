using Spectrum.Framework.Graphics;
using Spectrum.Framework.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Spectrum.Framework.Entities
{
    public class EntityCollection
    {
        public readonly Dictionary<Guid, Entity> Map = new Dictionary<Guid, Entity>();
        public readonly Dictionary<Guid, Entity> Compacted = new Dictionary<Guid, Entity>();
        private List<Entity> updatedSorted = new List<Entity>();
        private List<Entity> drawSorted = new List<Entity>();
        public event Action<Entity> OnEntityAdded;
        public event Action<Entity> OnEntityRemoved;
        public IEnumerable<Entity> UpdateSorted { get { lock (this) { return updatedSorted.Where(e => !e.Destroying); } } }
        public IEnumerable<Entity> DrawSorted { get { lock (this) { return drawSorted.Where(e => !e.Destroying); } } }
        public IEnumerable<Entity> Destroying { get { lock (this) { return Map.Values.Where(e => e.Destroying); } } }
        public IEnumerable<Entity> All { get { lock (this) { return Map.Values; } } }

        public void Add(Entity entity)
        {
            lock (this)
            {
                if (Map.ContainsKey(entity.ID))
                    throw new InvalidOperationException("An Entity with that ID has already been added to the collection");
                Map[entity.ID] = entity;
                int updateIndex = updatedSorted.TakeWhile(e => e.UpdateOrder < entity.UpdateOrder).Count();
                updatedSorted.Insert(updateIndex, entity);
                int drawIndex = drawSorted.TakeWhile(e => e.DrawOrder < entity.DrawOrder).Count();
                drawSorted.Insert(drawIndex, entity);
                OnEntityAdded?.Invoke(entity);
            }
        }
        public void Compact(Entity entity)
        {
            lock (this)
            {
                if (Map.ContainsKey(entity.ID))
                    RemoveFromMap(entity.ID);
                Compacted[entity.ID] = entity;
            }
        }
        public void Uncompact(Entity entity)
        {
            lock (this)
            {
                Compacted.Remove(entity.ID);
                Add(entity);
            }
        }
        private Entity RemoveFromMap(Guid entityID)
        {
            var entity = Map[entityID];
            Map.Remove(entityID);
            updatedSorted.Remove(entity);
            drawSorted.Remove(entity);
            OnEntityRemoved?.Invoke(entity);
            return entity;
        }
        public Entity Remove(Guid entityID)
        {
            lock (this)
            {
                Entity entity = null;
                if (Compacted.ContainsKey(entityID))
                {
                    entity = Compacted[entityID];
                    Compacted.Remove(entityID);
                }
                else if (Map.ContainsKey(entityID))
                    entity = RemoveFromMap(entityID);
                return entity;
            }
        }
    }
}
