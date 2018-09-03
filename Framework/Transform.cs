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
        public Vector3 Position;
        Vector3 ITransform.Position => Position;
        public Quaternion Orientation;
        Quaternion ITransform.Orientation => Orientation;
        public Matrix World => Matrix.CreateFromQuaternion(Orientation) * Matrix.CreateTranslation(Position);
    }
}
