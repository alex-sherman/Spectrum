using Microsoft.Xna.Framework;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Physics.Collision.Shapes
{
    [ProtoContract]
    public class ListMultishape : Multishape
    {
        [ProtoMember(1)]
        public List<Shape> Shapes;
        private int currentShape;

        public ListMultishape() : this(new List<Shape>()) { }

        public ListMultishape(List<Shape> shapes)
        {
            if (shapes.Any((Shape shape) => (shape is Multishape)))
            {
                throw new ArgumentException("Cannot use Multishapes in a ListMultishape");
            }
            Shapes = shapes;
        }

        public void Add(Shape shape)
        {
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            Shapes.Add(shape);
            UpdateShape();
        }

        public void Remove(Shape shape)
        {
            Shapes.Remove(shape);
            UpdateShape();
        }

        public override void SetCurrentShape(int index)
        {
            currentShape = index;
        }
        //TODO: These could be optimized
        public override int Prepare(ref LinearMath.JBBox box)
        {
            return Shapes.Count;
        }

        public override int Prepare(ref Vector3 rayOrigin, ref Vector3 rayDelta)
        {
            return Shapes.Count;
        }

        protected override Multishape CreateWorkingClone()
        {
            ListMultishape output = new ListMultishape();
            output.Shapes = Shapes;
            output.currentShape = currentShape;

            return output;
        }

        public override void SupportMapping(ref Vector3 direction, out Vector3 result)
        {
            Shapes[currentShape].SupportMapping(ref direction, out result);
        }

        public override void SetScale(float scale)
        {
            foreach (var shape in Shapes)
                shape.SetScale(scale);
            UpdateShape();
        }
    }
}
