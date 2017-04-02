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
        InlineLeft,
        InlineRight,
        Center,
        Relative,
        Absolute
    }
    public enum LayoutType
    {
        Vertical,
        Horizontal,
        Grid
    }
    public enum SizeConstraint
    {
        AtMost,
        Unspecified
    }
    public struct MeasureSpec
    {
        public SizeConstraint constraint;
        public int size;
        public MeasureSpec(int size, SizeConstraint constraint = SizeConstraint.AtMost)
        {
            this.size = size;
            this.constraint = constraint;
        }
    }
    public class RootElement : Element
    {
        public RootElement(ScreenManager manager) : base(manager) { }
        public override int MeasuredWidth
        {
            get
            {
                return Manager.Viewport.Width;
            }
        }
        public override int MeasuredHeight
        {
            get
            {
                return Manager.Viewport.Height;
            }
        }
    }
    #endregion

    public class Element
    {
        public static SpriteFont DefaultFont;
        public ScreenManager Manager { get; private set; }
        public Element Parent { get; private set; }
        public Dictionary<string, ElementField> Fields = new Dictionary<string, ElementField>();
        private List<Element> _children = new List<Element>();
        public List<Element> Children { get { return _children.ToList(); } }
        public ElementDisplay Display { get; set; }
        public PositionType Positioning { get; set; }
        public LayoutType LayoutType { get; set; }
        public bool HasFocus { get; protected set; }
        private bool Initialized = false;
        public List<string> Tags = new List<string>();
        public SpriteFont Font { get { return Fields["font"].ObjValue as SpriteFont; } }
        public Color FontColor { get { return (Color)(Fields["font-color"].ObjValue ?? Color.Black); } }
        public ImageAsset Texture { get { return Fields["image"].ObjValue as ImageAsset; } }
        public Color TextureColor { get { return (Color)(Fields["image-color"].ObjValue ?? Color.White); } }
        public ImageAsset Background { get { return Fields["background"].ObjValue as ImageAsset; } }
        public Color BackgroundColor { get { return (Color)(Fields["background-color"].ObjValue ?? Color.White); } }

        protected Element(ScreenManager manager) : this() { Manager = manager; }

        public Element()
        {
            Display = ElementDisplay.Visible;
            Positioning = PositionType.InlineLeft;
            this.Fields["font"] = new ElementField(
                this,
                "font",
                ElementField.ContentSetter<SpriteFont>,
                defaultValue: DefaultFont
                );
            this.Fields["font-color"] = new ElementField(
                this,
                "font-color",
                ElementField.ColorSetter
                );
            this.Fields["background"] = new ElementField(
                this,
                "background",
                ElementField.ContentSetter<ImageAsset>,
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
                ElementField.ContentSetter<ImageAsset>,
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
            set { if (Fields[key].StrValue != value) Fields[key].StrValue = value; }
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
        public virtual int MeasuredWidth { get; protected set; }

        public ElementSize Height;
        public virtual int MeasuredHeight { get; protected set; }

        public void Center()
        {
            Margin.LeftRelative = .5f;
            Margin.RightRelative = .5f;
            Margin.LeftOffset = -MeasuredWidth / 2;
            Margin.RightOffset = MeasuredWidth / 2;
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
            get { return new Rectangle((int)AbsoluteX, (int)AbsoluteY, (int)MeasuredWidth, (int)MeasuredHeight); }
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
        public virtual void OnMeasure(MeasureSpec width, MeasureSpec height, float contentWidth, float contentHeight)
        {
            MeasuredWidth = (int)Width.Measure(width.size, contentWidth);
            MeasuredHeight = (int)Height.Measure(height.size, contentHeight);
        }
        private void MeasureChildren(MeasureSpec width, MeasureSpec height, out float contentWidth, out float contentHeight)
        {
            foreach (var child in Children)
            {
                child.Measure(width, height);
            }
            contentWidth = 0;
            contentHeight = 0;
            switch (LayoutType)
            {
                case LayoutType.Vertical:
                    contentHeight = Children.Select(child => child.MeasuredHeight).DefaultIfEmpty(0).Sum();
                    contentWidth = Children.Select(child => child.MeasuredWidth).DefaultIfEmpty(0).Max();
                    break;
                case LayoutType.Horizontal:
                    contentHeight = Children.Select(child => child.MeasuredHeight).DefaultIfEmpty(0).Max();
                    contentWidth = Children.Select(child => child.MeasuredWidth).DefaultIfEmpty(0).Sum();
                    break;
                case LayoutType.Grid:
                default:
                    break;
            }
        }
        public virtual void Measure(MeasureSpec width, MeasureSpec height)
        {
            switch (Width.Type)
            {
                case SizeType.WrapContent:
                    width.size = 0;
                    width.constraint = SizeConstraint.Unspecified;
                    break;
                case SizeType.Flat:
                    width.size = (int)Width.Size;
                    width.constraint = SizeConstraint.AtMost;
                    break;
                case SizeType.Relative:
                    width.size = (int)Width.Size * width.size;
                    break;
                case SizeType.MatchParent:
                default:
                    break;
            }
            if (Height.Type == SizeType.WrapContent)
            {
                height.size = 0;
                height.constraint = SizeConstraint.Unspecified;
            }
            float contentWidth, contentHeight;
            MeasureChildren(width, height, out contentWidth, out contentHeight);
            OnMeasure(width, height, contentWidth, contentHeight);
            MeasureChildren(new MeasureSpec(MeasuredWidth), new MeasureSpec(MeasuredHeight), out contentWidth, out contentHeight);
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
                    case PositionType.InlineLeft:
                        if ((XOffset > 0 && XOffset + child.MeasuredWidth + child.Margin.Left(MeasuredWidth) > MeasuredWidth))
                        {
                            XOffset = 0;
                            YOffset += MaxRowHeight;
                            MaxRowHeight = 0;
                        }
                        MaxRowHeight = Math.Max(MaxRowHeight, child.MeasuredHeight + child.Margin.Top(MeasuredHeight) + child.Margin.Bottom(MeasuredWidth));
                        child.AbsoluteX = XOffset + AbsoluteX + child.Margin.Left(MeasuredWidth);
                        child.AbsoluteY = YOffset + AbsoluteY + child.Margin.Top(MeasuredHeight);
                        XOffset += child.MeasuredWidth + child.Margin.Left(MeasuredWidth) + child.Margin.Right(MeasuredWidth);
                        break;
                    //case PositionType.Absolute:
                    //    child.AbsoluteY = (int)child.Y.Apply(size: Manager.Root.MeasuredHeight);
                    //    child.AbsoluteX = (int)child.X.Apply(size: Manager.Root.MeasuredWidth);
                    //    break;
                    //case PositionType.Relative:
                    //    child.AbsoluteY = (int)child.Y.Apply(size: MeasuredHeight, offset: AbsoluteY);
                    //    child.AbsoluteX = (int)child.X.Apply(size: MeasuredWidth, offset: AbsoluteX);
                    //    break;
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
                Texture.Draw(spritebatch, Rect, TextureColor, Layer(1));
            }
            if (Background != null)
            {
                Background.Draw(spritebatch, Rect, BackgroundColor, Z);
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

        public virtual ElementDisplay Toggle(bool? show = null)
        {
            Display = (!show ?? (Display == ElementDisplay.Visible)) ? ElementDisplay.Hidden : ElementDisplay.Visible;
            return Display;
        }
    }
}
