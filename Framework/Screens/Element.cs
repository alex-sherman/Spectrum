using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.Content;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens
{
    #region DataTypes
    public enum PositionType
    {
        Inline,
        // TODO: Add flags for horizontal vs vertical
        Center,
        Relative,
        Absolute
    }
    struct InputHandler
    {
        public Func<InputState, bool> Handler;
        public KeyPressType PressType;
        public bool RequireDisplay;
        public bool IgnoreConsumed;
    }
    #endregion

    public class Element
    {
        public static SpriteFont DefaultFont;
        public LayoutManager LayoutManager;
        public Element Parent { get; private set; }
        public Dictionary<string, ElementField> Fields = new Dictionary<string, ElementField>();
        private List<Element> _children = new List<Element>();
        public List<Element> Children { get { return _children.ToList(); } }
        private bool _display = true;
        public bool Display
        {
            get => _display && (Parent?.Display ?? true);
            set
            {
                if (value != _display)
                {
                    _display = value;
                    OnDisplayChanged?.Invoke(value);
                }
            }
        }
        public event Action<bool> OnDisplayChanged;
        public PositionType Positioning;
        public virtual bool HasFocus { get { return Parent?.HasFocus ?? true; } }
        private bool Initialized = false;
        private HashSet<string> Tags = new HashSet<string>();
        public bool HasTag(string tag) => Tags.Contains(tag);
        public void AddTag(string tag)
        {
            Tags.Add(tag);
            foreach (ElementField field in Fields.Values)
                field.UpdateFromStyle();
        }
        public bool RemoveTag(string tag)
        {
            var output = Tags.Remove(tag);
            foreach (ElementField field in Fields.Values)
                field.UpdateFromStyle();
            return output;
        }

        public SpriteFont Font { get { return Fields["font"].ObjValue as SpriteFont; } }
        public string HoverText { get { return this["hover-text"]; } }
        public Color FontColor { get { return (Color)(Fields["font-color"].ObjValue ?? Color.Black); } }
        public ImageAsset Texture
        {
            get { return Fields["image"].ObjValue as ImageAsset; }
            set { Fields["image"].SetValue(null, value); }
        }
        public Color TextureColor { get { return (Color)(Fields["image-color"].ObjValue ?? Color.White); } }
        public Color? FillColor
        {
            get => Fields["fill-color"].ObjValue as Color?;
            set => Fields["fill-color"].SetValue(null, value);
        }
        public ImageAsset Background { get { return Fields["background"].ObjValue as ImageAsset; } }
        public Color? BackgroundColor
        {
            get => Fields["background-color"].ObjValue as Color?;
            set => Fields["background-color"].SetValue(null, value);
        }
        private readonly Dictionary<KeyBind, InputHandler> inputHandlers = new Dictionary<KeyBind, InputHandler>();

        public Element()
        {
            Positioning = PositionType.Inline;
            Fields["font"] = new ElementField(
                this,
                "font",
                ElementField.ContentSetter<SpriteFont>
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
            Fields["fill-color"] = new ElementField(
                this,
                "fill-color",
                (value) => ElementField.ColorSetter(value),
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
            Fields["hover-text"] = new ElementField(
                this,
                "hover-text",
                (value) => (value)
                );
            Type tagType = GetType();
            while (tagType != typeof(Element))
            {
                Tags.Add(tagType.Name.ToLower());
                tagType = tagType.BaseType;
            }
            foreach (ElementField field in Fields.Values)
                field.UpdateFromStyle();
        }

        public virtual void Initialize()
        {
            foreach (var child in Children)
                child.Initialize();
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
        /// <summary>
        /// Registers a handler for the given keybind. The input will be consumed if the handler returns true.
        /// </summary>
        /// <param name="keybind">The keybind on which to fire the handler</param>
        /// <param name="handler">An input handler for they keybind returning whether to consume input or not</param>
        /// <param name="pressType">Selects which press type the handler should fire on</param>
        /// <param name="requireDisplay">Requires that the element be displayed for the handler to fire</param>
        /// <param name="ignoreConsumed">Ignores other elements marking the keybind as already consumed</param>
        public void RegisterHandler(KeyBind keybind, Func<InputState, bool> handler,
            KeyPressType pressType = KeyPressType.Press, bool requireDisplay = true, bool ignoreConsumed = false)
        {
            inputHandlers[keybind] = new InputHandler()
            {
                Handler = handler,
                PressType = pressType,
                RequireDisplay = requireDisplay,
                IgnoreConsumed = ignoreConsumed
            };
        }
        /// <summary>
        /// Registers a handler for the given keybind. The input will be always be consumed.
        /// </summary>
        /// <param name="keybind">The keybind on which to fire the handler</param>
        /// <param name="handler">An input handler for they keybind</param>
        /// <param name="pressType">Selects which press type the handler should fire on</param>
        /// <param name="requireDisplay">Requires that the element be displayed for the handler to fire</param>
        /// <param name="ignoreConsumed">Ignores other elements marking the keybind as already consumed</param>
        public void RegisterHandler(KeyBind keybind, Action<InputState> handler,
            KeyPressType pressType = KeyPressType.Press, bool requireDisplay = true, bool ignoreConsumed = false) =>
            RegisterHandler(keybind, (input) => { handler(input); return true; }, pressType, requireDisplay, ignoreConsumed);

        public virtual bool HandleInput(bool otherTookInput, InputState input)
        {
            foreach (Element child in Children)
            {
                otherTookInput |= child.HandleInput(otherTookInput, input);
            }
            foreach (var inputHandler in inputHandlers)
            {
                if (input.IsConsumed(inputHandler.Key, inputHandler.Value.PressType))
                    continue;
                switch (inputHandler.Value.PressType)
                {
                    case KeyPressType.Press:
                        if (!input.IsNewKeyPress(inputHandler.Key))
                            continue;
                        break;
                    case KeyPressType.Release:
                        if (!input.IsNewKeyRelease(inputHandler.Key))
                            continue;
                        break;
                    case KeyPressType.Hold:
                        if (!input.IsKeyDown(inputHandler.Key))
                            continue;
                        break;
                }
                if ((Display || !inputHandler.Value.RequireDisplay) && (!otherTookInput || inputHandler.Value.IgnoreConsumed))
                {
                    if (inputHandler.Value.Handler(input))
                    {
                        otherTookInput = true;
                        input.ConsumeInput(inputHandler.Key, inputHandler.Value.PressType);
                    }
                }
            }
            return otherTookInput;
        }

        public RectOffset Margin;

        public ElementSize Width;
        public int MeasuredWidth { get; set; }

        public ElementSize Height;
        public int MeasuredHeight { get; set; }

        public void Center()
        {
            Positioning = PositionType.Center;
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

        public bool MouseInside(InputState input)
        {
            return Rect.Contains(input.MousePosition.X, input.MousePosition.Y);
        }

        public virtual void Update(GameTime gameTime)
        {
            foreach (Element child in Children)
            {
                child.Parent = this;
                if (child.Display)
                    child.Update(gameTime);
            }
        }
        public virtual void OnMeasure(int width, int height)
        {
            MeasuredWidth = Width.Measure(width, Children.Select(c => c.MeasuredWidth).DefaultIfEmpty(0).Max());
            MeasuredHeight = Height.Measure(height, Children.Select(c => c.MeasuredHeight).DefaultIfEmpty(0).Max());
        }
        public virtual void Measure(int width, int height)
        {
            if (!Display)
            {
                MeasuredHeight = 0;
                MeasuredWidth = 0;
                return;
            }
            width = Width.CropParentSize(width);
            height = Height.CropParentSize(height);
            if (LayoutManager != null)
                LayoutManager.OnMeasure(this, width, height);
            else
            {
                foreach (var child in Children)
                {
                    child.Measure(width - Margin.WidthTotal(width), height - Margin.HeightTotal(height));
                }
                OnMeasure(width, height);
                foreach (var child in Children)
                {
                    child.Measure(MeasuredWidth - Margin.WidthTotal(width), MeasuredHeight - Margin.HeightTotal(height));
                }
            }
        }
        public virtual void Layout(Rectangle bounds)
        {
            if (!Display)
                return;
            Bounds = new Rectangle()
            {
                X = bounds.X + (Positioning == PositionType.Absolute ? 0 : (Parent?.Rect.X ?? 0)),
                Y = bounds.Y + (Positioning == PositionType.Absolute ? 0 : (Parent?.Rect.Y ?? 0)),
                Width = bounds.Width,
                Height = bounds.Height,
            };
            Rect = new Rectangle()
            {
                X = Bounds.X + Margin.Left.Measure(Parent?.MeasuredWidth ?? 0),
                Y = Bounds.Y + Margin.Top.Measure(Parent?.MeasuredHeight ?? 0),
                Width = Bounds.Width - Margin.Left.Measure(Parent?.MeasuredWidth ?? 0) - Margin.Right.Measure(Parent?.MeasuredWidth ?? 0),
                Height = Bounds.Height - Margin.Top.Measure(Parent?.MeasuredHeight ?? 0) - Margin.Bottom.Measure(Parent?.MeasuredHeight ?? 0)
            };
            if (LayoutManager != null)
                LayoutManager.OnLayout(this, Rect);
            else
            {
                int XOffset = 0;
                int YOffset = 0;
                int MaxRowHeight = 0;
                foreach (Element child in Children)
                {
                    switch (child.Positioning)
                    {
                        case PositionType.Inline:
                            if ((XOffset > 0 && XOffset + child.MeasuredWidth > MeasuredWidth))
                            {
                                XOffset = 0;
                                YOffset += MaxRowHeight;
                                MaxRowHeight = 0;
                            }
                            child.Layout(new Rectangle(XOffset, YOffset, child.MeasuredWidth, child.MeasuredHeight));
                            MaxRowHeight = Math.Max(MaxRowHeight, child.MeasuredHeight);
                            XOffset += child.MeasuredWidth;
                            break;
                        case PositionType.Center:
                            child.Layout(new Rectangle(Rect.Width / 2 - child.MeasuredWidth / 2, Rect.Height / 2 - child.MeasuredHeight / 2, child.MeasuredWidth, child.MeasuredHeight));
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

        public virtual void Draw(float gameTime, SpriteBatch spritebatch)
        {
            if (Background != null)
                Background.Draw(spritebatch, Bounds, BackgroundColor ?? Color.White, Z);
            else if (BackgroundColor != null)
                ImageAsset.Blank.Draw(spritebatch, Bounds, BackgroundColor.Value, Z);

            if (Texture != null)
                Texture.Draw(spritebatch, Rect, TextureColor, Layer(1));
            else if(FillColor != null)
                ImageAsset.Blank.Draw(spritebatch, Rect, FillColor.Value, Layer(2));
            // TODO
            //if (HasFocus && MouseInside() && HoverText != null)
            //{
            //    spritebatch.DrawString(Font, HoverText, new Vector2(Mouse.GetState().X + 15, Mouse.GetState().Y), Color.Black, 0);
            //}
        }

        public virtual float DrawWithChildren(float gameTime, SpriteBatch spritebatch, float layer)
        {
            Z = layer;
            Draw(gameTime, spritebatch);
            List<Element> drawChildren = Children;
            drawChildren.Reverse();
            foreach (Element child in drawChildren)
            {
                if (child.Display)
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
        public T AddElement<T>(T element, int? index = null) where T : Element
        {
            if (element.Initialized)
                throw new Exception("Element already initiliazed cannot be added to a new parent");
            element.Parent = this;
            _children.Insert(index ?? _children.Count, element);
            if (Initialized)
                element.Initialize();
            return element;
        }
        public void RemoveElement(Element element)
        {
            _children.Remove(element);
        }

        public virtual bool Toggle(bool? show = null)
        {
            Display = !((!show) ?? Display);
            return Display;
        }
    }
}
