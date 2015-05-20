using Microsoft.Xna.Framework;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens
{
    public enum ElementDisplay
    {
        Visisble,
        Hidden
    }
    public enum PositionType
    {
        Inline,
        Absolute
    }
    public class Element
    {
        public Element Parent { get; private set; }
        public List<Element> Children { get; private set; }
        public ElementDisplay Display { get; set; }
        public PositionType Positioning { get; set; }

        public Element(Element parent)
        {
            Parent = parent;
            Display = ElementDisplay.Visisble;
            Positioning = PositionType.Inline;
            Children = new List<Element>();
        }

        public virtual bool HandleInput(bool otherTookInput, InputState input)
        {
            return false;
        }

        public RectOffset Margin;

        public float RelativeWidth;
        public int FlatWidth;
        public int Width { get { return (int)((Parent == null ?  ScreenManager.CurrentManager.Viewport.Width : Parent.Width) * RelativeWidth) + FlatWidth; } }

        public float RelativeHeight;
        public int FlatHeight;
        public int Height { get { return (int)((Parent == null ? ScreenManager.CurrentManager.Viewport.Height : Parent.Height) * RelativeHeight) + FlatHeight; } }

        public int X;
        public int Y;

        public Rectangle Rect
        {
            get { return new Rectangle((int)X, (int)Y, (int)Width, (int)Height); }
        }

        public virtual void PositionUpdate()
        {
            int XOffset = 0;
            int YOffset = 0;
            int MaxRowHeight = 0;
            foreach (Element child in Children)
            {
                child.X = XOffset + X + child.Margin.Left(Width - child.Width);
                child.Y = YOffset + Y + child.Margin.Top(Height - child.Height);
                MaxRowHeight = Math.Max(MaxRowHeight, child.Height);
                XOffset += child.Width + child.Margin.Left(Width - child.Width) + child.Margin.Right(Width - child.Width);
                if (XOffset > Width)
                {
                    XOffset = 0;
                    YOffset += MaxRowHeight;
                    MaxRowHeight = 0;
                }
            }
        }

        public virtual void Draw(GameTime gameTime, float layer) { }

        public virtual void DrawWithChildren(GameTime gameTime, float layer)
        {
            Draw(gameTime, layer);
            foreach (Element child in Children)
            {
                child.DrawWithChildren(gameTime, ScreenManager.Layer(1, layer));
            }
        }

        public virtual void AddElement(Element element)
        {
            Children.Add(element);
        }
        public virtual void RemoveElement(Element element)
        {
            Children.Remove(element);
        }
    }
}
