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
        object Load(string path);
    }
    public abstract class CachedContentParser<T, U> : ICachedContentParser where T : class
    {
        protected Dictionary<string, T> cachedData = new Dictionary<string, T>();
        protected abstract T LoadData(string path, string name);
        protected abstract U SafeCopy(T data);
        public string Prefix { get; set; }
        public U Load(string path)
        {
            if (!cachedData.ContainsKey(path)) { Cache(path); }
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
        public virtual void Cache(string path)
        {
            try
            {
                var timer = DebugTiming.Content.Time(GetType().Name);
                cachedData[path] = LoadData(path, path);
                timer.Stop();
            }
            catch (FileNotFoundException)
            {
                DebugPrinter.print(string.Format("File not found {0}", path));
                cachedData[path] = null;
            }
        }

        object ICachedContentParser.Load(string path)
        {
            return Load(path);
        }
    }
}
