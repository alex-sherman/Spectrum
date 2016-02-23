using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    public interface ICachedContentParser
    {
        string Prefix { get; set; }
        string Suffix { get; set; }
        object Load(string path);
    }
    public abstract class CachedContentParser<T, U> : ICachedContentParser where T : class
    {
        protected Dictionary<string, T> cachedData = new Dictionary<string, T>();
        protected abstract T LoadData(string path);
        protected abstract U SafeCopy(T data);
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public U Load(string path)
        {
            if (!cachedData.ContainsKey(path)) { Cache(path); }
            T data = cachedData[path];
            return data == null ? default(U) : SafeCopy(data);
        }

        public virtual void Cache(string path)
        {
            try
            {
                cachedData[path] = LoadData(path);
            }
            catch(FileNotFoundException)
            {
                cachedData[path] = null;
            }
        }

        object ICachedContentParser.Load(string path)
        {
            return Load(path);
        }
    }
}
