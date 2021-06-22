using DolarBot.API;
using DolarBot.Services.Crypto;
using log4net;
using Microsoft.Extensions.Configuration;

namespace DolarBot.Modules.Commands.Base
{
    /// <summary>
    /// Base class for crypto-related modules.
    /// </summary>
    public class BaseCryptoModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about cryptocurrencies rates and values.
        /// </summary>
        protected readonly CryptoService CryptoService;

        /// <summary>
        /// The log4net logger.
        /// </summary>
        protected readonly ILog Logger;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the object using the <see cref="IConfiguration"/> object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        public BaseCryptoModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration)
        {
            Logger = logger;
            CryptoService = new CryptoService(configuration, api);
        }
        #endregion
    }
}
