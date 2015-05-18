using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Graphics.Animation
{
    public class Bone
    {
        public string id;
        public List<Bone> children;
        public Bone parent;
        public Matrix defaultRotation;
        public Matrix defaultTranslation;
        public Matrix inverseBindPose;
        public Matrix transform;
        public Matrix withParentTransform { get { return transform * (parent != null ? parent.withParentTransform : Matrix.Identity); } }
        public Matrix absoluteTransform { get { return inverseBindPose * withParentTransform; } }

        public Bone(string id, Bone parent)
        {
            this.parent = parent;
            defaultRotation = Matrix.Identity;
            defaultTranslation = Matrix.Identity;
            inverseBindPose = Matrix.Identity;
            transform = Matrix.Identity;
            this.id = id;
            children = new List<Bone>();
        }
    }
}
