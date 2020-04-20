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
        public GridLayout(int cols)
        {
            LayoutManager = new GridLayoutManager(cols);
        }
    }
    public class GridLayoutManager : LayoutManager
    {
        private int Cols;
        public GridLayoutManager(int cols) { Cols = cols; }
        public void OnLayout(Element element, Rectangle bounds)
        {
            int curRow = 0;
            int curCol = 0;
            foreach (var item in element.Children)
            {
                item.Layout(new Rectangle(colWidths.Where((h, i) => i < curCol).Sum(), rowHeights.Where((h, i) => i < curRow).Sum(), colWidths[curCol], rowHeights[curRow]));
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
            int cellSize = element.PreChildWidth(width) / Cols;
            int curRowHeight = 0;
            int childHeight = height;
            foreach (var child in element.Children)
            {
                // Without passing in content dimensions children cannot have a parent percentage
                // that is affected by other childrens' dimensions. Might be fine for grids?
                child.Measure(cellSize, cellSize);
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
            element.MeasuredHeight = element.MeasureHeight(height, rowHeights.DefaultIfEmpty(0).Sum());
            element.MeasuredWidth = element.MeasureWidth(width, colWidths.DefaultIfEmpty(0).Sum());
        }

        public int ContentWidth(Element element)
        {
            return colWidths.DefaultIfEmpty(0).Sum();
        }

        public int ContentHeight(Element element)
        {
            return rowHeights.DefaultIfEmpty(0).Sum();
        }
    }
}
