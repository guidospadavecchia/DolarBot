using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Autocompletion.FiatCurrency;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Services.Currencies;
using DolarBot.Util.Extensions;
using Fergun.Interactive;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.InteractiveCommands
{
    /// <summary>
    /// Contains the world currencies related commands.
    /// </summary>
    [HelpOrder(10)]
    [HelpTitle("Cotizaciones del Mundo")]
    public class FiatCurrencyInteractiveModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about fiat currencies rates.
        /// </summary>
        private readonly FiatCurrencyService FiatCurrencyService;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        /// <param name="interactiveService">The interactive service.</param>
        public FiatCurrencyInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api, InteractiveService interactiveService) : base(configuration, logger, interactiveService)
        {
            FiatCurrencyService = new FiatCurrencyService(configuration, api);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Replies with a message indicating an invalid currency code.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        private async Task SendInvalidCurrencyCodeAsync(string userInput)
        {
            string commandPrefix = Configuration["commandPrefix"];
            string currencyCommand = GetType().GetMethod(nameof(GetCurrenciesAsync)).GetCustomAttributes(true).OfType<SlashCommandAttribute>().First().Name;
            await SendDeferredMessageAsync($"El código {Format.Code(userInput)} no corresponde con ningún código de moneda válido. Para ver la lista de códigos de monedas disponibles, ejecutá {Format.Code($"{commandPrefix}{currencyCommand}")}.");
        }

        #endregion

        [SlashCommand("cotizacion", "Muestra el valor de una cotización o lista todos los códigos de monedas disponibles.", false, RunMode.Async)]
        public async Task GetCurrenciesAsync(
            [Summary("moneda", "Código de la moneda. Si no se especifica, mostrará todos los códigos de monedas disponibles.")]
            [Autocomplete(typeof(FiatCurrencyCodeAutocompleteHandler))]
            string codigo = null
        )
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    List<WorldCurrencyCodeResponse> currenciesList = await FiatCurrencyService.GetWorldCurrenciesList();

                    if (codigo != null)
                    {
                        string currencyCode = Format.Sanitize(codigo).ToUpper().Trim();
                        WorldCurrencyCodeResponse worldCurrencyCode = currenciesList.FirstOrDefault(x => x.Code.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
                        if (worldCurrencyCode != null)
                        {
                            WorldCurrencyResponse currencyResponse = await FiatCurrencyService.GetCurrencyValue(currencyCode);
                            EmbedBuilder embed = await FiatCurrencyService.CreateWorldCurrencyEmbedAsync(currencyResponse, worldCurrencyCode.Name);
                            await SendDeferredEmbedAsync(embed.Build());
                        }
                        else
                        {
                            await SendInvalidCurrencyCodeAsync(currencyCode);
                        }
                    }
                    else
                    {
                        string commandPrefix = Configuration["commandPrefix"];
                        int replyTimeout = Convert.ToInt32(Configuration["interactiveMessageReplyTimeout"]);
                        string currencyCommand = GetType().GetMethod(nameof(GetCurrenciesAsync)).GetCustomAttributes(true).OfType<SlashCommandAttribute>().First().Name;

                        EmbedBuilder[] embeds = FiatCurrencyService.CreateWorldCurrencyListEmbedAsync(currenciesList, currencyCommand, Context.User.Username).ToArray();
                        await SendDeferredPaginatedEmbedAsync(embeds);
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