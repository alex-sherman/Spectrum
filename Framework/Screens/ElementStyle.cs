using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Screens
{
    public struct ElementColor
    {
        public static Color ColorSetter(string value)
        {
            try
            {
                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(value);
                return Color.FromNonPremultiplied(color.R, color.G, color.B, color.A);
            }
            catch (Exception)
            {
                return Color.Black;
            }
        }
        Color Color;
        public static implicit operator Color(ElementColor color) => color.Color;
        public static implicit operator ElementColor(Color color) => new ElementColor() { Color = color };
        public static implicit operator ElementColor(string color) => new ElementColor() { Color = ColorSetter(color) };
    }
    public struct ElementStyle
    {
        struct TagOverride
        {
            public Selector Selector;
            public ElementStyle Style;

            public TagOverride(Selector selector, ElementStyle style)
            {
                Selector = selector;
                Style = style;
            }
        }
        static TypeData typeData;
        static ElementStyle()
        {
            typeData = new TypeData(typeof(ElementStyle));
        }
        private static List<TagOverride> TagOverrides = new List<TagOverride>();
        public static void OverrideTag(ElementStyle style)
            => TagOverrides.Add(new TagOverride(null, style));
        public static void OverrideTag(string selector, ElementStyle style)
            => OverrideTag(Selector.Parse(selector), style);
        public static void OverrideTag(Selector selector, ElementStyle style)
            => TagOverrides.Add(new TagOverride(selector, style));
        public static ElementStyle GetStyle(Element element)
        {
            ElementStyle output = new ElementStyle();
            foreach (var overriden in TagOverrides)
            {
                if(overriden.Selector?.Matches(element) ?? true)
                    output.Apply(overriden.Style);
            }
            return output;
        }
        public ImageAsset Texture;
        public Color? TextureColor;
        public ImageAsset Background;
        public ElementColor? FontColor;
        public ElementColor? BackgroundColor;
        public ElementColor? FillColor;
        public SpriteFont Font;
        public ElementSize? Width;
        public ElementSize? Height;
        public RectOffset? Padding;
        public RectOffset? Margin;
        public static T Value<T>(T? value, T? parent = null) where T : struct
        {
            return value ?? parent ?? default(T);
        }
        public static T Value<T>(T value, T parent) where T : class
        {
            return value ?? parent ?? default(T);
        }
        public void Apply(ElementStyle newStyle)
        {
            foreach (var field in typeData.members.Where(f => !f.Value.IsStatic))
            {
                object value = field.Value.GetValue(newStyle);
                if (value != null)
                    this = (ElementStyle)field.Value.SetValue(this, value);
            }
        }
    }
}
