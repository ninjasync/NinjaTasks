using System.Collections.Generic;

namespace NinjaTools.Collections
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            TValue ret;
            if (!dict.TryGetValue(key, out ret))
                dict[key] = ret = new TValue();
            return ret;
        }

        public static bool AddWeight<TKey>(this IDictionary<TKey,int> dict, TKey key, int add = 1)
        {
            bool isNew = !dict.ContainsKey(key);
            if (!isNew) dict[key] += add;
            else dict[key] = add;
            return isNew;
        }
        public static bool AddWeight<TKey>(this IDictionary<TKey, double> dict, TKey key, double add = 1)
        {
            bool isNew = !dict.ContainsKey(key);
            if (!isNew) dict[key] += add;
            else dict[key] = add;
            return isNew;
        }
        public static bool AddWeight<TKey>(this IDictionary<TKey, float> dict, TKey key, float add = 1)
        {
            bool isNew = !dict.ContainsKey(key);
            if (!isNew) dict[key] += add;
            else dict[key] = add;
            return isNew;
        }

        public static void AddLookup<TKey, TValue>(this IDictionary<TKey, List<TValue>> dict, TKey key, TValue value)
                       // where TListType : List<TValue>, new()
        {
            List<TValue> list;
            if (!dict.TryGetValue(key, out list))
                list = dict[key] = new List<TValue>();
            list.Add(value);
        }
    }
}
