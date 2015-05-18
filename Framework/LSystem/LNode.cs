using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.LSystem
{
    public class LNode
    {
        public Matrix Transform { get { return Matrix.CreateTranslation(Vector3.Up) * Matrix.CreateFromAxisAngle(new Vector3((float)Math.Cos(R2), 0, (float)Math.Sin(R2)), R1) * Matrix.CreateScale(D) * root; } }
        public List<LNode> Children = new List<LNode>();
        public float D;
        public float R1;
        public float R2;
        private Matrix root;
        public LNode(float D, float R1, float R2, Matrix root)
        {
            this.root = root;
            this.D = D;
            this.R1 = R1;
            this.R2 = R2;
        }
    }
}
