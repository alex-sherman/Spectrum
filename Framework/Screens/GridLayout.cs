using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Screens
{
    public class GridLayout : Element
    {
        public GridLayout(int cols) { LayoutManager = new GridLayoutManager(cols); }
    }
    public class GridLayoutManager : LayoutManager
    {
        int Cols;
        public GridLayoutManager(int cols) { Cols = cols; }
        public void OnLayout(Element element, Rectangle bounds)
        {
            int curRow = 0;
            int curCol = 0;
            foreach (var item in element.Children)
            {
                item.Layout(new Rectangle(curCol * bounds.Width / Cols, rowHeights.Where((h, i) => i < curRow).Sum(), bounds.Width / Cols, rowHeights[curRow]));
                curCol += 1;
                if (curCol == Cols)
                {
                    curRow += 1;
                    curCol = 0;
                }
            }
        }
        private List<int> rowHeights = new List<int>();
        private List<int> colWidths = new List<int>();
        public void OnMeasure(Element element, int width, int height)
        {
            rowHeights.Clear();
            colWidths.Clear();
            colWidths.AddRange(Enumerable.Repeat(0, Cols));
            int curRow = 0;
            int curCol = 0;
            int curRowHeight = 0;
            int childWidth = width;
            childWidth /= Cols;
            int childHeight = height;
            foreach (var child in element.Children)
            {
                child.Measure(width, height);
                curRowHeight = Math.Max(child.MeasuredHeight, curRowHeight);
                colWidths[curCol] = Math.Max(colWidths[curCol], child.MeasuredWidth);
                curCol += 1;
                if (curCol == Cols)
                {
                    rowHeights.Add(curRowHeight);
                    curRowHeight = 0;
                    curRow += 1;
                    curCol = 0;
                }
            }
            rowHeights.Add(curRowHeight);
            if (element.Height.WrapContent)
                element.MeasuredHeight = rowHeights.DefaultIfEmpty(0).Sum();
            if (element.Width.WrapContent)
                element.MeasuredWidth = colWidths.DefaultIfEmpty(0).Sum();
        }
    }
}
