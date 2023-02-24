using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AODBImporter
{
    internal static class Extensions
    {
        internal static bool TryGetKey<K, V>(this IDictionary<K, V> instance, V value, out K key)
        {
            foreach (var entry in instance)
            {
                if (!entry.Value.Equals(value))
                {
                    continue;
                }
                key = entry.Key;
                return true;
            }
            key = default(K);
            return false;
        }
    }
}
