using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;

namespace DolarBot.API.Cache
{
    public class ResponseCache
    {
        private readonly IConfiguration configuration;
        private readonly MemoryCache cache;

        public ResponseCache(IConfiguration configuration)
        {
            this.configuration = configuration;
            cache = new MemoryCache(new MemoryCacheOptions());
        }

        public T GetObject<T>(object key) where T : class
        {            
            return cache.Get<T>(key);
        }

        public void SaveObject(object key, object data)
        {
            int cacheExpiration = int.Parse(configuration["requestCacheExpirationMinutes"]);
            cache.Set(key, data, TimeSpan.FromMinutes(cacheExpiration));
        }
    }
}
