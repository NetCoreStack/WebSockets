using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Libs
{
    public static class CacheHelper
    {
        public static IDictionary<string, CacheItemDescriptor> CacheKeys
        {
            get
            {
                var dict = new Dictionary<string, CacheItemDescriptor>()
                {
                    [nameof(CacheItem2)] = new CacheItemDescriptor { Type = typeof(CacheItem2), Weight = CacheItemWeights.MiddleWeight },
                    [nameof(CacheItem1)] = new CacheItemDescriptor { Type = typeof(CacheItem1), Weight = CacheItemWeights.HeavyWeight },
                    [nameof(CacheItem3)] = new CacheItemDescriptor { Type = typeof(CacheItem3), Weight = CacheItemWeights.MiddleWeight },
                };

                var keys = dict.OrderBy(x => x.Key).ToDictionary(t => t.Key, t => t.Value);
                return keys;
            }
        }

        public static CacheItemDescriptor GetDescriptor(string key)
        {
            if (CacheKeys.TryGetValue(key, out CacheItemDescriptor descriptor))
            {
                return descriptor;
            }

            return null;
        }
    }
}
