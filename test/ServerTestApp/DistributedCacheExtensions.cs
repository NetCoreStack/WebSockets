using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using NetCoreStack.WebSockets;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Common.Libs.CacheHelper;

namespace ServerTestApp
{
    public static class DistributedCacheExtensions
    {
        private static byte[] GetCompressedBinaryObject(this IDistributedCache cache, string key)
        {
            var bytes = cache.Get(key);

            if (bytes == null)
                throw new ArgumentOutOfRangeException($"Cache not found: {key}");

            return bytes;
        }

        public static async Task SendCache(this IDistributedCache cache, IConnectionManager connectionManager, string connectionId)
        {
            var keys = CacheKeys.Keys.Select(x => x).OrderBy(x => x);
            foreach (var key in keys)
            {
                try
                {
                    var values = cache.GetCompressedBinaryObject(key);
                    await connectionManager.SendBinaryAsync(connectionId, values, new RouteValueDictionary(new { Key = key }));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
