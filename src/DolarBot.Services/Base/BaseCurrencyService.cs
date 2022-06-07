using Discord;
using DolarBot.API;
using DolarBot.API.Models.Base;
using DolarBot.Services.Banking;
using DolarBot.Services.Banking.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace DolarBot.Services.Base
{
    /// <summary>
    /// Base class for currency related services
    /// </summary>
    public abstract class BaseCurrencyService<T> : BaseService, IBankCurrencyService where T : CurrencyResponse
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

        /// <inheritdoc />
        public abstract Banks[] GetValidBanks();

        /// <summary>
        /// Fetches all available standard rates for this currency.
        /// </summary>
        /// <returns>An array of <see cref="T"/> objects.</returns>
        public abstract Task<T[]> GetAllStandardRates();

        /// <summary>
        /// Fetches all oficial rates from <see cref="Banks"/> for this currency.
        /// </summary>
        /// <typeparam name="T">The currency response type.</typeparam>
        /// <returns>An array of <typeparamref name="T"/> objects.</returns>
        public abstract Task<T[]> GetAllBankRates();

        /// <summary>
        /// Fetches the official rate from a specific bank for this currency.
        /// </summary>
        /// <param name="bank">The bank who's rate is to be retrieved.</param>
        /// <returns>A single <typeparamref name="T"/> response.</returns>
        public abstract Task<T> GetByBank(Banks bank);

        #region Embeds

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple currency responses.
        /// </summary>
        /// <param name="responses">The currency responses to show.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public abstract EmbedBuilder CreateEmbed(T[] responses, decimal amount = 1);

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple currency responses specifying a custom description and thumbnail URL.
        /// </summary>
        /// <param name="responses">The currency responses to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="thumbnailUrl">The URL of the embed's thumbnail image.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public abstract EmbedBuilder CreateEmbed(T[] responses, string description, string thumbnailUrl, decimal amount = 1);

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a single currency response specifying a custom description, title and thumbnail URL.
        /// </summary>
        /// <param name="response">The currency response to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="amount">The amount to rate against.</param>
        /// <param name="title">Optional. The embed's title.</param>
        /// <param name="thumbnailUrl">Optional. The embed's thumbnail URL.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public abstract Task<EmbedBuilder> CreateEmbedAsync(T response, string description, decimal amount = 1, string title = null, string thumbnailUrl = null);

        #endregion

        #endregion
    }
}
