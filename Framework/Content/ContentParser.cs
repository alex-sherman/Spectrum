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
        IEnumerable<string> FindAll(string directory, string glob, bool recursive);
        void Clear();
    }
    public abstract class CachedContentParser<T, U> : IContentParser where T : class
    {
        protected Dictionary<string, T> cachedData = new Dictionary<string, T>();
        protected abstract T LoadData(string path, string name);
        protected abstract U SafeCopy(T data);
        public IEnumerable<string> Extensions = Enumerable.Empty<string>();
        public string Prefix { get; set; }
        public CachedContentParser() { }
        public CachedContentParser(params string[] extensions)
        {
            Extensions = extensions.ToList();
        }
        public U Load(string path, string name)
        {
            if (!cachedData.ContainsKey(path)) { Cache(path, name); }
            T data = cachedData[path];
            return data == null ? default(U) : SafeCopy(data);
        }
        string TryExtensions(string path)
        {
            if (File.Exists(path)) return path;
            if (Extensions != null)
                foreach (var extension in Extensions)
                {
                    var newPath = $"{path}.{extension}";
                    if (File.Exists(newPath)) return newPath;
                }
            throw new FileNotFoundException("The file could not be loaded: ", path);
        }
        public virtual void Cache(string path, string name)
        {
            try
            {
                using (DebugTiming.Content.Time(GetType().Name))
                    cachedData[path] = LoadData(TryExtensions(path), name);
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

        public IEnumerable<string> FindAll(string directory, string glob, bool recursive)
        {
            try
            {
                var results = Directory.EnumerateFiles(directory, glob, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(path => Extensions.Contains(Path.GetExtension(path).Substring(1)));
                return results
                    .Select(path => Path.Combine(Path.GetDirectoryName(path).Replace(directory, "").TrimStart('\\'), 
                                    Path.GetFileNameWithoutExtension(path)));
            }
            catch(DirectoryNotFoundException)
            {
                return Enumerable.Empty<string>();
            }
        }
    }
    public abstract class CachedContentParser<T> : CachedContentParser<T, T> where T : class
    {
        protected override T SafeCopy(T data) => data;
        public CachedContentParser() { }
        public CachedContentParser(params string[] extensions) : base(extensions) { }
    }
}
