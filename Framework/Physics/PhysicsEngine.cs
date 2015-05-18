using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Physics;
using Spectrum.Framework.Physics.Collision;
using Spectrum.Framework.Physics.Dynamics;
using Spectrum.Framework.Entities;

namespace Spectrum.Framework.Physics
{
    public class PhysicsEngine
    {
        public CollisionSystemPersistentSAP CollisionSystem = new CollisionSystemPersistentSAP();
        private static PhysicsEngine _single;
        private EntityCollection ECollection;
        World world;
        public static PhysicsEngine Single
        {
            get
            {
                return _single;
            }
        }
        public static void Init(EntityCollection ECollection)
        {
            _single = new PhysicsEngine(ECollection);
        }
        private PhysicsEngine(EntityCollection ECollection)
        {
            this.ECollection = ECollection;
            world = new World(CollisionSystem);
            world.Events.BodiesBeginCollide += worldEvents_bodiesBeginCollide;
            world.Events.BodiesEndCollide += worldEvents_bodiesEndCollide;
            ECollection.OnEntityAdded += ECollection_OnEntityAdded;
            ECollection.OnEntityRemoved += ECollection_OnEntityRemoved;
        }

        void ECollection_OnEntityRemoved(Entity updated)
        {
            if (updated is GameObject)
                world.RemoveBody(updated as GameObject);
        }

        void ECollection_OnEntityAdded(Entity updated)
        {
            if (updated is GameObject)
                world.AddBody(updated as GameObject);
        }

        void worldEvents_bodiesBeginCollide(GameObject body1, GameObject body2)
        {
            body1.OnCollide(body2);
            body2.OnCollide(body1);
        }
        void worldEvents_bodiesEndCollide(GameObject body1, GameObject body2)
        {
            body1.OnEndCollide(body2);
            body2.OnEndCollide(body1);
        }

        public void Update(GameTime gameTime)
        {
            world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f, true);
        }
        public void ShiftOrigin(Vector3 offset)
        {
            foreach (Entity obj in world.Collidables)
            {
                if (obj as GameObject != null)
                {
                    (obj as GameObject).Position += offset;
                }
            }
        }
        public GroundInfo GetTerrainHeight(Vector3 point)
        {
            float fraction;
            Vector3 normal;
            foreach (GameObject t in world.Collidables)
            {
                point.Y = t.BoundingBox.Max.Y + 100;
                if (CollisionSystem.Raycast(t, point, Vector3.Down, out normal, out fraction))
                {
                    return new GroundInfo(t, point + Vector3.Down * fraction, normal);
                }
            }

            return null;
        }
    }
}
