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
            cache.Set(key, data, TimeSpan.FromMinutes(GetDefaultExpiration()));
        }

        /// <summary>
        /// Saves an object to cache.
        /// </summary>
        /// <param name="key">Object's key.</param>
        /// <param name="data">Object to save.</param>
        /// <param name="expirationSeconds">TTL in seconds.</param>
        public void SaveObject(object key, object data, int expirationSeconds)
        {
            cache.Set(key, data, TimeSpan.FromMinutes(expirationSeconds > 0 ? expirationSeconds : int.MaxValue));
        }

        /// <summary>
        /// Retrieves the default TTL (time-to-live), in seconds, before an item expires from cache.
        /// </summary>
        /// <returns>The expiration TTL in seconds.</returns>
        public int GetDefaultExpiration()
        {
            return int.TryParse(configuration.GetSection("cacheExpirationSeconds")["default"], out int result) ? result : 0;
        }

        /// <summary>
        /// Retrieves the TTL (time-to-live) for cryptocurrency requests, in seconds, before an item expires from cache.
        /// </summary>
        /// <returns>The expiration TTL in seconds.</returns>
        public int GetCryptoExpiration()
        {
            return int.TryParse(configuration.GetSection("cacheExpirationSeconds")["crypto"], out int result) ? result : 0;
        }

        /// <summary>
        /// Retrieves the TTL (time-to-live) for the list of cryptocurrency codes, in seconds, before an item expires from cache.
        /// </summary>
        /// <returns>The expiration TTL in seconds.</returns>
        public int GetCryptoListExpiration()
        {
            return int.TryParse(configuration.GetSection("cacheExpirationSeconds")["cryptoList"], out int result) ? result : 0;
        }

        /// <summary>
        /// Retrieves the TTL (time-to-live) for the list of currency codes, in seconds, before an item expires from cache.
        /// </summary>
        /// <returns>The expiration TTL in seconds.</returns>
        public int GetCurrencyListExpiration()
        {
            return int.TryParse(configuration.GetSection("cacheExpirationSeconds")["currencyList"], out int result) ? result : 0;
        }
        #endregion
    }
}
