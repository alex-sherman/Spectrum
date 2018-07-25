using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Physics.Collision.Shapes
{
    public class ListMultishape : Multishape
    {
        public List<Shape> Shapes;
        private int currentShape;

        public ListMultishape() : this(new List<Shape>()) { }

        public ListMultishape(List<Shape> shapes)
        {
            if(shapes.Any((Shape shape) => (shape is Multishape)))
            {
                throw new ArgumentException("Cannot use Multishapes in a ListMultishape");
            }
            Shapes = shapes;
        }

        public void AddShape(Shape shape)
        {
            UpdateShape();
            Shapes.Add(shape);
        }

        public void RemoveShape(Shape shape)
        {
            UpdateShape();
            Shapes.Remove(shape);
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

        public override int Prepare(ref Microsoft.Xna.Framework.Vector3 rayOrigin, ref Microsoft.Xna.Framework.Vector3 rayDelta)
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

        public override void SupportMapping(ref Microsoft.Xna.Framework.Vector3 direction, out Microsoft.Xna.Framework.Vector3 result)
        {
            Shapes[currentShape].SupportMapping(ref direction, out result);
        }
    }
}
