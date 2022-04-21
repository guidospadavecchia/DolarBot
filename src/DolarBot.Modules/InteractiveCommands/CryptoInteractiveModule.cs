using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Attributes;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Autocompletion.Crypto;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Services.Crypto;
using DolarBot.Util.Extensions;
using Fergun.Interactive;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DolarBot.API.ApiCalls.DolarBotApi;

namespace DolarBot.Modules.InteractiveCommands
{
    /// <summary>
    /// Contains cryptocurrency related commands.
    /// </summary>
    [HelpOrder(6)]
    [HelpTitle("Crypto")]
    public class CryptoInteractiveModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about cryptocurrencies rates and values.
        /// </summary>
        protected readonly CryptoService CryptoService;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        /// <param name="interactiveService">The interactive service.</param>
        public CryptoInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api, InteractiveService interactiveService) : base(configuration, logger, interactiveService)
        {
            CryptoService = new CryptoService(configuration, api);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Replies with a paginated embed result showing the complete list of cryptocurrencies.
        /// </summary>
        /// <param name="cryptoCurrenciesList">The list of cryptocurrency codes.</param>
        private async Task SendDeferredCryptoCurrencyListAsync(List<CryptoCodeResponse> cryptoCurrenciesList)
        {
            string currencyCommand = GetType().GetMethod(nameof(GetCryptoCurrenciesAsync)).GetCustomAttributes(true).OfType<SlashCommandAttribute>().First().Name;
            EmbedBuilder[] embeds = CryptoService.CreateCryptoListEmbedAsync(cryptoCurrenciesList, currencyCommand).ToArray();
            await SendDeferredPaginatedEmbedAsync(embeds);
        }

        /// <summary>
        /// Replies with an embed message for a single cryptocurrency value.
        /// </summary>
        /// <param name="cryptoCurrenciesList">The collection of valid currency codes.</param>
        /// <param name="code">The code to be searched.</param>
        private async Task SendCryptoResponseAsync(List<CryptoCodeResponse> cryptoCurrenciesList, string code = null)
        {
            if (code == null)
            {
                await SendDeferredCryptoCurrencyListAsync(cryptoCurrenciesList);
            }
            else
            {
                CryptoCodeResponse cryptoCurrencyCode = cryptoCurrenciesList.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (cryptoCurrencyCode != null)
                {
                    List<CryptoCurrencies> cryptocurrencies = Enum.GetValues(typeof(CryptoCurrencies)).Cast<CryptoCurrencies>().ToList();
                    bool isEnumerated = cryptocurrencies.Any(x => x.GetAttribute<CryptoCodeAttribute>()?.Code.Equals(cryptoCurrencyCode.Code, StringComparison.OrdinalIgnoreCase) ?? false);
                    if (isEnumerated)
                    {
                        CryptoCurrencies cryptoCurrency = cryptocurrencies.First(x => x.GetAttribute<CryptoCodeAttribute>().Code.Equals(cryptoCurrencyCode.Code, StringComparison.OrdinalIgnoreCase));
                        CryptoResponse cryptoResponse = await CryptoService.GetCryptoRateByCode(cryptoCurrency);
                        if (cryptoResponse != null)
                        {
                            EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(cryptoResponse);
                            await SendDeferredEmbedAsync(embed.Build());
                        }
                        else
                        {
                            await SendDeferredApiErrorResponseAsync();
                        }
                    }
                    else
                    {
                        CryptoResponse cryptoResponse = await CryptoService.GetCryptoRateByCode(cryptoCurrencyCode.Code);
                        EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(cryptoResponse, cryptoCurrencyCode.Name);
                        await SendDeferredEmbedAsync(embed.Build());
                    }
                }
                else
                {
                    await SendDeferredMessageAsync($"No hay resultados para la búsqueda. Asegurate de utilizar los resultados autocompletados.");
                }
            }
        }

        #endregion

        [SlashCommand("crypto", "Muestra el valor de una cotización o lista todos los códigos de criptomonedas disponibles.", false, RunMode.Async)]
        public async Task GetCryptoCurrenciesAsync(
            [Summary("código", "Código de la criptomoneda.")]
            [Autocomplete(typeof(CryptoSymbolAutocompleteHandler))]
            string symbol = null,
            [Summary("nombre", "Nombre de la criptomoneda.")]
            [Autocomplete(typeof(CryptoNameAutocompleteHandler))]
            string name = null
        )
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(symbol) && !string.IsNullOrWhiteSpace(name))
                    {
                        await SendDeferredMessageAsync($"{Format.Bold("Atención")}: Debe especificar el {Format.Bold("código")} o el {Format.Bold("nombre")} de la criptomoneda, pero no ambos.");
                    }
                    else
                    {
                        List<CryptoCodeResponse> cryptoCurrenciesList = await CryptoService.GetCryptoCodeList();
                        string cryptoCurrencyCode = symbol != null ? Format.Sanitize(symbol).ToUpper().Trim() : name != null ? Format.Sanitize(name).ToUpper().Trim() : null;
                        await SendCryptoResponseAsync(cryptoCurrenciesList, cryptoCurrencyCode);
                    }
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponseAsync(ex);
                }
            });
        }
    }
}