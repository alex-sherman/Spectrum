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
        private Matrix _transform;
        public Matrix transform
        {
            get { return _transform; }
            set { SetDirty(); _transform = value; }
        }
        private Matrix _withParentTransform;
        private bool dirty = true;
        private void SetDirty()
        {
            dirty = true;
            foreach (var child in children)
            {
                child.SetDirty();
            }
        }
        public Matrix withParentTransform
        {
            get
            {
                if (dirty)
                {
                    _withParentTransform = transform * (parent != null ? parent.withParentTransform : Matrix.Identity);
                    dirty = false;
                }
                return _withParentTransform;
            }
        }
        public Matrix absoluteTransform { get { return inverseBindPose * withParentTransform; } }

        public Bone(string id, Bone parent)
        {
            this.parent = parent;
            children = new List<Bone>();
            defaultRotation = Matrix.Identity;
            defaultTranslation = Matrix.Identity;
            inverseBindPose = Matrix.Identity;
            transform = Matrix.Identity;
            this.id = id;
        }
    }
}
