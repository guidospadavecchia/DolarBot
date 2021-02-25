using DolarBot.API;
using DolarBot.Services.Banking;
using Microsoft.Extensions.Configuration;

namespace DolarBot.Services.Base
{
    /// <summary>
    /// Base class for currency related services
    /// </summary>
    public abstract class BaseCurrencyService : BaseService
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="BaseCurrencyService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public BaseCurrencyService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a collection of valid banks for the currency.
        /// </summary>
        /// <returns>Collection of valid banks.</returns>
        public abstract Banks[] GetValidBanks();

        #endregion
    }
}
