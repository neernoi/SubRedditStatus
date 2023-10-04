using Microsoft.Extensions.Caching.Memory;

namespace SubRedditStatus.Helpers
{
    public class CacheHelper
    {
        private readonly IMemoryCache _memoryCache;
        public CacheHelper(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void SetMemory(string key, dynamic value, int absTimeInSeconds = 3600)
        {
            var cacheExpiryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(absTimeInSeconds),
                Priority = CacheItemPriority.High,
                Size = 1024,
            };

            _memoryCache.Set(key, value as object, cacheExpiryOptions);
        }

        public dynamic GetMemory(string key)
        {
            object value;
            _memoryCache.TryGetValue(key, out value);
            return value as dynamic;
        }

        public void RemoveMemory(string key)
        {
            _memoryCache.Remove(key);
        }
    }
}
