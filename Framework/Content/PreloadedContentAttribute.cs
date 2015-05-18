using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    public class PreloadedContentAttribute : System.Attribute
    {
        public Type Type { get; private set; }
        public string Path { get; private set; }
        public string Plugin { get; private set; }
        public PreloadedContentAttribute(Type type, string path, string plugin)
        {
            Type = type;
            Path = path;
            Plugin = plugin;
        }
        public PreloadedContentAttribute(string path, string plugin = null)
        {
            Path = path;
            Plugin = plugin;
        }
    }
}
