using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AODB.Common.Extensions
{
    internal static class Misc
    {
        internal static bool TryGetKey(this Dictionary<int, string> dict, string value, out int key)
        {
            foreach(var kvp in dict) 
            {
                if(kvp.Value == value)
                {
                    key = kvp.Key;
                    return true;
                }
            }

            key = -1;
            return false;
        }
    }
}
