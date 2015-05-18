using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework
{
    /// <summary>
    /// Because the csharp one is kinda dumb about exceptions. I don't care if the key exists and is null or doesn't exist, it should return null.
    /// </summary>
    /// <typeparam name="TKey">Key</typeparam>
    /// <typeparam name="TValue">Value</typeparam>
    public class RealDict<TKey, TValue>
    {
        Dictionary<TKey, TValue> internalDict;
        public RealDict()
        {
            internalDict = new Dictionary<TKey, TValue>();
        }
        /// <summary>
        /// Returns null when the key doesn't exist
        /// </summary>
        /// <param name="key">Location in dictionary</param>
        /// <returns>Value at key location</returns>
        public TValue this[TKey key] {
            get
            {
                if (internalDict.ContainsKey(key)) { return internalDict[key]; }
                return default(TValue);
            }
            set
            {
                internalDict[key] = value;
            }
        }
        public Dictionary<TKey, TValue>.ValueCollection Values { get { return internalDict.Values; } }
        public Dictionary<TKey, TValue>.KeyCollection Keys { get { return internalDict.Keys; } }
        public bool Remove(TKey key)
        {
            if (internalDict.Keys.Contains(key))
            {
                internalDict.Remove(key);
                return true;
            }
            return false;
        }
        public RealDict<TKey, TValue> Copy()
        {
            RealDict<TKey, TValue> output = new RealDict<TKey, TValue>();
            foreach (TKey key in internalDict.Keys)
            {
                output.internalDict[key] = internalDict[key];
            }
            return output;
        }
    }
}
