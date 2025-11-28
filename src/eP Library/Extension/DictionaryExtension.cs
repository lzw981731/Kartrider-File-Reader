namespace eP.Extension;

public static class DictionaryExtension
{
    /// <summary>
    /// Attempts to get a value from a dictionary.
    /// If this dictionary doesn't contain the given key, then it will add a new key-value pair automatically.
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue: new()
    {
        return dictionary.GetOrCreate(key, new TValue());
    }
    
    /// <summary>
    /// Attempts to get a value from a dictionary.
    /// If this dictionary doesn't contain the given key, then it will add a new key-value pair automatically.
    /// </summary>
    public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
        dictionary.TryAdd(key, defaultValue);
        return dictionary[key];
    }

    public static Dictionary<TKey, TValue> AddContinue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        dictionary.Add(key, value);
        return dictionary;
    }
}