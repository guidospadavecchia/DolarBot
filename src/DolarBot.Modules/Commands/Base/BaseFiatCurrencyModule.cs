using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models.Base;
using DolarBot.Services.Banking;
using DolarBot.Services.Base;
using DolarBot.Services.Currencies;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands.Base
{
    /// <summary>
    /// Base class for fiat currency related modules.
    /// </summary>
    /// <typeparam name="TypeService">The type of service associated with this class.</typeparam>
    public abstract class BaseFiatCurrencyModule<TypeService, TypeResponse> : BaseInteractiveModule where TypeService : BaseCurrencyService<TypeResponse> where TypeResponse : CurrencyResponse
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about currency rates and values.
        /// </summary>
        protected readonly TypeService Service;

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
        /// <param name="logger">The log4net logger.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public BaseFiatCurrencyModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration)
        {
            Logger = logger;
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
        /// Sends a reply indicating an error has occurred.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        protected async Task SendErrorReply(Exception ex)
        {
            await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
            Logger.Error("Error al ejecutar comando.", ex);
        }

        /// <summary>
        /// Replies with an embed message containing all available standard rates for this currency.
        /// </summary>
        protected async Task SendAllStandardRates()
        {
            TypeResponse[] responses = await Service.GetAllStandardRates().ConfigureAwait(false);
            if (responses.Any(r => r != null))
            {
                TypeResponse[] successfulResponses = responses.Where(r => r != null).ToArray();
                EmbedBuilder embed = Service.CreateEmbed(successfulResponses);
                if (responses.Length != successfulResponses.Length)
                {
                    await ReplyAsync($"{Format.Bold("Atención")}: No se pudieron obtener algunas cotizaciones. Sólo se mostrarán aquellas que no presentan errores.").ConfigureAwait(false);
                }
                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Replies with an embed message containing all available bank rates for this currency.
        /// </summary>
        /// <param name="description">The message to show as a description.</param>
        protected async Task SendAllBankRates(string description)
        {
            TypeResponse[] responses = await Service.GetAllBankRates().ConfigureAwait(false);
            if (responses.Any(r => r != null))
            {
                string thumbnailUrl = Configuration.GetSection("images").GetSection("bank")["64"];
                TypeResponse[] successfulResponses = responses.Where(r => r != null).ToArray();
                EmbedBuilder embed = Service.CreateEmbed(successfulResponses, description, thumbnailUrl);
                if (responses.Length != successfulResponses.Length)
                {
                    await ReplyAsync($"{Format.Bold("Atención")}: No se pudieron obtener algunas cotizaciones. Sólo se mostrarán aquellas que no presentan errores.").ConfigureAwait(false);
                }
                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Replies with an embed message containing a specific bank rate for this currency.
        /// </summary>
        /// <param name="bank">The bank which rate must be replied.</param>
        protected async Task SendBankRate(Banks bank, string description)
        {
            string thumbnailUrl = Service.GetBankThumbnailUrl(bank);
            TypeResponse result = await Service.GetByBank(bank).ConfigureAwait(false);
            if (result != null)
            {
                EmbedBuilder embed = await Service.CreateEmbedAsync(result, description, null, thumbnailUrl);
                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
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
                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sends a reply indicating that the specified bank is not a valid bank for this currency.
        /// </summary>
        /// <param name="bank">The invalid bank.</param>
        protected async Task SendInvalidBankForCurrency(Banks bank)
        {
            Currencies currency = GetCurrentCurrency();
            string commandPrefix = Configuration["commandPrefix"];
            string bankCommand = typeof(MiscModule).GetMethod("GetBanks").GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
            await ReplyAsync($"La cotización del {Format.Bold(bank.GetDescription())} no está disponible para esta moneda. Verifique los bancos disponibles con {Format.Code($"{commandPrefix}{bankCommand} {currency.GetDescription().ToLower()}")}.").ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a reply indicating that the user input could not be converted to a valid bank parameter.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        protected async Task SendUnknownBankParameter(string userInput)
        {
            Currencies currency = GetCurrentCurrency();
            string commandPrefix = Configuration["commandPrefix"];
            string bankCommand = typeof(MiscModule).GetMethod("GetBanks").GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
            await ReplyAsync($"Banco '{Format.Bold(userInput)}' inexistente. Verifique los bancos disponibles con {Format.Code($"{commandPrefix}{bankCommand} {currency.GetDescription().ToLower()}")}.").ConfigureAwait(false);
        }

        #endregion
    }
}
