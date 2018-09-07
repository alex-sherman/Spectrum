using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public interface ITransform
    {
        Vector3 Position { get; }
        Quaternion Orientation { get; }
    }
    public class Transform : ITransform
    {
        public Transform() { }
        public Transform(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Orientation = rotation;
        }
        public Vector3 Position;
        Vector3 ITransform.Position => Position;
        public Quaternion Orientation;
        Quaternion ITransform.Orientation => Orientation;
        public static Transform From(ITransform transform)
            => new Transform(transform.Position, transform.Orientation);

    }
    public static class TransformExtension
    {
        public static Matrix World(this ITransform transform)
            => Matrix.CreateFromQuaternion(transform.Orientation) * Matrix.CreateTranslation(transform.Position);
        public static Vector3 Apply(this ITransform transform, Vector3 point)
            => Vector3.Transform(point, transform.World());
    }
}
