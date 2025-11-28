using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace eP.Collections;

public class BidirectionalDict<T1, T2> : IReadOnlyCollection<BidirectionalDict<T1,T2>.KeysPair>
    where T1 : notnull where T2 : notnull
{
    private readonly Dictionary<T1, KeysPair> _dict1 = [];
    private readonly Dictionary<T2, KeysPair> _dict2 = [];

    public bool TryAdd(T1 value1, T2 value2)
    {
        lock (_dict1)
        {
            if (_dict1.ContainsKey(value1) || _dict2.ContainsKey(value2))
                return false;
            KeysPair keysPair = new KeysPair(value1, value2);
            _dict1.Add(value1, keysPair);
            _dict2.Add(value2, keysPair);
            
            return true;
        }
    }

    public bool ContainsKey1(T1 key)
    {
        lock (_dict1)
        {
            return _dict1.ContainsKey(key);
        }
    }
    
    public bool ContainsKey2(T2 key)
    {
        lock (_dict1)
        {
            return _dict2.ContainsKey(key);
        }
    }

    public bool TryGetValueByKey1(T1 key, [MaybeNullWhen(false)] out KeysPair value)
    {
        lock (_dict1)
        {
            return _dict1.TryGetValue(key, out value);
        }
    }
    public bool TryGetValueByKey2(T2 key, [MaybeNullWhen(false)] out KeysPair value)
    {
        lock (_dict1)
        {
            return _dict2.TryGetValue(key, out value);
        }
    }

    public bool RemoveByKey1(T1 value)
    {
        lock (_dict1)
        {
            if (_dict1.Remove(value, out var outValue))
            {
                _dict2.Remove(outValue.Key2);
                return true;
            }

            return false;
        }
    }
    
    public bool RemoveByKey2(T2 value)
    {
        lock (_dict1)
        {
            if (_dict2.Remove(value, out var outValue))
            {
                _dict1.Remove(outValue.Key1);
                return true;
            }

            return false;
        }
    }

    public class KeysPair
    {
        public readonly T1 Key1;
        public readonly T2 Key2;
        
        public KeysPair(T1 key1, T2 key2)
        {
            Key1 = key1;
            Key2 = key2;
        }
    }

    public IEnumerator<KeysPair> GetEnumerator()
    {
        return _dict1.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _dict1.Values.GetEnumerator();
    }

    public bool Contains(KeysPair item)
    {
        lock (_dict1)
        {
            return _dict1.TryGetValue(item.Key1, out var pair) && pair == item;
        }
    }

    public void CopyTo(KeysPair[] array, int arrayIndex)
    {
        lock (_dict1)
        {
            _dict1.Values.CopyTo(array, arrayIndex);
        }
    }

    public int Count => _dict1.Count;
}