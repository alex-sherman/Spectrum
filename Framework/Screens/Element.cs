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
        Visible,
        Hidden
    }
    public enum PositionType
    {
        Inline,
        Absolute
    }
    public class RootElement : Element
    {
        public ScreenManager Manager { get; private set; }
        public RootElement(ScreenManager manager) : base(null) { Manager = manager; }
        public override int Width
        {
            get
            {
                return Manager.Viewport.Width;
            }
        }
        public override int Height
        {
            get
            {
                return Manager.Viewport.Height;
            }
        }
    }
    public class Element : IDisposable
    {
        public Element Parent { get; private set; }
        public List<Element> Children { get; private set; }
        public ElementDisplay Display { get; set; }
        public PositionType Positioning { get; set; }

        public Element(Element parent)
        {
            Parent = parent;
            Display = ElementDisplay.Visible;
            Positioning = PositionType.Inline;
            Children = new List<Element>();
        }

        public virtual void Dispose()
        {
            foreach (Element child in Children)
            {
                child.Dispose();
            }
        }

        public virtual bool HandleInput(bool otherTookInput, InputState input)
        {
            foreach (Element child in Children)
            {
                if (child.Display == ElementDisplay.Visible)
                    otherTookInput |= child.HandleInput(otherTookInput, input);
            }
            return otherTookInput;
        }

        public RectOffset Margin;

        public float RelativeWidth;
        public int FlatWidth;
        public virtual int Width { get { return (int)((Parent == null ? ScreenManager.CurrentManager.Viewport.Width : Parent.Width) * RelativeWidth) + FlatWidth; } }

        public float RelativeHeight;
        public int FlatHeight;
        public virtual int Height { get { return (int)((Parent == null ? ScreenManager.CurrentManager.Viewport.Height : Parent.Height) * RelativeHeight) + FlatHeight; } }

        public int X;
        public int Y;

        public Rectangle Rect
        {
            get { return new Rectangle((int)X, (int)Y, (int)Width, (int)Height); }
        }

        public virtual void Update(GameTime gameTime)
        {
            foreach (Element child in Children)
            {
                if (child.Display == ElementDisplay.Visible)
                    child.Update(gameTime);
            }
        }

        public virtual void PositionUpdate()
        {
            int XOffset = 0;
            int YOffset = 0;
            int MaxRowHeight = 0;
            foreach (Element child in Children)
            {
                MaxRowHeight = Math.Max(MaxRowHeight, child.Height + child.Margin.Top(Height) + child.Margin.Bottom(Width));
                if (XOffset > 0 && XOffset + child.Width + child.Margin.Left(Width) > Width)
                {
                    XOffset = 0;
                    YOffset += MaxRowHeight;
                    MaxRowHeight = 0;
                }
                child.X = XOffset + X + child.Margin.Left(Width);
                child.Y = YOffset + Y + child.Margin.Top(Height);
                child.PositionUpdate();
                XOffset += child.Width + child.Margin.Left(Width) + child.Margin.Right(Width);
            }
        }

        public virtual void Draw(GameTime gameTime, float layer) { }

        public virtual void DrawWithChildren(GameTime gameTime, float layer)
        {
            Draw(gameTime, layer);
            foreach (Element child in Children)
            {
                if (child.Display == ElementDisplay.Visible)
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

        public virtual ElementDisplay Toggle()
        {
            Display = Display == ElementDisplay.Visible ? ElementDisplay.Hidden : ElementDisplay.Visible;
            return Display;
        }
    }
}
