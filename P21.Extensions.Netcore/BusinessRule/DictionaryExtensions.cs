namespace P21.Extensions.BusinessRule;

internal static class DictionaryExtensions
{
    public static TValue GetValue<TKey, TValue>(
      this Dictionary<TKey, TValue> dict,
      TKey key,
      TValue defaultValue)
    {
        return !dict.TryGetValue(key, out TValue obj) ? defaultValue : obj;
    }
}
