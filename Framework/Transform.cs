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
        Vector3 Scale { get; }
    }
    public class Transform : ITransform
    {
        private ITransform source;
        public Transform() { }
        public Transform(Vector3 position)
        {
            Translation = position;
        }
        public Transform(Vector3 position, Quaternion rotation)
        {
            Translation = position;
            Rotation = rotation;
        }
        public Transform(ITransform source) { this.source = source; }
        public ITransform Parent;
        private Vector3 translation;
        public Vector3 Translation
        {
            get => source != null ? translation + source.Position : translation;
            set => translation = value;
        }
        public Vector3 Position
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.Apply(Translation);
                }
                return Translation;
            }
        }
        private Quaternion rotation = Quaternion.Identity;
        public Quaternion Rotation
        {
            get => source != null ? rotation.Concat(source.Orientation) : rotation;
            set => rotation = value;
        }
        public Vector3 Scale = Vector3.One;
        Vector3 ITransform.Scale => Scale;
        public Quaternion Orientation => Parent != null ? Rotation.Concat(Parent.Orientation) : Rotation;
        public static Transform Copy(ITransform transform)
            => new Transform(transform.Position, transform.Orientation);
        public Matrix LocalWorld => Matrix.CreateScale(Scale) * Rotation.ToMatrix() * Matrix.CreateTranslation(Translation);

    }
    public static class TransformExtension
    {
        public static Matrix World(this ITransform transform)
            => Matrix.CreateScale(transform.Scale) * transform.Orientation.ToMatrix() * Matrix.CreateTranslation(transform.Position);
        public static Vector3 Apply(this ITransform transform, Vector3 point)
            => transform.World() * point;
    }
}
