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

        public static List<Tuple<string, string, string>> TagOverrides = new List<Tuple<string, string, string>>();
        public static void OverrideTag(string tag, string name, string value)
        {
            TagOverrides.Add(new Tuple<string, string, string>(tag, name, value));
        }
        private static string FindTagOverride(Element element, string fieldName)
        {
            string output = null;
            int matchIndex = -1;
            for (int i = matchIndex + 1; i < TagOverrides.Count; i++)
            {
                if ((TagOverrides[i].Item1 == null || element.HasTag(TagOverrides[i].Item1)) && TagOverrides[i].Item2 == fieldName)
                {
                    output = TagOverrides[i].Item3;
                    matchIndex = i;
                }
            }
            return output;
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
