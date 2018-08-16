using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Utility
{
    public class ArgParseResult
    {
        public List<string> Positional = new List<string>();
        public HashSet<string> Flags = new HashSet<string>();
        public Dictionary<string, string> Arguments = new Dictionary<string, string>();
        public T Get<T>(string key = null, int? position = null, T missing = default(T))
        {
            if (key != null && Arguments.TryGetValue(key, out string value))
                return (T)Convert.ChangeType(value, typeof(T));
            if (position != null && Positional.Count > position.Value)
                return (T)Convert.ChangeType(Positional[position.Value], typeof(T));
            return missing;
        }
        public bool HasFlag(string key) => Flags.Contains(key);
    }
    public class ArgParseHelper
    {
        public static ArgParseResult Parse(string[] args)
        {
            var result = new ArgParseResult();
            for(int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.Substring(0, 2) == "--")
                {
                    if (i + 1 >= args.Length || args[i + 1].Substring(0, 1) == "-")
                        result.Flags.Add(arg.Substring(2));
                    else
                    {
                        result.Arguments.Add(arg.Substring(2), args[++i]);
                    }
                }
                else if (arg.Substring(0, 1) == "-")
                    result.Flags.Add(arg.Substring(1));
                else
                    result.Positional.Add(arg);
            }
            return result;
        }
    }
}
