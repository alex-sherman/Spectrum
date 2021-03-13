using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Graphics.Animation
{
    public class Bone
    {
        public string Id;
        public List<Bone> Children;
        public Bone Parent;
        public Matrix DefaultRotation;
        public Matrix DefaultTranslation;
        public Matrix BindPose;
        public Matrix InverseBindPose;
        private Matrix transform;
        public Matrix Transform
        {
            get { return transform; }
            set { SetDirty(); transform = value; }
        }
        private Matrix withParentTransform;
        private bool dirty = true;
        private void SetDirty()
        {
            dirty = true;
            foreach (var child in Children)
            {
                child.SetDirty();
            }
        }
        public Matrix WithParentTransform
        {
            get
            {
                if (dirty)
                {
                    withParentTransform = Transform * (Parent != null ? Parent.WithParentTransform : Matrix.Identity);
                    dirty = false;
                }
                return withParentTransform;
            }
        }
        public Matrix DeltaTransform => InverseBindPose * WithParentTransform;

        public Bone(string id, Bone parent)
        {
            Parent = parent;
            Children = new List<Bone>();
            DefaultRotation = Matrix.Identity;
            DefaultTranslation = Matrix.Identity;
            BindPose = Matrix.Identity;
            InverseBindPose = Matrix.Identity;
            Transform = Matrix.Identity;
            Id = id;
        }

        public Bone Clone(Dictionary<string, Bone> bones, Bone parent = null)
        {
            Bone output = new Bone(Id, parent);
            output.DefaultRotation = DefaultRotation;
            output.DefaultTranslation = DefaultTranslation;
            output.BindPose = BindPose;
            output.InverseBindPose = InverseBindPose;
            output.transform = transform;
            bones[Id] = output;
            foreach (var child in Children)
            {
                output.Children.Add(child.Clone(bones, output));
            }
            return output;
        }
    }
}
