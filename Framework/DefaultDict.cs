using System;
using System.Collections;
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
    public class DefaultDict<TKey, TValue> : IDictionary<TKey, TValue>
    {
        Dictionary<TKey, TValue> internalDict = new Dictionary<TKey, TValue>();
        Func<TValue> Constructor = () => default(TValue);
        bool _addToDictionary;
        public DefaultDict(Func<TValue> constructor, bool addToDictionary = false)
        {
            Constructor = constructor;
            _addToDictionary = addToDictionary;
        }
        public DefaultDict() { }
        /// <summary>
        /// Returns null when the key doesn't exist
        /// </summary>
        /// <param name="key">Location in dictionary</param>
        /// <returns>Value at key location</returns>
        public TValue this[TKey key]
        {
            get
            {
                if (!internalDict.ContainsKey(key))
                {
                    if (!_addToDictionary)
                        return Constructor();
                    else
                        internalDict[key] = Constructor();
                }
                return internalDict[key];
            }
            set
            {
                internalDict[key] = value;
            }
        }
        public Dictionary<TKey, TValue>.ValueCollection Values { get { return internalDict.Values; } }
        public Dictionary<TKey, TValue>.KeyCollection Keys { get { return internalDict.Keys; } }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return ((IDictionary<TKey, TValue>)internalDict).Keys;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return ((IDictionary<TKey, TValue>)internalDict).Values;
            }
        }

        public int Count
        {
            get
            {
                return ((IDictionary<TKey, TValue>)internalDict).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IDictionary<TKey, TValue>)internalDict).IsReadOnly;
            }
        }

        public bool Remove(TKey key)
        {
            if (internalDict.Keys.Contains(key))
            {
                internalDict.Remove(key);
                return true;
            }
            return false;
        }
        public DefaultDict<TKey, TValue> Copy()
        {
            DefaultDict<TKey, TValue> output = new DefaultDict<TKey, TValue>();
            foreach (TKey key in internalDict.Keys)
            {
                output.internalDict[key] = internalDict[key];
            }
            return output;
        }

        public bool ContainsKey(TKey key)
        {
            return ((IDictionary<TKey, TValue>)internalDict).ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            ((IDictionary<TKey, TValue>)internalDict).Add(key, value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return ((IDictionary<TKey, TValue>)internalDict).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((IDictionary<TKey, TValue>)internalDict).Add(item);
        }

        public void Clear()
        {
            ((IDictionary<TKey, TValue>)internalDict).Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)internalDict).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)internalDict).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)internalDict).Remove(item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IDictionary<TKey, TValue>)internalDict).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<TKey, TValue>)internalDict).GetEnumerator();
        }
    }
}
