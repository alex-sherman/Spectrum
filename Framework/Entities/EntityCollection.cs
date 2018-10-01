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
        public Dictionary<Guid, Entity> Map = new Dictionary<Guid, Entity>();
        private List<Entity> updatedSorted = new List<Entity>();
        private List<Entity> drawSorted = new List<Entity>();
        public IEnumerable<Entity> UpdateSorted { get { lock (this) { return updatedSorted.Where(e => !e.Destroying); } } }
        public IEnumerable<Entity> DrawSorted { get { lock (this) { return drawSorted.Where(e => !e.Destroying); } } }
        public IEnumerable<Entity> Destroying { get { lock (this) { return Map.Values.Where(e => e.Destroying); } } }
        public IEnumerable<Entity> All { get { lock (this) { return Map.Values; } } }

        public void Add(Entity entity)
        {
            lock (this)
            {
                if (Map.ContainsKey(entity.ID))
                    throw new InvalidOperationException("An Enttiy with that ID has already been added to the collection");
                Map[entity.ID] = entity;
                int i;
                for (i = 0; i < updatedSorted.Count - 1 && updatedSorted[i].UpdateOrder < entity.UpdateOrder; i++) { }
                updatedSorted.Insert(i, entity);
                drawSorted.Insert(drawSorted.Count(check => check.DrawOrder < entity.DrawOrder), entity);
            }
        }
        public Entity Remove(Guid entityID)
        {
            lock (this)
            {
                if (Map.ContainsKey(entityID))
                {
                    Entity entity = Map[entityID];
                    Map.Remove(entityID);
                    updatedSorted.Remove(entity);
                    drawSorted.Remove(entity);
                    entity.Destroy();
                    return entity;
                }
                return null;
            }
        }
    }
}
