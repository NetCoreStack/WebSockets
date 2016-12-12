using Microsoft.Extensions.Caching.Memory;
using System;

namespace Common.Libs
{
    public class InMemoryCacheProvider
    {
        private readonly IMemoryCache _memoryCache;

        public InMemoryCacheProvider(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public object SetObject(string key, object value, MemoryCacheEntryOptions options)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException("Cache value is null");
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var entryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                Priority = options.Priority
            };

            _memoryCache.Set(key, value, entryOptions);
            return value;
        }

        public object GetObject(string key)
        {
            return _memoryCache.Get(key);
        }

        public T GetObject<T>(string key)
        {
            return (T)_memoryCache.Get(key);
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }
    }
}
