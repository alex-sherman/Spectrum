using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public struct Ray
    {
        public Ray(Vector3 position, Vector3 direction) { Direction = direction; Position = position; }
        public Vector3 Direction { get; set; }
        public Vector3 Position { get; set; }
    }
}
