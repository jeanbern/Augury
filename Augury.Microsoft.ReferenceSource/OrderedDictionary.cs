//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Collections.Specialized;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    // System.Collections.Specialized.OrderedDictionary is NOT generic.
    // This class is essentially a generic wrapper for OrderedDictionary.
    public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
        where TKey : class
    {
        readonly OrderedDictionary _privateDictionary;
        private Lazy<IReadOnlyList<TKey>> _cache;

        public OrderedDictionary()
        {
            _privateDictionary = new OrderedDictionary();
            ResetCache();
        }

        public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> dictionary)
        {
            if (dictionary != null)
            {
                _privateDictionary = new OrderedDictionary();

                foreach (var pair in dictionary)
                {
                    _privateDictionary.Add(pair.Key, pair.Value);
                }
            }

            ResetCache();
        }

        private void ResetCache()
        {
            _cache = new Lazy<IReadOnlyList<TKey>>(KeyList);
        }

        public int Count
        {
            get
            {
                return _privateDictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public TKey this[int index]
        {
            get { return _cache.Value[index]; }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                if (_privateDictionary.Contains(key))
                {
                    return (TValue)_privateDictionary[key];
                }

                throw new KeyNotFoundException();
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                _privateDictionary[key] = value;
                ResetCache();
            }
        }

        public List<TKey> KeyList()
        {
            var keys = new List<TKey>(_privateDictionary.Count);

            foreach (TKey key in _privateDictionary.Keys)
            {
                keys.Add(key);
            }

            // Keys should be put in a ReadOnlyCollection,
            // but since this is an internal class, for performance reasons,
            // we choose to avoid creating yet another collection.

            return keys;
        }

        public ICollection<TKey> Keys
        {
            get { return KeyList(); }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var values = new List<TValue>(_privateDictionary.Count);

                foreach (TValue value in _privateDictionary.Values)
                {
                    values.Add(value);
                }

                // Values should be put in a ReadOnlyCollection,
                // but since this is an internal class, for performance reasons,
                // we choose to avoid creating yet another collection.

                return values;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            _privateDictionary.Add(key, value);
            ResetCache();
        }

        public void Clear()
        {
            _privateDictionary.Clear();
            ResetCache();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null || !_privateDictionary.Contains(item.Key))
            {
                return false;
            }

            return _privateDictionary[item.Key].Equals(item.Value);
        }

        public bool ContainsKey(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            return _privateDictionary.Contains(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }

            if (array.Rank > 1 || arrayIndex >= array.Length || array.Length - arrayIndex < _privateDictionary.Count)
            {
                throw new ArgumentException("array argument out of range?", "array");
            }

            var index = arrayIndex;
            foreach (DictionaryEntry entry in _privateDictionary)
            {
                array[index] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
                index++;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (DictionaryEntry entry in _privateDictionary)
            {
                yield return new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var result = Contains(item);
            if (result)
            {
                _privateDictionary.Remove(item.Key);
                ResetCache();
            }

            return result;
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            var result = _privateDictionary.Contains(key);
            if (result)
            {
                _privateDictionary.Remove(key);
                ResetCache();
            }

            return result;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            var keyExists = _privateDictionary.Contains(key);
            value = keyExists ? (TValue)_privateDictionary[key] : default(TValue);

            return keyExists;
        }

        void IDictionary.Add(object key, object value)
        {
            _privateDictionary.Add(key, value);
            ResetCache();
        }

        void IDictionary.Clear()
        {
            _privateDictionary.Clear();
            ResetCache();
        }

        bool IDictionary.Contains(object key)
        {
            return _privateDictionary.Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return _privateDictionary.GetEnumerator();
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return ((IDictionary)_privateDictionary).IsFixedSize;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return _privateDictionary.IsReadOnly;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return _privateDictionary.Keys;
            }
        }

        void IDictionary.Remove(object key)
        {
            _privateDictionary.Remove(key);
            ResetCache();
        }

        ICollection IDictionary.Values
        {
            get
            {
                return _privateDictionary.Values;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return _privateDictionary[key];
            }
            set
            {
                _privateDictionary[key] = value;
                ResetCache();
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            _privateDictionary.CopyTo(array, index);
        }

        int ICollection.Count
        {
            get
            {
                return _privateDictionary.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection)_privateDictionary).IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)_privateDictionary).SyncRoot;
            }
        }

    }
}