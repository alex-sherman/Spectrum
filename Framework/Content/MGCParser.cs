using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content
{
    public class MGCParser<T> : IContentParser
    {
        public string Prefix { get; set; }
        string Suffix;
        public MGCParser(string prefix, string suffix)
        {
            Prefix = prefix;
            Suffix = suffix;
        }
        public object Load(string path, string name, bool skipCache)
        {
            return SpectrumGame.Game.Content.Load<T>(path + Suffix);
        }

        public void Clear() { }

        public IEnumerable<string> FindAll(string directory, string glob = "*", bool recursive = true)
        {
            return Enumerable.Empty<string>();
        }
    }
}
