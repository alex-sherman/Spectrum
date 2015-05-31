using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    public interface ICachedContentParser
    {
        string Prefix { get; set; }
        string Suffix { get; set; }
        void Cache(string path);
        object Load(string path);
    }
    public abstract class CachedContentParser<T, U> : ICachedContentParser
    {
        protected Dictionary<string, T> cachedData = new Dictionary<string, T>();
        protected abstract T LoadData(string path);
        protected abstract U SafeCopy(T data);
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public U Load(string path)
        {
            if (!cachedData.ContainsKey(path)) { Cache(path); }
            return SafeCopy(cachedData[path]);
        }

        public virtual void Cache(string path)
        {
            cachedData[path] = LoadData(path);
        }

        object ICachedContentParser.Load(string path)
        {
            return Load(path);
        }
    }
}
