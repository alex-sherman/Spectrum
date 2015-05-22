﻿using Microsoft.Xna.Framework;
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
        public RootElement(ScreenManager manager) : base(manager) { }
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
    public class Element
    {
        public ScreenManager Manager { get; private set; }
        public Element Parent { get; private set; }
        private List<Element> _children = new List<Element>();
        public List<Element> Children { get { return _children.ToList(); } }
        public ElementDisplay Display { get; set; }
        public PositionType Positioning { get; set; }

        protected Element(ScreenManager manager) : this() { Manager = manager; }

        public Element()
        {
            Display = ElementDisplay.Visible;
            Positioning = PositionType.Inline;
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
        protected const float ZDiff = 0.00001f;
        protected const int ZLayers = 10;
        public float Z { get; private set; }
        protected float Layer(int layer)
        {
            return Z - ZDiff * layer / ZLayers;
        }

        public Rectangle Rect
        {
            get { return new Rectangle((int)X, (int)Y, (int)Width, (int)Height); }
        }

        public virtual void Update(GameTime gameTime)
        {
            foreach (Element child in Children)
            {
                child.Parent = this;
                child.Manager = Manager;
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
                if (child.Positioning == PositionType.Inline)
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
                    XOffset += child.Width + child.Margin.Left(Width) + child.Margin.Right(Width);
                }
                child.PositionUpdate();
            }
        }

        public virtual void Draw(GameTime gameTime) { }

        public virtual float DrawWithChildren(GameTime gameTime, float layer)
        {
            Z = layer;
            Draw(gameTime);
            List<Element> drawChildren = Children;
            drawChildren.Reverse();
            foreach (Element child in drawChildren)
            {
                if (child.Display == ElementDisplay.Visible)
                    layer = child.DrawWithChildren(gameTime, layer * .9999f);
            }
            return layer * .9999f;
        }
        public void MoveElement(Element child, int newIndex)
        {
            _children.Remove(child);
            _children.Insert(newIndex, child);
        }
        public virtual void AddElement(Element element, int? index = null)
        {
            element.Parent = this;
            element.Manager = Manager;
            _children.Insert(index ?? _children.Count, element);
        }
        public virtual void RemoveElement(Element element)
        {
            element.Parent = null;
            _children.Remove(element);
        }

        public virtual ElementDisplay Toggle()
        {
            Display = Display == ElementDisplay.Visible ? ElementDisplay.Hidden : ElementDisplay.Visible;
            return Display;
        }
    }
}
