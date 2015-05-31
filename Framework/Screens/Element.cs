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
        private bool Initialized = false;
        public List<string> Tags = new List<string>();
        public SpriteFont Font { get { return Fields["font"].ObjValue as SpriteFont; } }
        public Color FontColor { get { return (Color)(Fields["font-color"].ObjValue ?? Color.Black); } }
        public ScalableTexture Texture { get { return Fields["image"].ObjValue as ScalableTexture; } }
        public Color TextureColor { get { return (Color)(Fields["image-color"].ObjValue ?? Color.White); } }

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

        public float RelativeWidth;
        public int FlatWidth;
        public virtual int Width { get { return (int)((Parent == null ? ScreenManager.CurrentManager.Viewport.Width : Parent.Width) * RelativeWidth) + FlatWidth; } }

        public float RelativeHeight;
        public int FlatHeight;
        public virtual int Height { get { return (int)((Parent == null ? ScreenManager.CurrentManager.Viewport.Height : Parent.Height) * RelativeHeight) + FlatHeight; } }

        public void Center()
        {
            Margin.LeftRelative = .5f;
            Margin.RightRelative = .5f;
            Margin.LeftOffset = -Width / 2;
            Margin.RightOffset = Width / 2;
        }

        public int X;
        public int Y;
        protected const float ZDiff = 0.00001f;
        protected const int ZLayers = 10;
        public float Z { get; private set; }
        public float Layer(int layer)
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
                    if (XOffset > 0 && XOffset + child.Width + child.Margin.Left(Width) > Width)
                    {
                        XOffset = 0;
                        YOffset += MaxRowHeight;
                        MaxRowHeight = 0;
                    }
                    MaxRowHeight = Math.Max(MaxRowHeight, child.Height + child.Margin.Top(Height) + child.Margin.Bottom(Width));
                    child.X = XOffset + X + child.Margin.Left(Width);
                    child.Y = YOffset + Y + child.Margin.Top(Height);
                    XOffset += child.Width + child.Margin.Left(Width) + child.Margin.Right(Width);
                }
                child.PositionUpdate();
            }
        }

        public virtual void Draw(GameTime gameTime)
        {
            if (Texture != null)
            {
                Texture.Draw(Rect, ScreenManager.CurrentManager.SpriteBatch, Z, color: TextureColor);
            }
        }

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
            if (element.Initialized)
                throw new Exception("Element already initiliazed cannot be added to a new parent");
            element.Parent = this;
            element.Manager = Manager;
            element.Initialize();
            _children.Insert(index ?? _children.Count, element);
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
