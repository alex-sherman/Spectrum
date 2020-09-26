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
        object Load(string path, string name, bool refreshCache);
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
        public U Load(string path, string name, bool refreshCache)
        {
            if (refreshCache || !cachedData.ContainsKey(path)) { Cache(path, name); }
            T data = cachedData[path];
            return data == null ? default(U) : SafeCopy(data);
        }
        protected virtual string ResolvePath(string path, string name)
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
                    cachedData[path] = LoadData(ResolvePath(path, name), name);
            }
            catch (FileNotFoundException)
            {
                cachedData[path] = null;
            }
        }

        object IContentParser.Load(string path, string name, bool refreshCache)
        {
            return Load(path, name, refreshCache);
        }
        public void Clear()
        {
            cachedData.Clear();
        }

        public IEnumerable<string> FindAll(string directory, string glob, bool recursive)
        {
            try
            {
                var results = Directory.EnumerateFiles(directory, glob, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(path => Extensions.Contains(Path.GetExtension(path).Substring(1)));
                return results
                    .Select(path => Path.Combine(Path.GetDirectoryName(path).Replace(directory, "").TrimStart('\\').TrimStart('/'), 
                                    Path.GetFileNameWithoutExtension(path)).Replace('\\', '/'));
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
