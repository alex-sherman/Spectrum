using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens
{
    #region DataTypes
    public enum ElementDisplay
    {
        Visible,
        Hidden
    }
    public enum PositionType
    {
        Inline,
        Relative,
        Absolute
    }
    public class RootElement : Element
    {
        public RootElement(ScreenManager manager) : base(manager) { }
        public override int TotalWidth
        {
            get
            {
                return Manager.Viewport.Width;
            }
        }
        public override int TotalHeight
        {
            get
            {
                return Manager.Viewport.Height;
            }
        }
    }
    public class LineBreak : Element { }
    #endregion

    public class Element
    {
        public ScreenManager Manager { get; private set; }
        public Element Parent { get; private set; }
        public Dictionary<string, ElementField> Fields = new Dictionary<string, ElementField>();
        private List<Element> _children = new List<Element>();
        public List<Element> Children { get { return _children.ToList(); } }
        public ElementDisplay Display { get; set; }
        public PositionType Positioning { get; set; }
        public bool HasFocus { get; protected set; }
        private bool Initialized = false;
        public List<string> Tags = new List<string>();
        public SpriteFont Font { get { return Fields["font"].ObjValue as SpriteFont; } }
        public Color FontColor { get { return (Color)(Fields["font-color"].ObjValue ?? Color.Black); } }
        public ScalableTexture Texture { get { return Fields["image"].ObjValue as ScalableTexture; } }
        public Color TextureColor { get { return (Color)(Fields["image-color"].ObjValue ?? Color.White); } }
        public ScalableTexture Background { get { return Fields["background"].ObjValue as ScalableTexture; } }
        public Color BackgroundColor { get { return (Color)(Fields["background-color"].ObjValue ?? Color.White); } }

        protected Element(ScreenManager manager) : this() { Manager = manager; }

        public Element()
        {
            Display = ElementDisplay.Visible;
            Positioning = PositionType.Inline;
            this.Fields["font"] = new ElementField(
                this,
                "font",
                ElementField.ContentSetter<SpriteFont>
                );
            this.Fields["font-color"] = new ElementField(
                this,
                "font-color",
                ElementField.ColorSetter
                );
            this.Fields["background"] = new ElementField(
                this,
                "background",
                ElementField.ContentSetter<ScalableTexture>,
                false
                );
            this.Fields["background-color"] = new ElementField(
                this,
                "background-color",
                ElementField.ColorSetter,
                false
                );
            this.Fields["image"] = new ElementField(
                this,
                "image",
                ElementField.ContentSetter<ScalableTexture>,
                false
                );
            this.Fields["image-color"] = new ElementField(
                this,
                "image-color",
                ElementField.ColorSetter,
                false
                );
        }

        public virtual void Initialize()
        {
            Type tagType = GetType();
            while (tagType != typeof(Element))
            {
                this.Tags.Add(tagType.Name.ToLower());
                tagType = tagType.BaseType;
            }
            foreach (ElementField field in Fields.Values)
            {
                field.Initialize();
            }
            Initialized = true;
        }

        public string this[string key]
        {
            get { return Fields[key].StrValue; }
            set { Fields[key].StrValue = value; }
        }

        public List<Element> FindAll(Func<Element, bool> Predicate)
        {
            List<Element> output = new List<Element>();
            foreach (Element child in Children)
            {
                if (Predicate(child))
                    output.Add(child);
                output.AddRange(child.FindAll(Predicate));
            }
            return output;
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
        public RectOffset Padding;

        public ElementSize Width;
        public virtual int TotalWidth { get { return (int)Width.Apply(size: Parent == null ? ScreenManager.CurrentManager.Viewport.Width : Parent.TotalWidth); } }

        public ElementSize Height;
        public virtual int TotalHeight { get { return (int)Height.Apply(size: Parent == null ? ScreenManager.CurrentManager.Viewport.Height : Parent.TotalHeight); } }

        public void Center()
        {
            Margin.LeftRelative = .5f;
            Margin.RightRelative = .5f;
            Margin.LeftOffset = -TotalWidth / 2;
            Margin.RightOffset = TotalWidth / 2;
        }

        public ElementSize X;
        public ElementSize Y;
        public int AbsoluteX { get; private set; }
        public int AbsoluteY { get; private set; }
        protected const float ZDiff = 0.00001f;
        protected const int ZLayers = 10;
        public float Z { get; private set; }
        public float Layer(int layer)
        {
            return Z - ZDiff * layer / ZLayers;
        }

        public Rectangle Rect
        {
            get { return new Rectangle((int)AbsoluteX, (int)AbsoluteY, (int)TotalWidth, (int)TotalHeight); }
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
                switch (child.Positioning)
                {
                    case PositionType.Inline:
                        if ((XOffset > 0 && XOffset + child.TotalWidth + child.Margin.Left(TotalWidth) > TotalWidth) || child is LineBreak)
                        {
                            XOffset = 0;
                            YOffset += MaxRowHeight;
                            MaxRowHeight = 0;
                            if (child is LineBreak)
                                continue;
                        }
                        MaxRowHeight = Math.Max(MaxRowHeight, child.TotalHeight + child.Margin.Top(TotalHeight) + child.Margin.Bottom(TotalWidth));
                        child.AbsoluteX = XOffset + AbsoluteX + child.Margin.Left(TotalWidth);
                        child.AbsoluteY = YOffset + AbsoluteY + child.Margin.Top(TotalHeight);
                        XOffset += child.TotalWidth + child.Margin.Left(TotalWidth) + child.Margin.Right(TotalWidth);
                        break;
                    case PositionType.Absolute:
                        child.AbsoluteY = (int)child.Y.Apply(size: Manager.Root.TotalHeight);
                        child.AbsoluteX = (int)child.X.Apply(size: Manager.Root.TotalWidth);
                        break;
                    case PositionType.Relative:
                        child.AbsoluteY = (int)child.Y.Apply(size: TotalHeight, offset: AbsoluteY);
                        child.AbsoluteX = (int)child.X.Apply(size: TotalWidth, offset: AbsoluteX);
                        break;
                    default:
                        break;
                }
                child.PositionUpdate();
            }
        }
        public virtual bool UpdateFocus(bool parentHasFocus)
        {
            foreach (var child in Children)
            {
                child.UpdateFocus(parentHasFocus);
            }
            HasFocus = parentHasFocus;
            return parentHasFocus;
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spritebatch)
        {
            if (Texture != null)
            {
                Texture.Draw(Rect, ScreenManager.CurrentManager.SpriteBatch, Layer(1), color: TextureColor);
            }
            if (Background != null)
            {
                Background.Draw(Rect, ScreenManager.CurrentManager.SpriteBatch, Z, color: BackgroundColor);
            }
        }

        public virtual float DrawWithChildren(GameTime gameTime, SpriteBatch spritebatch, float layer)
        {
            Z = layer;
            Draw(gameTime, spritebatch);
            List<Element> drawChildren = Children;
            drawChildren.Reverse();
            foreach (Element child in drawChildren)
            {
                if (child.Display == ElementDisplay.Visible)
                {
                    layer = child.DrawWithChildren(gameTime, spritebatch, layer * .9999f);
                }
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
            if (element.Initialized)
                throw new Exception("Element already initiliazed cannot be added to a new parent");
            element.Parent = this;
            element.Manager = Manager;
            _children.Insert(index ?? _children.Count, element);
            element.Initialize();
            PositionUpdate();
        }
        public virtual void RemoveElement(Element element)
        {
            _children.Remove(element);
        }

        public virtual ElementDisplay Toggle()
        {
            Display = Display == ElementDisplay.Visible ? ElementDisplay.Hidden : ElementDisplay.Visible;
            return Display;
        }
    }
}
