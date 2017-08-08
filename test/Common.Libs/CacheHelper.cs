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
                    [nameof(HizmetEnvanteri)] = new CacheItemDescriptor { Type = typeof(HizmetEnvanteri), Weight = CacheItemWeights.MiddleWeight },
                    [nameof(KurumBirimDto)] = new CacheItemDescriptor { Type = typeof(KurumBirimDto), Weight = CacheItemWeights.HeavyWeight },
                    [nameof(KurumBirimPasifDto)] = new CacheItemDescriptor { Type = typeof(KurumBirimPasifDto), Weight = CacheItemWeights.MiddleWeight },
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

            throw new ArgumentOutOfRangeException($"Key could not be found: \"{key}\"");
        }
    }
}
