using Microsoft.Xna.Framework;
using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Physics.Collision
{
    public class GroundInfo
    {
        public GameObject Object { get; private set; }
        public Plane Plane { get; private set; }
        public Vector3 Point { get; private set; }
        public GroundInfo(GameObject other, Vector3 point, Vector3 normal)
        {
            Point = point;
            Object = other;
            Plane = new Plane(normal, Vector3.Dot(point, normal));
        }
        public void Update(Vector3 point, Vector3 normal)
        {
            Plane = new Plane(normal, Vector3.Dot(point, normal));
        }
    }
}
