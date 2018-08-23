using System;
using System.Collections.Generic;
using System.Text;

namespace Spectrum.Framework.Physics.Dynamics
{

    // TODO: Check values, Documenation
    // Maybe some default materials, aka Material.Soft?
    public class Material
    {
        public float kineticFriction = 0.1f;
        public float staticFriction = 0.6f;
        public float restitution = 0.2f;
    }
}
