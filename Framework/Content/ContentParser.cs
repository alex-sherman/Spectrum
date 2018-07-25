using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    public interface IContentParser
    {
        string Prefix { get; set; }
        object Load(string path, string name);
        void Clear();
    }
    public abstract class CachedContentParser<T, U> : IContentParser where T : class
    {
        protected Dictionary<string, T> cachedData = new Dictionary<string, T>();
        protected abstract T LoadData(string path, string name);
        protected abstract U SafeCopy(T data);
        public string Prefix { get; set; }
        public U Load(string path, string name)
        {
            if (!cachedData.ContainsKey(path)) { Cache(path, name); }
            T data = cachedData[path];
            return data == null ? default(U) : SafeCopy(data);
        }
        public string TryExtensions(string path, params string[] extensions)
        {
            if (File.Exists(path)) return path;
            foreach (var extension in extensions)
            {
                if (File.Exists(path + extension)) return path + extension;
            }
            return null;
        }
        public string TryThrowExtensions(string path, params string[] extensions)
        {
            string full_path = TryExtensions(path, extensions);
            if(full_path == null)
                throw new FileNotFoundException("The file could not be loaded: ", path);
            return full_path;
        }
        public virtual void Cache(string path, string name)
        {
            try
            {
                using (DebugTiming.Content.Time(GetType().Name))
                    cachedData[path] = LoadData(path, name);
            }
            catch (FileNotFoundException)
            {
                cachedData[path] = null;
            }
        }

        object IContentParser.Load(string path, string name)
        {
            return Load(path, name);
        }
        public void Clear()
        {
            cachedData = new Dictionary<string, T>();
        }
    }
    public abstract class CachedContentParser<T> : CachedContentParser<T, T> where T : class
    {
        protected override T SafeCopy(T data) => data;
    }
}
