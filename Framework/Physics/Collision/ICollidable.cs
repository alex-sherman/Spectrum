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
        Vector3 Velocity { get; }
        Matrix Orientation { get; }
        JBBox BoundingBox { get;  }
    }
    public static class ICollidableExtension
    {
        public static JBBox SweptBoundingBox(this ICollidable col)
        {
            JBBox box = col.BoundingBox;
            box.AddPoint(box.Max + col.Velocity);
            box.AddPoint(box.Min + col.Velocity);
            return box;
        }
    }
}
