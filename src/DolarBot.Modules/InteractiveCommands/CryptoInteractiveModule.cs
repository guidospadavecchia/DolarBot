using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Attributes;
using DolarBot.API.Enums;
using DolarBot.API.Models;
using DolarBot.API.Services.DolarBotApi;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Autocompletion.Crypto;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Modules.InteractiveCommands.Components.Calculator;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Buttons;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Enums;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Modals;
using DolarBot.Services.Crypto;
using DolarBot.Util.Extensions;
using Fergun.Interactive;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
        /// <param name="quantity">Crypto currency quantity.</param>
        private async Task SendCryptoResponseAsync(List<CryptoCodeResponse> cryptoCurrenciesList, string code, decimal quantity = 1)
        {
            CryptoCodeResponse cryptoCurrencyCode = cryptoCurrenciesList.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            if (cryptoCurrencyCode != null)
            {
                CryptoResponse cryptoResponse;
                string cryptoCurrencyName = null;
                List<CryptoCurrencies> cryptocurrencies = Enum.GetValues(typeof(CryptoCurrencies)).Cast<CryptoCurrencies>().ToList();
                bool isEnumerated = cryptocurrencies.Any(x => x.GetAttribute<CryptoCodeAttribute>()?.Code.Equals(cryptoCurrencyCode.Code, StringComparison.OrdinalIgnoreCase) ?? false);
                if (isEnumerated)
                {
                    CryptoCurrencies cryptoCurrency = cryptocurrencies.First(x => x.GetAttribute<CryptoCodeAttribute>().Code.Equals(cryptoCurrencyCode.Code, StringComparison.OrdinalIgnoreCase));
                    cryptoResponse = await CryptoService.GetCryptoRateByCode(cryptoCurrency);
                }
                else
                {
                    cryptoResponse = await CryptoService.GetCryptoRateByCode(cryptoCurrencyCode.Code);
                    cryptoCurrencyName = cryptoCurrencyCode.Name;
                }

                if (cryptoResponse != null)
                {
                    EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(cryptoResponse, cryptoCurrencyName, quantity);
                    await SendDeferredEmbedAsync(embed.Build(), new CalculatorComponentBuilder(cryptoCurrencyCode.Code, CalculatorTypes.Crypto, Configuration).Build());
                }
                else
                {
                    await SendDeferredApiErrorResponseAsync();
                }
            }
            else
            {
                await SendDeferredMessageAsync($"No hay resultados para la búsqueda. Asegurate de utilizar los resultados autocompletados.");
            }
        }

        #endregion

        #region Components

        [ComponentInteraction($"{CryptoCalculatorButtonBuilder.Id}:*", runMode: RunMode.Async)]
        public async Task HandleCalculatorButtonClick(string cryptoCode)
        {
            await RespondWithModalAsync<CryptoCalculatorModal>($"{CryptoCalculatorModal.Id}:{cryptoCode}");
        }

        [ModalInteraction($"{CryptoCalculatorModal.Id}:*", runMode: RunMode.Async)]
        public async Task HandleCalculatorModalInput(string cryptoCode, CryptoCalculatorModal calculatorModal)
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                bool isNumeric = decimal.TryParse(calculatorModal.Value.Replace(",", "."), NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal quantity);
                if (!isNumeric || quantity <= 0)
                {
                    quantity = 1;
                }
                List<CryptoCodeResponse> cryptoCurrenciesList = await CryptoService.GetCryptoCodeList();
                await SendCryptoResponseAsync(cryptoCurrenciesList, cryptoCode, quantity);
            });
        }

        #endregion


        [SlashCommand("crypto", "Muestra el valor de una cotización o lista todos los códigos de criptomonedas disponibles.", false, RunMode.Async)]
        public async Task GetCryptoCurrenciesAsync(
            [Summary("criptomoneda", "Código o nombre de la criptomoneda.")]
            [Autocomplete(typeof(CryptoAutocompleteHandler))]
            string value
        )
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    List<CryptoCodeResponse> cryptoCurrenciesList = await CryptoService.GetCryptoCodeList();
                    string cryptoCurrencyCode = value != null ? Format.Sanitize(value).ToUpper().Trim() : null;
                    if (cryptoCurrencyCode != null)
                    {
                        await SendCryptoResponseAsync(cryptoCurrenciesList, cryptoCurrencyCode);
                    }
                    else
                    {
                        await SendDeferredCryptoCurrencyListAsync(cryptoCurrenciesList);
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