using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Crypto;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands.Base
{
    /// <summary>
    /// Base class for crypto-related modules.
    /// </summary>
    public class BaseCryptoModule : BaseModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about cryptocurrencies rates and values.
        /// </summary>
        protected readonly CryptoService CryptoService;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the object using the <see cref="IConfiguration"/> object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        public BaseCryptoModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger)
        {
            CryptoService = new CryptoService(configuration, api);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Replies with an embed message containing the rate for the current cryptocurrency.
        /// </summary>
        /// <param name="response">The crypto response with the data.</param>
        /// <param name="cryptoCurrencyName">A custom cryptocurrency name.</param>
        protected async Task SendCryptoReply(CryptoResponse response, string cryptoCurrencyName = null)
        {
            if (response != null)
            {
                EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(response, cryptoCurrencyName);
                embed.AddCommandDeprecationNotice(Configuration);
                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                await ReplyAsync(REQUEST_ERROR_MESSAGE);
            }
        }

        #endregion
    }
}
