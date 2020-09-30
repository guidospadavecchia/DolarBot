using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;

namespace DolarBot.API.Cache
{
    /// <summary>
    /// Wrapper for in-memory caching of REST results.
    /// </summary>
    public class ResponseCache
    {
        #region Vars
        /// <summary>
        /// Allows access to application settings.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// In-memory cache.
        /// </summary>
        private readonly MemoryCache cache;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a <see cref="ResponseCache"/> with default options.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> object to access application settings.</param>
        public ResponseCache(IConfiguration configuration)
        {
            this.configuration = configuration;
            cache = new MemoryCache(new MemoryCacheOptions());
        }
        #endregion

        #region Methods
        /// <summary>
        /// Retrieves an object from cache by its key.
        /// </summary>
        /// <typeparam name="T">The object to retrieve.</typeparam>
        /// <param name="key">The key of the object to retrieve.</param>
        /// <returns>An object of type <typeparamref name="T"/> if found, otherwise null.</returns>
        public T GetObject<T>(object key) where T : class
        {
            return cache.Get<T>(key);
        }

        /// <summary>
        /// Saves an object to cache.
        /// </summary>
        /// <param name="key">Object's key.</param>
        /// <param name="data">Object to save.</param>
        public void SaveObject(object key, object data)
        {
            int cacheExpiration = int.Parse(configuration["requestCacheExpirationMinutes"]);
            cache.Set(key, data, TimeSpan.FromMinutes(cacheExpiration));
        }
        #endregion
    }
}
