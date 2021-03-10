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
        public static SpriteFont DefaultFont = ContentHelper.Load<SpriteFont>("default");
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
        public RootElement Root { get; private set; }
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
        public bool IsInline => Positioning == PositionType.Center || Positioning == PositionType.Inline;
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
            if (!(input.IsConsumed(new KeyBind(-1)) || input.IsConsumed(new KeyBind(-2))) && MouseInside(input) && input.MouseScrollY != 0 && AllowScrollY)
            {
                input.ConsumeInput(new KeyBind(-1), false);
                input.ConsumeInput(new KeyBind(-2), false);
                ScrollY = Math.Max(0, Math.Min(ContentHeight - Rect.Height, ScrollY - input.MouseScrollY / 10));
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
                        input.ConsumeInput(inputHandler.Key, false);
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
        public int ScrollX;
        public int ScrollY;
        public bool AllowScrollX;
        public bool AllowScrollY;
        public bool ZDetach;
        public int Z;
        const float DZ = 1e-6f;
        public float LayerDepth = 0;
        public float Layer(int z, float? layer = null) => Math.Max((layer ?? LayerDepth) - z * DZ, 0);
        protected float MaxChildLayerDepth => Children.Select(c => c.MaxChildLayerDepth).DefaultIfEmpty(LayerDepth).Min();
        /// <summary>
        /// A bounding rectangle with a relative offset that accounts for padding from the parent but is not necessarily relative to the parent
        /// </summary>
        public Rectangle Bounds { get; protected set; }
        /// <summary>
        /// The absolute rectangle in terms of screen coordinates for the element
        /// </summary>
        public Rectangle Rect { get; protected set; }
        /// <summary>
        /// The element's Rect clipped by the parent's Clipped
        /// </summary>
        public Rectangle Clipped { get; protected set; }

        public bool MouseInside(InputState input)
        {
            return Rect.Clip(Clipped).Contains(input.MousePosition);
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

        // Calculate the dims to pass to a child before considering margin
        public int PreChildWidth(int parentWidth, int contentWidth = 0) =>
            Math.Max(0, Width.Measure(parentWidth, contentWidth + Padding.WidthTotal(parentWidth)) - Padding.WidthTotal(parentWidth));
        public int PreChildHeight(int parentHeight, int contentHeight = 0) =>
            Math.Max(0, Height.Measure(parentHeight, contentHeight + Padding.HeightTotal(parentHeight)) - Padding.HeightTotal(parentHeight));

        public virtual int ContentWidth =>
            LayoutManager == null ? Children.Where(c => c.IsInline).Select(c => c.MeasuredWidth).DefaultIfEmpty(0).Sum() : LayoutManager.ContentWidth(this);

        public virtual int ContentHeight =>
            LayoutManager == null ? Children.Where(c => c.IsInline).Select(c => c.MeasuredHeight).DefaultIfEmpty(0).Max() : LayoutManager.ContentHeight(this);

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
            MeasuredWidth = MeasureWidth(width, ContentWidth);
            MeasuredHeight = MeasureHeight(height, ContentHeight);
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
                    child.Measure(PreChildWidth(parentWidth, ContentWidth),
                        PreChildHeight(parentHeight, ContentHeight));
                }
                OnMeasure(parentWidth, parentHeight);
            }
        }
        public virtual void Layout(Rectangle bounds)
        {
            LayerDepth = 0;
            if (!Display)
                return;
            // Accounts for Margin
            Bounds = new Rectangle()
            {
                X = bounds.X + Margin.Left.Measure(bounds.Width) + (Positioning == PositionType.Absolute ? 0 : (Parent?.Rect.X ?? 0)),
                Y = bounds.Y + Margin.Top.Measure(bounds.Height) + (Positioning == PositionType.Absolute ? 0 : (Parent?.Rect.Y ?? 0)),
                Width = Math.Max(0, Width.Measure(bounds.Width, ContentWidth) + Padding.WidthTotal(bounds.Width)),
                Height = Math.Max(0, Height.Measure(bounds.Height, ContentHeight) + Padding.HeightTotal(bounds.Height)),
            };
            // Accounts for Padding
            Rect = new Rectangle()
            {
                X = Bounds.X + Padding.Left.Measure(bounds.Width),
                Y = Bounds.Y + Padding.Top.Measure(bounds.Height),
                Width = Math.Max(0, Width.Measure(bounds.Width, ContentWidth)),
                Height = Math.Max(0, Height.Measure(bounds.Height, ContentHeight)),
            };
            if (ZDetach || Parent == null)
                Clipped = Bounds;
            else
                Clipped = Bounds.Clip(Parent.Clipped);
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
                            child.Layout(new Rectangle(XOffset, YOffset, MeasuredWidth, MeasuredHeight));
                            MaxRowHeight = Math.Max(MaxRowHeight, child.MeasuredHeight);
                            XOffset += child.MeasuredWidth;
                            break;
                        case PositionType.Center:
                            child.Layout(new Rectangle(Rect.Width / 2 - child.MeasuredWidth / 2, Rect.Height / 2 - child.MeasuredHeight / 2, MeasuredWidth, MeasuredHeight));
                            break;
                        case PositionType.Absolute:
                        case PositionType.Relative:
                            child.Layout(new Rectangle(child.X, child.Y, MeasuredWidth, MeasuredHeight));
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
                spritebatch.Draw(Background, Bounds, BackgroundColor ?? Color.White, Layer(-2), clip: Clipped);
            else if (BackgroundColor != null)
                spritebatch.Draw(ImageAsset.Blank, Bounds, BackgroundColor.Value, Layer(-2), clip: Clipped);

            if (Texture != null)
                spritebatch.Draw(Texture, Rect, TextureColor, Layer(-1), clip: Parent?.Rect);
            else if (FillColor != null)
                spritebatch.Draw(ImageAsset.Blank, Rect, FillColor.Value, Layer(-1), clip: Clipped);
            if (AllowScrollY && ContentHeight > Rect.Height)
            {
                var sbX = Rect.Top + (float)ScrollY * Rect.Height / ContentHeight;
                var sbH = (float)Rect.Height * Rect.Height / ContentHeight;
                spritebatch.Draw(ImageAsset.Blank, new Rectangle(Rect.Right - 3, (int)sbX, 3, (int)sbH), Color.Black, Layer(10, MaxChildLayerDepth), clip: Clipped);
            }
        }
        public void MoveElement(Element child, int newIndex)
        {
            var oldIndex = _children.IndexOf(child);
            _children.RemoveAt(oldIndex);
            _children.Insert(oldIndex < newIndex ? newIndex - 1 : newIndex, child);
        }
        public T AddElement<T>(T element, int? index = null) where T : Element
        {
            if (element.Initialized)
                throw new Exception("Element already initiliazed cannot be added to a new parent");
            element.Parent = this;
            element.Root = this as RootElement ?? Root;
            _children.Insert(index ?? _children.Count, element);
            if (Initialized)
                element.Initialize();
            return element;
        }
        public void RemoveElement(Element element)
        {
            if (_children.Remove(element))
            {
                element.Parent = null;
                element.Root = null;
            }
        }

        public virtual bool Toggle(bool? show = null)
        {
            Display = !((!show) ?? Display);
            return Display;
        }
    }
}
