using System;
using System.Collections.Generic;

namespace ACMESharp.Util
{
    public class KeyValuePairComparer<K, V> : IEqualityComparer<KeyValuePair<K, V>>
    {
        Func<K, K, bool> _keyComparer;
        Func<V, V, bool> _valueComparer;
        
        public KeyValuePairComparer(
                Func<K, K, bool> keyComparer = null,
                Func<V, V, bool> valueComparer = null)
        {
            _keyComparer = keyComparer ?? ((k1, k2) => object.Equals(k1, k2));
            _valueComparer = valueComparer ?? ((v1, v2) => object.Equals(v1, v2));
        }

        public bool Equals(KeyValuePair<K, V> x, KeyValuePair<K, V> y)
        {
            return _keyComparer(x.Key, y.Key) && _valueComparer(x.Value, y.Value);
        }

        public int GetHashCode(KeyValuePair<K, V> obj)
        {
            return obj.GetHashCode();
        }
    }
}