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
            switch (type)
            {
                case LinearLayoutType.Vertical:
                    Width = 1f;
                    Height.WrapContent = true;
                    break;
                case LinearLayoutType.Horizontal:
                    Height = 1f;
                    Width.WrapContent = true;
                    break;
            }
        }
    }
    public class LinearLayoutManager : LayoutManager
    {
        public LinearLayoutType LayoutType;
        public LinearLayoutManager(LinearLayoutType type)
        {
            LayoutType = type;
        }
        public void OnLayout(Element element, Rectangle bounds)
        {
            int currentX = 0;
            int currentY = 0;
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
        private void getContentDims(IEnumerable<Element> children, ref int contentWidth, ref int contentHeight)
        {
            switch (LayoutType)
            {
                case LinearLayoutType.Vertical:
                    contentHeight = children.Select(child => child.MeasuredHeight).DefaultIfEmpty(0).Sum();
                    contentWidth = children.Select(child => child.MeasuredWidth).DefaultIfEmpty(0).Max();
                    break;
                case LinearLayoutType.Horizontal:
                    contentHeight = children.Select(child => child.MeasuredHeight).DefaultIfEmpty(0).Max();
                    contentWidth = children.Select(child => child.MeasuredWidth).DefaultIfEmpty(0).Sum();
                    break;
            }
        }
        public void OnMeasure(Element element, int width, int height)
        {
            int contentWidth = 0;
            int contentHeight = 0;
            var layoutChildren = element.Children.Where(c => c.Positioning != PositionType.Relative && c.Positioning != PositionType.Absolute);
            getContentDims(layoutChildren, ref contentWidth, ref contentHeight);
            foreach (var child in layoutChildren)
                child.Measure(element.PreChildWidth(width, contentWidth), element.PreChildHeight(height, contentHeight));
            getContentDims(layoutChildren, ref contentWidth, ref contentHeight);
            element.MeasuredWidth = element.MeasureWidth(width, contentWidth);
            element.MeasuredHeight = element.MeasureHeight(height, contentHeight);
        }
    }
}
