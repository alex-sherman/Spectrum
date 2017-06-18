using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PreloadedContentAttribute : System.Attribute
    {
        public string Path { get; private set; }
        public Type Type { get; private set; }
        public PreloadedContentAttribute(string path, Type type = null)
        {
            Path = path;
            Type = type;
        }
    }
}
