﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens
{
    public delegate object ElementFieldSetter(Element element, string value);
    public class ElementField
    {
        public static object ColorSetter(Element element, string value)
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

        public static object ContentSetter<T>(Element element, string value) where T : class
        {
            try
            {
                return ContentHelper.Load<T>(value);
            }
            catch (ContentLoadException)
            {
                return null;
            }
        }

        public static List<Tuple<string, string, string>> TagOverrides = new List<Tuple<string, string, string>>();
        private static string FindTagOverride(Element element, string fieldName)
        {
            string output = null;
            int matchIndex = -1;
            foreach (string tag in element.Tags)
            {
                for (int i = matchIndex + 1; i < TagOverrides.Count; i++)
                {
                    if (TagOverrides[i].Item1 == tag && TagOverrides[i].Item2 == fieldName)
                    {
                        output = TagOverrides[i].Item3;
                        matchIndex = i;
                    }
                }
            }
            return output;
        }

        private ElementFieldSetter setter;
        private string _strValue;
        private object _value;
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
        public ElementField(Element element, string fieldName, ElementFieldSetter setter, bool inherited = true)
        {
            this.element = element;
            this.fieldName = fieldName;
            this.setter = setter;
            this.inherited = inherited;
        }
        public void Initialize()
        {
            if (!initialized)
            {
                string overrideValue = ElementField.FindTagOverride(element, fieldName);
                if (overrideValue != null)
                {
                    _strValue = overrideValue;
                    _value = setter(element, overrideValue);
                }
                else if (element.Parent != null && inherited && element.Parent.Fields.ContainsKey(fieldName))
                {
                    _strValue = element.Parent.Fields[fieldName].StrValue;
                    _value = element.Parent.Fields[fieldName].ObjValue;
                }
            }
            initialized = true;
        }
        public string StrValue
        {
            get
            {
                return _strValue;
            }
            set
            {
                initialized = true;
                _strValue = value;
                _value = setter(element, value);
            }
        }
    }
}
