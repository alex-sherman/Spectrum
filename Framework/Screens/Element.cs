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
    #endregion

    public class Element
    {
        public static SpriteFont DefaultFont;
        public LayoutManager LayoutManager { get; protected set; }
        public Element Parent { get; private set; }
        public Dictionary<string, ElementField> Fields = new Dictionary<string, ElementField>();
        private List<Element> _children = new List<Element>();
        public List<Element> Children { get { return _children.ToList(); } }
        public ElementDisplay Display { get; set; }
        public PositionType Positioning { get; set; }
        public virtual bool HasFocus { get { return Parent?.HasFocus ?? true; } }
        private bool Initialized = false;
        public List<string> Tags = new List<string>();
        public SpriteFont Font { get { return Fields["font"].ObjValue as SpriteFont; } }
        public Color FontColor { get { return (Color)(Fields["font-color"].ObjValue ?? Color.Black); } }
        public ImageAsset Texture
        {
            get { return Fields["image"].ObjValue as ImageAsset; }
            set { Fields["image"].SetValue(null, value); }
        }
        public Color TextureColor { get { return (Color)(Fields["image-color"].ObjValue ?? Color.White); } }
        public ImageAsset Background { get { return Fields["background"].ObjValue as ImageAsset; } }
        public Color? BackgroundColor
        {
            get => Fields["background-color"].ObjValue as Color?;
            set => Fields["background-color"].SetValue(null, value);
        }

        public Element()
        {
            Display = ElementDisplay.Visible;
            Positioning = PositionType.InlineLeft;
            Fields["font"] = new ElementField(
                this,
                "font",
                ElementField.ContentSetter<SpriteFont>,
                defaultValue: DefaultFont
                );
            Fields["font-color"] = new ElementField(
                this,
                "font-color",
                (value) => ElementField.ColorSetter(value)
                );
            Fields["background"] = new ElementField(
                this,
                "background",
                ElementField.ContentSetter<ImageAsset>,
                false
                );
            Fields["background-color"] = new ElementField(
                this,
                "background-color",
                (value) => ElementField.ColorSetter(value),
                false
                );
            Fields["image"] = new ElementField(
                this,
                "image",
                ElementField.ContentSetter<ImageAsset>,
                false
                );
            Fields["image-color"] = new ElementField(
                this,
                "image-color",
                (value) => ElementField.ColorSetter(value),
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
        public int MeasuredWidth { get; set; }

        public ElementSize Height;
        public int MeasuredHeight { get; set; }

        public void Center()
        {
            Margin.LeftRelative = .5f;
            Margin.RightRelative = .5f;
            Margin.LeftOffset = -MeasuredWidth / 2;
            Margin.RightOffset = MeasuredWidth / 2;
        }

        public int X;
        public int Y;
        public int AbsoluteX { get; private set; }
        public int AbsoluteY { get; private set; }
        protected const float ZDiff = 0.00001f;
        protected const int ZLayers = 10;
        public float Z { get; private set; }
        public float Layer(int layer)
        {
            return Z - ZDiff * layer / ZLayers;
        }
        /// <summary>
        /// A bounding rectangle with a relative offset that accounts for padding from the parent but is not necessarily relative to the parent
        /// </summary>
        public Rectangle Bounds { get; private set; }
        /// <summary>
        /// The absolute rectangle in terms of screen coordinates for the element
        /// </summary>
        public Rectangle Rect { get; private set; }

        public virtual void Update(GameTime gameTime)
        {
            foreach (Element child in Children)
            {
                child.Parent = this;
                if (child.Display == ElementDisplay.Visible)
                    child.Update(gameTime);
            }
        }
        public virtual void OnMeasure(int width, int height)
        {
            if (LayoutManager != null)
                LayoutManager.OnMeasure(this, width, height);
            else
            {
                MeasuredWidth = Width.Measure(width, Children.Select(c => c.MeasuredWidth).DefaultIfEmpty(0).Max());
                MeasuredHeight = Height.Measure(height, Children.Select(c => c.MeasuredHeight).DefaultIfEmpty(0).Max());
            }
        }
        public virtual void Measure(int width, int height)
        {
            if (Display == ElementDisplay.Hidden)
            {
                MeasuredHeight = 0;
                MeasuredWidth = 0;
            }
            width = Width.CropParentSize(width);
            height = Height.CropParentSize(height);
            foreach (var child in Children)
            {
                child.Measure(width, height);
            }
            OnMeasure(width, height);
            foreach (var child in Children)
            {
                child.Measure(MeasuredWidth, MeasuredHeight);
            }
        }
        public virtual void Layout(Rectangle bounds)
        {
            if (Display == ElementDisplay.Hidden)
                return;
            Bounds = bounds;
            int X = (Positioning == PositionType.Absolute ? 0 : (Parent?.Rect.X ?? 0)) + bounds.X;
            int Y = (Positioning == PositionType.Absolute ? 0 : (Parent?.Rect.Y ?? 0)) + bounds.Y;
            Rect = new Rectangle(X, Y, bounds.Width, bounds.Height);
            if (LayoutManager != null)
                LayoutManager.OnLayout(this, bounds);
            else
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
                            child.Layout(new Rectangle(XOffset, YOffset, child.MeasuredWidth, child.MeasuredHeight));
                            MaxRowHeight = Math.Max(MaxRowHeight, child.MeasuredHeight + child.Margin.Top(MeasuredHeight) + child.Margin.Bottom(MeasuredWidth));
                            XOffset += child.MeasuredWidth + child.Margin.Left(MeasuredWidth) + child.Margin.Right(MeasuredWidth);
                            break;
                        case PositionType.Center:
                            child.Layout(new Rectangle(MeasuredWidth / 2 - child.MeasuredWidth / 2, MeasuredHeight / 2 - child.MeasuredHeight / 2, child.MeasuredWidth, child.MeasuredHeight));
                            break;
                        case PositionType.Absolute:
                        case PositionType.Relative:
                            child.Layout(new Rectangle(child.X, child.Y, child.MeasuredWidth, child.MeasuredHeight));
                            break;
                        default:
                            break;
                    }
                }
            }

        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spritebatch)
        {
            if (Texture != null)
            {
                Texture.Draw(spritebatch, Rect, TextureColor, Layer(1));
            }
            if (Background != null)
            {
                Background.Draw(spritebatch, Rect, BackgroundColor ?? Color.White, Z);
            }
            else if (BackgroundColor != null)
                ImageAsset.Blank.Draw(spritebatch, Rect, BackgroundColor.Value, Z);
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
            _children.Insert(index ?? _children.Count, element);
            element.Initialize();
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
