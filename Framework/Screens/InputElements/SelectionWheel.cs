using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework;
using Spectrum.Framework.Content;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Input;
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
    public class SelectionWheel
    {
        public int SquareSize = 400;
        public float OR = 200;
        public float IR = 120;
        public int Count = 4;
        public int? Index { get; private set; }
        public Point RenderTargetSize = new Point(400, 400);
        public Vector2 Size = new Vector2(0.2f, 0.2f);
        public Transform Transform = new Transform();
        public RootElement Root = new RootElement();
        double halfAngle;
        Element[] highlightElements;
        MaterialData material;
        string SVGVec(Vector2 vector)
        {
            return string.Format("{0} {1}", vector.X.ToString("n5"), vector.Y.ToString("n5"));
        }
        public SelectionWheel()
        {
            Root.Target = new RenderTarget2D(SpectrumGame.Game.GraphicsDevice, RenderTargetSize.X, RenderTargetSize.Y);
            material = new MaterialData() { DiffuseTexture = Root.Target, DisableLighting = true };
            Root.Initialize();
        }
        public void ResetElements()
        {
            Root.Clear();
            var viewBox = new SvgViewBox(-SquareSize / 2, -SquareSize / 2, SquareSize, SquareSize);
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
                    PathData = SvgPathBuilder.Parse(arcPath),
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
                arcElement.AddTag("selection-wheel-arc");
                var highlightSvg = new SvgDocument() { ViewBox = viewBox };
                highlightSvg.Children.Add(arc);
                arcElement.Background = new ImageAsset() { SVG = highlightSvg };
                Root.AddElement(arcElement);
            }
        }
        public void Draw()
        {
            Root.Update(0, InputState.Current);
            Root.Draw(0);
            Billboard.Draw(Transform.World(), Size, material);
        }
        public int GetIndex(float angle)
        {
            return (2 * Count - (int)Math.Ceiling((angle - Math.PI / 2) / halfAngle / 2 - 0.5)) % Count;
        }
        public Element GetElement(int index)
        {
            return highlightElements[index];
        }
        public void Highlight(int? index)
        {
            foreach (var ele in highlightElements)
                ele.RemoveTag("highlight");
            if (index == null) return;
            highlightElements[index.Value].AddTag("highlight");
        }
        public int? UpdateFromCursor(ref Vector2 cursor)
        {
            cursor += new Vector2(InputState.Current.CursorState.DX, InputState.Current.CursorState.DY);
            InputState.Current.CursorState.DX = 0;
            InputState.Current.CursorState.DY = 0;
            int? index;
            if (Count == 0) return null;
            if (cursor.LengthSquared > 20000)
                cursor *= (float)Math.Sqrt(20000 / cursor.LengthSquared);
            if (cursor.LengthSquared > 10000)
                index = GetIndex((float)Math.Atan2(-cursor.Y, cursor.X));
            else
                index = null;
            Highlight(index);
            return index;
        }
    }
}
