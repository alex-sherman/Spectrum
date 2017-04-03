﻿using System;
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
        Horizontal,
        Grid
    }
    public class LinearLayout : Element
    {
        public LinearLayout()
        {

        }
    }
    public class LinearLayoutManager : LayoutManager
    {
        LinearLayoutType LayoutType;
        public void OnLayout(Element element, Rectangle bounds)
        {
            int currentX = 0;
            int currentY = 0;
            foreach (var item in element.Children)
            {
                if (item.Positioning != PositionType.Relative && item.Positioning != PositionType.Absolute)
                {
                    int width = LayoutType == LinearLayoutType.Horizontal ? item.MeasuredWidth :
                        (item.Width.Type == SizeType.MatchParent ? element.MeasuredWidth : item.MeasuredWidth);
                    int height = LayoutType == LinearLayoutType.Vertical ? item.MeasuredHeight :
                        (item.Height.Type == SizeType.MatchParent ? element.MeasuredHeight : item.MeasuredHeight);
                    item.Layout(new Rectangle(currentX, currentY, width, height));
                    if (LayoutType == LinearLayoutType.Vertical)
                        currentY += height;
                    if (LayoutType == LinearLayoutType.Horizontal)
                        currentX += width;
                }
                else
                    item.Layout(new Rectangle(item.X, item.Y, item.MeasuredWidth, item.MeasuredHeight));
            }
        }
        public void OnMeasure(Element element, int width, int height)
        {
            float contentWidth = 0;
            float contentHeight = 0;
            var layoutChildren = element.Children.Where(c => c.Positioning != PositionType.Relative && c.Positioning != PositionType.Absolute);
            switch (LayoutType)
            {
                case LinearLayoutType.Vertical:
                    contentHeight = layoutChildren.Select(child => child.MeasuredHeight).DefaultIfEmpty(0).Sum();
                    contentWidth = layoutChildren.Select(child => child.MeasuredWidth).DefaultIfEmpty(0).Max();
                    break;
                case LinearLayoutType.Horizontal:
                    contentHeight = layoutChildren.Select(child => child.MeasuredHeight).DefaultIfEmpty(0).Max();
                    contentWidth = layoutChildren.Select(child => child.MeasuredWidth).DefaultIfEmpty(0).Sum();
                    break;
                default:
                    break;
            }
            element.MeasuredWidth = (int)element.Width.Measure(width, contentWidth);
            element.MeasuredHeight = (int)element.Height.Measure(height, contentHeight);
        }
    }
}