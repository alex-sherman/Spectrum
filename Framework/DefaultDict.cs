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
        Func<TKey, TValue> Constructor = (_) => default(TValue);
        bool _addToDictionary;
        public DefaultDict() { }
        public DefaultDict(bool addToDictionary)
            : this()
        {
            _addToDictionary = addToDictionary;
        }
        public DefaultDict(Func<TValue> constructor, bool addToDictionary = false)
            : this(addToDictionary)
        {
            Constructor = (_) => constructor();
        }
        public DefaultDict(Func<TKey, TValue> constructor, bool addToDictionary = false)
            : this(addToDictionary)
        {
            Constructor = constructor;
        }
        /// <summary>
        /// Returns null when the key doesn't exist
        /// </summary>
        /// <param name="key">Location in dictionary</param>
        /// <returns>Value at key location</returns>
        public TValue this[TKey key]
        {
            get
            {
                if (!internalDict.TryGetValue(key, out TValue output))
                {
                    if (!_addToDictionary)
                        return Constructor(key);
                    else
                        return internalDict[key] = Constructor(key);
                }
                return output;
            }
            set
            {
                internalDict[key] = value;
            }
        }
        public Dictionary<TKey, TValue>.ValueCollection Values { get { return internalDict.Values; } }
        public Dictionary<TKey, TValue>.KeyCollection Keys { get { return internalDict.Keys; } }
        public void SetFrom(IDictionary<TKey, TValue> dict)
        {
            internalDict = new Dictionary<TKey, TValue>(dict);
        }
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
            DefaultDict<TKey, TValue> output = new DefaultDict<TKey, TValue>() { internalDict = new Dictionary<TKey, TValue>(internalDict) };
            output.Constructor = Constructor;
            output._addToDictionary = _addToDictionary;
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
