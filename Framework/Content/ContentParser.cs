using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    public interface ContentParser
    {
        object Load(string path);
    }
    public abstract class CachedContentParser<T, U> : ContentParser
    {
        protected Dictionary<string, T> cachedData = new Dictionary<string, T>();
        protected abstract T LoadData(string path);
        protected abstract U SafeCopy(T data);
        public U Load(string path)
        {
            if (!cachedData.ContainsKey(path)) { cachedData[path] = LoadData(path); }
            return SafeCopy(cachedData[path]);
        }

        object ContentParser.Load(string path)
        {
            return Load(path);
        }
    }
}
