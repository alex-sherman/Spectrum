using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens
{
    struct TagOverride
    {
        public Selector Selector;
        public string Field;
        public string Value;

        public TagOverride(Selector selector, string field, string value)
        {
            Selector = selector;
            Field = field;
            Value = value;
        }
    }
    public class ElementField
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

        public static object ContentSetter<T>(string value) where T : class
        {
            return value == null ? null : ContentHelper.Load<T>(value);
        }

        public static object JSONSetter<T>(string value)
        {
            return JToken.Parse(value).Value<T>();
        }

        private static List<TagOverride> TagOverrides = new List<TagOverride>();
        public static void OverrideTag(string selector, string field, string value)
            => OverrideTag(Selector.Parse(selector), field, value);
        public static void OverrideTag(Selector selector, string field, string value)
            => TagOverrides.Add(new TagOverride(selector, field, value));
        public static void OverrideTag(string field, string value)
            => TagOverrides.Add(new TagOverride(null, field, value));
        private static string FindTagOverride(Element element, string fieldName)
        {
            for (int i = 0; i < TagOverrides.Count; i++)
            {
                if ((TagOverrides[i].Selector?.Matches(element) ?? true) && TagOverrides[i].Field == fieldName)
                {
                    return TagOverrides[i].Value;
                }
            }
            return null;
        }

        private Func<string, object> setter;
        private string _styleStrValue;
        private object _styleValue;
        private string _strValue;
        private object _value;
        private bool hasElementValue => _value != null;
        private string fieldName;
        private bool inherited;
        private Element element;
        public object ObjValue => hasElementValue ? _value : _styleValue;
        public string StrValue
        {
            get => hasElementValue ? _strValue : _styleStrValue;
            set => SetValue(value, setter(value));
        }
        public void SetValue(string strValue, object objValue)
        {
            _strValue = strValue;
            _value = objValue;
        }
        public ElementField(Element element, string fieldName, Func<string, object> setter, bool inherited = true)
        {
            this.element = element;
            this.fieldName = fieldName;
            this.setter = setter;
            this.inherited = inherited;
        }
        public void UpdateFromStyle()
        {
            string overrideValue = FindTagOverride(element, fieldName);
            if (overrideValue != null)
            {
                _styleStrValue = overrideValue;
                _styleValue = setter(overrideValue);
            }
            else if (element.Parent != null && inherited && element.Parent.Fields.ContainsKey(fieldName))
            {
                _styleStrValue = element.Parent.Fields[fieldName].StrValue;
                _styleValue = element.Parent.Fields[fieldName].ObjValue;
            }
            else
            {
                _styleStrValue = null; _styleValue = null;
            }
        }
    }
}
