using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Physics.Collision.Shapes
{
    public class ListMultishape : Multishape
    {
        private List<Shape> shapes;
        private int currentShape;

        private ListMultishape() { }

        public ListMultishape(List<Shape> shapes)
        {
            if(shapes.Any((Shape shape) => (shape is Multishape)))
            {
                throw new ArgumentException("Cannot use Multishapes in a ListMultishape");
            }
            this.shapes = shapes;
        }

        public override void SetCurrentShape(int index)
        {
            currentShape = index;
        }
        //TODO: These could be optimized
        public override int Prepare(ref LinearMath.JBBox box)
        {
            return shapes.Count;
        }

        public override int Prepare(ref Microsoft.Xna.Framework.Vector3 rayOrigin, ref Microsoft.Xna.Framework.Vector3 rayDelta)
        {
            return shapes.Count;
        }

        protected override Multishape CreateWorkingClone()
        {
            ListMultishape output = new ListMultishape();
            output.shapes = shapes;
            output.currentShape = currentShape;

            return output;
        }

        public override void SupportMapping(ref Microsoft.Xna.Framework.Vector3 direction, out Microsoft.Xna.Framework.Vector3 result)
        {
            shapes[currentShape].SupportMapping(ref direction, out result);
        }
    }
}
