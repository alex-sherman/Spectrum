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
        public List<Entity> UpdateSorted { get { lock (this) { return updatedSorted.ToList(); } } }
        public List<Entity> DrawSorted { get { lock (this) { return drawSorted.ToList(); } } }

        public void Add(Entity entity)
        {
            lock (this)
            {
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
                    entity.Dispose();
                    return entity;
                }
                return null;
            }
        }
    }
}
