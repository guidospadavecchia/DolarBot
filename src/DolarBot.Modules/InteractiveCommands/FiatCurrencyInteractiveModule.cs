using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.API.Models.Base;
using DolarBot.API.Services.DolarBotApi;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Autocompletion.FiatCurrency;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Modules.InteractiveCommands.Components.Calculator;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Buttons;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Enums;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Modals;
using DolarBot.Services.Currencies;
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
            string currencyCommand = GetType().GetMethod(nameof(GetCurrenciesAsync)).GetCustomAttributes(true).OfType<SlashCommandAttribute>().First().Name;
            await FollowupAsync($"El código {Format.Code(userInput)} no corresponde con ningún código de moneda válido. Para ver la lista de códigos de monedas disponibles, ejecutá {Format.Code($"/{currencyCommand}")}.");
        }

        /// <summary>
        /// Replies with a message indicating that the date parameter was not correcly specified.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        private async Task SendNoDataForDateAsync(DateTime date)
        {
            await FollowupAsync($"No hay datos históricos para la fecha {Format.Code(date.ToString("dd/MM/yyyy"))}.");
        }

        /// <summary>
        /// Replies with a message indicating one of the date parameters was not correcly specified.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        private async Task SendInvalidDateParameterAsync()
        {
            string currencyCommand = GetType().GetMethod(nameof(GetCurrenciesAsync)).GetCustomAttributes(true).OfType<SlashCommandAttribute>().First().Name;
            string validDateFormats = string.Join(", ", FiatCurrencyService.GetValidDateFormats().Select(x => Format.Code(x)));
            await ReplyAsync($"La fecha desde/hasta es inválida. Formatos de fecha aceptados: {validDateFormats}.");
        }

        #endregion

        #region Components

        [ComponentInteraction($"{FiatCurrencyCalculatorButtonBuilder.Id}:*", runMode: RunMode.Async)]
        public async Task HandleCalculatorButtonClick(string currencyCode)
        {
            await RespondWithModalAsync<FiatCurrencyCalculatorModal>($"{FiatCurrencyCalculatorModal.Id}:{currencyCode}");
        }

        [ModalInteraction($"{FiatCurrencyCalculatorModal.Id}:*", runMode: RunMode.Async)]
        public async Task HandleCalculatorModalInput(string currencyCode, FiatCurrencyCalculatorModal calculatorModal)
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    bool isNumeric = decimal.TryParse(calculatorModal.Value.Replace(",", "."), NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal amount);
                    if (!isNumeric || amount <= 0)
                    {
                        amount = 1;
                    }
                    List<WorldCurrencyCodeResponse> currenciesList = await FiatCurrencyService.GetWorldCurrenciesList();
                    WorldCurrencyCodeResponse worldCurrencyCode = currenciesList.FirstOrDefault(x => x.Code.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
                    if (worldCurrencyCode != null)
                    {
                        WorldCurrencyResponse currencyResponse = await FiatCurrencyService.GetCurrencyValue(currencyCode);
                        EmbedBuilder embed = await FiatCurrencyService.CreateWorldCurrencyEmbedAsync(currencyResponse, worldCurrencyCode.Name, amount);
                        await FollowupAsync(embed: embed.Build());
                    }
                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
                }
            });
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
                            await FollowupAsync(embed: embed.Build(), components: new CalculatorComponentBuilder(currencyCode, CalculatorTypes.FiatCurrency, Configuration).Build());
                        }
                        else
                        {
                            await SendInvalidCurrencyCodeAsync(currencyCode);
                        }
                    }
                    else
                    {
                        int replyTimeout = Convert.ToInt32(Configuration["interactiveMessageReplyTimeout"]);
                        string currencyCommand = GetType().GetMethod(nameof(GetCurrenciesAsync)).GetCustomAttributes(true).OfType<SlashCommandAttribute>().First().Name;

                        EmbedBuilder[] embeds = FiatCurrencyService.CreateWorldCurrencyListEmbedAsync(currenciesList, currencyCommand, Context.User.Username).ToArray();
                        await FollowUpWithPaginatedEmbedAsync(embeds);
                    }
                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
                }
            });
        }

        [SlashCommand("historico", "Muestra valores históricos entre fechas para una moneda determinada.", false, RunMode.Async)]
        public async Task GetHistoricalCurrencyValuesAsync(
            [Summary("moneda", "Código de la moneda.")]
            [Autocomplete(typeof(FiatCurrencyCodeAutocompleteHandler))]
            string codigo,
            [Summary("fecha", "Fecha de la cotización.")]
            string date
        )
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    string currencyCode = Format.Sanitize(codigo).ToUpper().Trim();
                    List<WorldCurrencyCodeResponse> historicalCurrencyCodeList = await FiatCurrencyService.GetWorldCurrenciesList();
                    WorldCurrencyCodeResponse worldCurrencyCodeResponse = historicalCurrencyCodeList.FirstOrDefault(x => x.Code.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
                    if (worldCurrencyCodeResponse != null)
                    {
                        if (FiatCurrencyService.ParseDate(date, out DateTime? dateResult) && dateResult.HasValue)
                        {
                            DateTime date = dateResult.Value;
                            WorldCurrencyResponse historicalValue = await FiatCurrencyService.GetHistoricalCurrencyValue(currencyCode, date);
                            if (historicalValue != null)
                            {
                                EmbedBuilder embed = await FiatCurrencyService.CreateWorldCurrencyEmbedAsync(historicalValue, worldCurrencyCodeResponse.Name, date: date);
                                await FollowupAsync(embed: embed.Build(), components: new CalculatorComponentBuilder(currencyCode, CalculatorTypes.FiatCurrency, Configuration).Build());
                            }
                            else
                            {
                                await SendNoDataForDateAsync(date);
                            }

                        }
                        else
                        {
                            await SendInvalidDateParameterAsync();
                        }
                    }
                    else
                    {
                        await SendInvalidCurrencyCodeAsync(currencyCode);
                    }

                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
                }
            });
        }
    }
}