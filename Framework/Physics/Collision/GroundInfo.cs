﻿using Microsoft.Xna.Framework;
using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Physics.Collision
{
    public class CollisionInfo
    {
        public GameObject Object { get; private set; }
        public Vector3 Normal { get; private set; }
        public Vector3 Point { get; private set; }
        private CollisionInfo() { }
        public CollisionInfo(GameObject other, Vector3 point, Vector3 normal)
        {
            Point = point;
            Object = other;
            Normal = normal;
        }
        public void Update(Vector3 point, Vector3 normal)
        {
            Point = point;
            Normal = normal;
        }
    }
}
