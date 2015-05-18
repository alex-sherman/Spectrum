using Spectrum.Framework.Physics.Collision.Shapes;
using Spectrum.Framework.Physics.LinearMath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Physics.Collision
{
    public interface ICollidable
    {
        Shape Shape { get; }
        Vector3 Position { get; }
        Matrix Orientation { get; }
        JBBox BoundingBox { get;  }
    }
}
