using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models.Base;
using DolarBot.Modules.InteractiveCommands;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Services.Banking;
using DolarBot.Services.Base;
using DolarBot.Services.Currencies;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.InteractiveCommands.Base
{
    /// <summary>
    /// Base class for fiat currency related modules.
    /// </summary>
    /// <typeparam name="TypeService">The type of service associated with this class.</typeparam>
    public abstract class BaseFiatCurrencyInteractiveModule<TypeService, TypeResponse> : BaseInteractiveModule where TypeService : BaseCurrencyService<TypeResponse> where TypeResponse : CurrencyResponse
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about currency rates and values.
        /// </summary>
        protected readonly TypeService Service;

        private readonly string PARTIAL_RESPONSE_MESSAGE = $"{Format.Bold("Atención")}: No se pudieron obtener algunas cotizaciones. Sólo se mostrarán aquellas que no presentan errores.";
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the object using the <see cref="IConfiguration"/> object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="logger">The log4net logger.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public BaseFiatCurrencyInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger)
        {
            Service = CreateService(configuration, api);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Creates a new service for this currency.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <returns>An instance of type <typeparamref name="TypeService"/>.</returns>
        protected abstract TypeService CreateService(IConfiguration configuration, ApiCalls api);

        /// <summary>
        /// Retrieves the currency from <see cref="Currencies"/> that represents the current module.
        /// </summary>
        /// <returns></returns>
        protected abstract Currencies GetCurrentCurrency();

        /// <summary>
        /// Replies with an embed message containing all available standard rates for this currency.
        /// </summary>
        protected async Task SendAllStandardRates()
        {
            TypeResponse[] responses = await Service.GetAllStandardRates();
            if (responses.Any(r => r != null))
            {
                TypeResponse[] successfulResponses = responses.Where(r => r != null).ToArray();
                EmbedBuilder embed = Service.CreateEmbed(successfulResponses);
                if (responses.Length != successfulResponses.Length)
                {
                    await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Content = PARTIAL_RESPONSE_MESSAGE);
                }
                await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embed = embed.Build());
            }
            else
            {
                await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Content = REQUEST_ERROR_MESSAGE);
            }
        }

        /// <summary>
        /// Replies with an embed message containing all available bank rates for this currency.
        /// </summary>
        /// <param name="description">The message to show as a description.</param>
        protected async Task SendAllBankRates(string description)
        {
            TypeResponse[] responses = await Service.GetAllBankRates();
            if (responses.Any(r => r != null))
            {
                string thumbnailUrl = Configuration.GetSection("images").GetSection("bank")["64"];
                TypeResponse[] successfulResponses = responses.Where(r => r != null).ToArray();
                EmbedBuilder embed = Service.CreateEmbed(successfulResponses, description, thumbnailUrl);
                if (responses.Length != successfulResponses.Length)
                {
                    await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Content = PARTIAL_RESPONSE_MESSAGE);
                }
                await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embed = embed.Build());
            }
            else
            {
                await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Content = REQUEST_ERROR_MESSAGE);
            }
        }

        /// <summary>
        /// Replies with an embed message containing a specific bank rate for this currency.
        /// </summary>
        /// <param name="bank">The bank which rate must be replied.</param>
        protected async Task SendBankRate(Banks bank, string description)
        {
            string thumbnailUrl = Service.GetBankThumbnailUrl(bank);
            TypeResponse result = await Service.GetByBank(bank);
            if (result != null)
            {
                EmbedBuilder embed = await Service.CreateEmbedAsync(result, description, null, thumbnailUrl);
                await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embed = embed.Build());
            }
            else
            {
                await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Content = REQUEST_ERROR_MESSAGE);
            }
        }

        /// <summary>
        /// Replies with an embed message containing a specific standard rate for this currency.
        /// </summary>
        /// <param name="response">The currency response containing the data.</param>
        /// <param name="description">The embed message description.</param>
        protected async Task SendStandardRate(TypeResponse response, string description)
        {
            if (response != null)
            {
                EmbedBuilder embed = await Service.CreateEmbedAsync(response, description);
                await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embed = embed.Build());
            }
            else
            {
                await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Content = REQUEST_ERROR_MESSAGE);
            }
        }

        #endregion
    }
}
