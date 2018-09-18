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
        #region Style
        private ElementStyle inheritedStyle;
        private ElementStyle style;
        public ImageAsset Texture
        {
            get => style.Texture ?? inheritedStyle.Texture;
            set => style.Texture = value;
        }
        public Color TextureColor { get => style.TextureColor ?? inheritedStyle.TextureColor ?? Color.White; }
        public ImageAsset Background
        {
            get => style.Background ?? inheritedStyle.Background;
            set => style.Background = value;
        }
        public Color FontColor { get => style.FontColor ?? inheritedStyle.FontColor ?? Color.Black; }
        public Color? FillColor
        {
            get => style.FillColor ?? inheritedStyle.FillColor;
            set => style.FillColor = value;
        }
        public Color? BackgroundColor
        {
            get => style.BackgroundColor ?? inheritedStyle.BackgroundColor;
            set => style.BackgroundColor = value;
        }
        public SpriteFont Font
        {
            get => ElementStyle.Value(style.Font, inheritedStyle.Font);
            set => style.Font = value;
        }
        public RectOffset Margin
        {
            get => ElementStyle.Value(style.Margin, inheritedStyle.Margin);
            set => style.Margin = value;
        }
        public RectOffset Padding { get => ElementStyle.Value(style.Padding, inheritedStyle.Padding); set => style.Padding = value; }
        public ElementSize Width
        {
            get => style.Width ?? inheritedStyle.Width ?? new ElementSize(wrapContent: true);
            set => style.Width = value;
        }
        public ElementSize Height
        {
            get => style.Height ?? inheritedStyle.Height ?? new ElementSize(wrapContent: true);
            set => style.Height = value;
        }
        #endregion
        public LayoutManager LayoutManager;
        public Element Parent { get; private set; }
        private List<Element> _children = new List<Element>();
        //TODO: Maybe cache this and update during Update(), can't be modified during a frame though
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
                    if (Parent?.Display ?? true)
                        checkDisplayChanged(_display, true);
                }
            }
        }
        private void checkDisplayChanged(bool newDisplay, bool force = false)
        {
            if (_display || force)
            {
                OnDisplayChanged?.Invoke(newDisplay);
                foreach (var child in Children)
                    child.checkDisplayChanged(newDisplay);
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
            if (!Tags.Contains(tag))
            {
                Tags.Add(tag);
                UpdateStyle();
            }
        }
        public void AddTagsFromType(Type type)
        {
            if (type != typeof(object) && type != typeof(Element))
            {
                string name = type.Name.ToLower();
                int index;
                if ((index = name.IndexOf('`')) > 0)
                    name = name.Substring(0, index);
                AddTag(name);
                AddTagsFromType(type.BaseType);
            }
        }
        public bool RemoveTag(string tag)
        {
            if (Tags.Remove(tag))
            {
                UpdateStyle();
                return true;
            }
            return false;
        }

        private readonly Dictionary<KeyBind, InputHandler> inputHandlers = new Dictionary<KeyBind, InputHandler>();

        public Element()
        {
            Positioning = PositionType.Inline;
            AddTagsFromType(GetType());
        }

        public void UpdateStyle()
        {
            inheritedStyle = ElementStyle.GetStyle(this);
        }
        public virtual void Initialize()
        {
            UpdateStyle();
            foreach (var child in Children)
                child.Initialize();
            Initialized = true;
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
                if (input.IsConsumed(inputHandler.Key))
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
                        input.ConsumeInput(inputHandler.Key, inputHandler.Value.PressType == KeyPressType.Hold);
                    }
                }
            }
            return otherTookInput;
        }

        public int MeasuredWidth { get; set; }
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

        public virtual void Update(float gameTime)
        {
            foreach (Element child in Children.Where(c => c.Display))
                child.Update(gameTime);
        }

        public int MeasureWidth(int parentWidth, int contentWidth) =>
            Width.Measure(parentWidth, contentWidth + Padding.WidthTotal(parentWidth)) + Margin.WidthTotal(parentWidth);
        public int MeasureHeight(int parentHeight, int contentHeight) =>
            Height.Measure(parentHeight, contentHeight + Padding.HeightTotal(parentHeight)) + Margin.HeightTotal(parentHeight);
        private int MaxChildWidth => Children.Select(c => c.MeasuredWidth).DefaultIfEmpty(0).Max();
        private int MaxChildHeight => Children.Select(c => c.MeasuredHeight).DefaultIfEmpty(0).Max();

        // Calculate the dims to pass to a child before considering margin
        public int PreChildWidth(int parentWidth, int contentWidth = 0) =>
            Math.Max(0, Width.Measure(parentWidth, contentWidth + Padding.WidthTotal(parentWidth)) - Padding.WidthTotal(parentWidth));
        public int PreChildHeight(int parentHeight, int contentHeight = 0) =>
            Math.Max(0, Height.Measure(parentHeight, contentHeight + Padding.HeightTotal(parentHeight)) - Padding.HeightTotal(parentHeight));

        public void ClearMeasure()
        {
            MeasuredWidth = 0;
            MeasuredHeight = 0;
            foreach (var child in Children)
            {
                child.ClearMeasure();
            }
        }
        public virtual void OnMeasure(int width, int height)
        {
            MeasuredWidth = MeasureWidth(width, MaxChildWidth);
            MeasuredHeight = MeasureHeight(height, MaxChildHeight);
        }
        public virtual void Measure(int parentWidth, int parentHeight)
        {
            if (!Display)
            {
                MeasuredHeight = 0;
                MeasuredWidth = 0;
                return;
            }
            if (LayoutManager != null)
                LayoutManager.OnMeasure(this, parentWidth, parentHeight);
            else
            {
                foreach (var child in Children)
                {
                    child.Measure(PreChildWidth(parentWidth, Children.Select(c => c.MeasuredWidth).DefaultIfEmpty(0).Max()),
                        PreChildHeight(parentHeight, Children.Select(c => c.MeasuredHeight).DefaultIfEmpty(0).Max()));
                }
                OnMeasure(parentWidth, parentHeight);
            }
        }
        public virtual void Layout(Rectangle bounds)
        {
            if (!Display)
                return;
            // Accounts for Margin
            Bounds = new Rectangle()
            {
                X = bounds.X + Margin.Left.Measure(bounds.Width) + (Positioning == PositionType.Absolute ? 0 : (Parent?.Rect.X ?? 0)),
                Y = bounds.Y + Margin.Top.Measure(bounds.Height) + (Positioning == PositionType.Absolute ? 0 : (Parent?.Rect.Y ?? 0)),
                Width = Math.Max(0, bounds.Width - Margin.WidthTotal(Parent?.Rect.Width ?? 0)),
                Height = Math.Max(0, bounds.Height - Margin.HeightTotal(Parent?.Rect.Height ?? 0)),
            };
            // Accounts for Padding
            Rect = new Rectangle()
            {
                X = Bounds.X + Padding.Left.Measure(bounds.Width),
                Y = Bounds.Y + Padding.Top.Measure(bounds.Height),
                Width = Math.Max(0, Bounds.Width - Padding.WidthTotal(Parent?.Rect.Width ?? 0)),
                Height = Math.Max(0, Bounds.Height - Padding.HeightTotal(Parent?.Rect.Height ?? 0)),
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
            else if (FillColor != null)
                ImageAsset.Blank.Draw(spritebatch, Rect, FillColor.Value, Layer(1));
        }

        public virtual float DrawWithChildren(float gameTime, SpriteBatch spritebatch, float layer)
        {
            Z = layer;
            Draw(gameTime, spritebatch);
            List<Element> drawChildren = Children.ToList();
            drawChildren.Reverse();
            foreach (Element child in drawChildren)
            {
                if (child.Display)
                {
                    layer = child.DrawWithChildren(gameTime, spritebatch, layer * (1 - ZDiff * ZLayers));
                }
            }
            return layer * (1 - ZDiff * ZLayers);
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
