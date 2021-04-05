using Spectrum.Framework;
using Spectrum.Framework.Content;
using Spectrum.Framework.Screens;
using Spectrum.Framework.VR;
using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Screens.InputElements
{
    public class SelectionWheel : VRMenu
    {
        public int SquareSize = 400;
        public float OR = 200;
        public float IR = 120;
        public int Count = 4;
        double halfAngle;
        Element[] highlightElements;
        string SVGVec(Vector2 vector)
        {
            return string.Format("{0} {1}", vector.X.ToString("n5"), vector.Y.ToString("n5"));
        }
        public override void Initialize()
        {
            RenderTargetSize = new Point(400, 400);
            var viewBox = new SvgViewBox(-SquareSize / 2, -SquareSize / 2, SquareSize, SquareSize);
            Size = new Vector2(0.2f, 0.2f);
            Offset = new Transform(Vector3.Up * 0.02f + Vector3.Backward * 0.1f);
            base.Initialize();
            highlightElements = new Element[Count];
            halfAngle = Math.PI / Count;
            for (int i = 0; i < Count; i++)
            {
                var angle0 = halfAngle * (i * 2 - 0.8) - Math.PI / 2;
                var angle1 = halfAngle * (i * 2 + 0.8) - Math.PI / 2;
                Vector2 p0or = new Vector2((float)Math.Cos(angle0) * OR, (float)Math.Sin(angle0) * OR);
                Vector2 p0ir = new Vector2((float)Math.Cos(angle0) * IR, (float)Math.Sin(angle0) * IR);
                Vector2 p1or = new Vector2((float)Math.Cos(angle1) * OR, (float)Math.Sin(angle1) * OR);
                Vector2 p1ir = new Vector2((float)Math.Cos(angle1) * IR, (float)Math.Sin(angle1) * IR);
                string arcPath = string.Format("M {2} L {3} A {1} {1} 0 0 1 {4} L {5} A {0} {0} 0 0 0 {2} z", IR, OR, SVGVec(p0ir), SVGVec(p0or), SVGVec(p1or), SVGVec(p1ir));
                var arc = new SvgPath()
                {
                    Fill = new SvgColourServer(System.Drawing.Color.White),
                    ShapeRendering = SvgShapeRendering.CrispEdges,
                    PathData = SvgPathBuilder.Parse(arcPath)
                };
                var angleC = halfAngle * i * 2 - Math.PI / 2;
                var childCenter = new Vector2((float)Math.Cos(angleC) * (OR + IR) / 2, (float)Math.Sin(angleC) * (OR + IR) / 2) + new Vector2(SquareSize) / 2;
                var offset = (OR - IR) / 2;
                var arcElement = highlightElements[i] = new Element()
                {
                    Positioning = PositionType.Absolute,
                    Height = 1.0,
                    Width = 1.0,
                    Padding = new RectOffset()
                    {
                        Left = (int)(childCenter.X - offset),
                        Top = (int)(childCenter.Y - offset),
                        Bottom = (int)(SquareSize - childCenter.Y - offset),
                        Right = (int)(SquareSize - childCenter.X - offset)
                    },
                };
                var highlightSvg = new SvgDocument() { ViewBox = viewBox };
                highlightSvg.Children.Add(arc);
                arcElement.Background = new ImageAsset() { SVG = highlightSvg };
                Root.AddElement(arcElement);
            }
        }
        public int GetIndex(float angle)
        {
            return ((int)Math.Ceiling(2 * Count - ((angle - Math.PI / 2) / halfAngle + 1) / 2)) % Count;
        }
        public Element GetElement(int index)
        {
            return highlightElements[index];
        }
        public void Highlight(int index)
        {
            Highlight(null);
            highlightElements[index].AddTag("highlight");
        }
        public void Highlight(bool[] highlight)
        {
            foreach (var ele in highlightElements)
                ele.RemoveTag("highlight");
            if (highlight == null) return;
            for (int i = 0; i < highlightElements.Length && i < highlight.Length; i++)
            {
                if (highlight[i])
                    highlightElements[i].AddTag("highlight");
            }
        }
    }
}
