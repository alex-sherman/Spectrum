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
        public static object ColorSetter(string value)
        {
            try
            {
                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(value);
                return Color.FromNonPremultiplied(color.R, color.G, color.B, color.A);
            }
            catch(Exception)
            {
                return Color.Black;
            }
        }

        public static object ContentSetter<T>(string value) where T : class
        {
            try
            {
                return value == null ? null : ContentHelper.Load<T>(value);
            }
            catch (ContentLoadException)
            {
                return null;
            }
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
            foreach (string tag in element.Tags)
            {
                for (int i = matchIndex + 1; i < TagOverrides.Count; i++)
                {
                    if ((TagOverrides[i].Item1 == null || TagOverrides[i].Item1 == tag) && TagOverrides[i].Item2 == fieldName)
                    {
                        output = TagOverrides[i].Item3;
                        matchIndex = i;
                    }
                }
            }
            return output;
        }

        private Func<string, object> setter;
        private string _strValue;
        private object _value;
        private object defaultValue;
        public object ObjValue
        {
            get
            {
                return _value;
            }
        }
        private string fieldName;
        private bool inherited;
        private Element element;
        private bool initialized = false;
        public ElementField(Element element, string fieldName, Func<string, object> setter, bool inherited = true, object defaultValue = null)
        {
            this.element = element;
            this.fieldName = fieldName;
            this.setter = setter;
            this.inherited = inherited;
            this.defaultValue = defaultValue;
        }
        public void Initialize()
        {
            if (!initialized)
            {
                _value = defaultValue;
                string overrideValue = ElementField.FindTagOverride(element, fieldName);
                if (overrideValue != null)
                {
                    _strValue = overrideValue;
                    _value = setter(overrideValue);
                }
                else if (element.Parent != null && inherited && element.Parent.Fields.ContainsKey(fieldName))
                {
                    _strValue = element.Parent.Fields[fieldName].StrValue;
                    _value = element.Parent.Fields[fieldName].ObjValue;
                }
            }
            initialized = true;
        }
        public void SetValue(string strValue, object objValue)
        {
            initialized = true;
            _strValue = strValue;
            _value = objValue;
        }
        public string StrValue
        {
            get
            {
                return _strValue;
            }
            set
            {
                SetValue(value, setter(value));
            }
        }
    }
}
