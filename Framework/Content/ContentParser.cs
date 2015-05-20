using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    public interface ICachedContentParser
    {
        string Prefix { get; }
        string Suffix { get; }
        void Cache(string path);
        object Load(string path);
    }
    public abstract class CachedContentParser<T, U> : ICachedContentParser
    {
        protected Dictionary<string, T> cachedData = new Dictionary<string, T>();
        protected abstract T LoadData(string path);
        protected abstract U SafeCopy(T data);
        public virtual string Prefix { get { return ""; } }
        public virtual string Suffix { get { return ""; } }
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
