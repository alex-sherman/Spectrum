using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Spectrum.Framework.Screens
{
    public enum LinearLayoutType
    {
        Vertical,
        Horizontal
    }
    public class LinearLayout : Element
    {
        public LinearLayout(LinearLayoutType type = LinearLayoutType.Vertical)
        {
            LayoutManager = new LinearLayoutManager(type);
        }
    }
    public class LinearLayoutManager : LayoutManager
    {
        public LinearLayoutType LayoutType;

        public int ContentWidth(Element element)
        {
            switch (LayoutType)
            {
                case LinearLayoutType.Vertical:
                    return layoutChildren(element).Select(child => child.MeasuredWidth).DefaultIfEmpty(0).Max();
                case LinearLayoutType.Horizontal:
                    return layoutChildren(element).Select(child => child.MeasuredWidth).DefaultIfEmpty(0).Sum();
            }
            return 0;
        }

        public int ContentHeight(Element element)
        {
            switch (LayoutType)
            {
                case LinearLayoutType.Vertical:
                    return layoutChildren(element).Select(child => child.MeasuredHeight).DefaultIfEmpty(0).Sum();
                case LinearLayoutType.Horizontal:
                    return layoutChildren(element).Select(child => child.MeasuredHeight).DefaultIfEmpty(0).Max();
            }
            return 0;
        }

        public LinearLayoutManager(LinearLayoutType type = LinearLayoutType.Vertical)
        {
            LayoutType = type;
        }
        public void OnLayout(Element element, Rectangle bounds)
        {
            int currentX = -element.ScrollX;
            int currentY = -element.ScrollY;
            Rectangle relativeBounds = bounds;
            relativeBounds.X = relativeBounds.Y = 0;
            foreach (var item in element.Children)
            {
                if (item.Positioning != PositionType.Relative && item.Positioning != PositionType.Absolute)
                {
                    item.Layout(new Rectangle(currentX, currentY, item.MeasuredWidth, item.MeasuredHeight));
                    if (LayoutType == LinearLayoutType.Vertical)
                        currentY += item.MeasuredHeight;
                    if (LayoutType == LinearLayoutType.Horizontal)
                        currentX += item.MeasuredWidth;
                }
                else
                    item.Layout(new Rectangle(item.X, item.Y, item.MeasuredWidth, item.MeasuredHeight));
            }
        }
        IEnumerable<Element> layoutChildren(Element element) => element.Children.Where(c => c.IsInline);
        public void OnMeasure(Element element, int width, int height)
        {
            var layoutChildren = this.layoutChildren(element);
            foreach (var child in layoutChildren)
                child.Measure(element.PreChildWidth(width, element.ContentWidth), element.PreChildHeight(height, element.ContentHeight));
            element.MeasuredWidth = element.MeasureWidth(width, element.ContentWidth);
            element.MeasuredHeight = element.MeasureHeight(height, element.ContentHeight);
        }
    }
}
