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
        public Quaternion DefaultRotation;
        public Vector3 DefaultTranslation;
        public Matrix BindPose;
        public Matrix InverseBindPose;
        private Quaternion rotation;
        public Quaternion Rotation
        {
            get => rotation;
            set { SetDirty(); rotation = value; }
        }
        private Vector3 translation;
        public Vector3 Translation
        {
            get => translation;
            set { SetDirty(); translation = value; }
        }
        private Matrix transform;

        private Matrix withParentTransform;
        private bool dirty = true;
        private void SetDirty()
        {
            dirty = true;
            foreach (var child in Children)
                child.SetDirty();
        }
        private void ResolveDirty()
        {
            if (dirty)
            {
                // TODO: I think this can be combined into one operation
                transform = rotation.ToMatrix() * Matrix.CreateTranslation(translation);
                withParentTransform = transform * (Parent != null ? Parent.WithParentTransform : Matrix.Identity);
                dirty = false;
            }
        }
        public Matrix WithParentTransform
        {
            get
            {
                ResolveDirty();
                return withParentTransform;
            }
        }
        public Matrix DeltaTransform => InverseBindPose * WithParentTransform;

        public Bone(string id, Bone parent)
        {
            Parent = parent;
            Children = new List<Bone>();
            DefaultRotation = Quaternion.Identity;
            DefaultTranslation = Vector3.Zero;
            BindPose = Matrix.Identity;
            InverseBindPose = Matrix.Identity;
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
