using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Currencies;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains cryptocurrency related commands.
    /// </summary>
    [HelpOrder(4)]
    [HelpTitle("Cotizaciones del Mundo")]
    public class FiatCurrencyModule : BaseModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about bolivar rates.
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
        public FiatCurrencyModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger)
        {
            FiatCurrencyService = new FiatCurrencyService(configuration, api);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Replies with an embed message for a single currency value.
        /// </summary>
        /// <param name="currencyCode">The currency 3-digit code.</param>
        /// <param name="currenciesList">The collection of valid currency codes.</param>
        private async Task SendCurrencyValueAsync(string currencyCode, List<WorldCurrencyCodeResponse> currenciesList)
        {
            WorldCurrencyCodeResponse worldCurrencyCode = currenciesList.FirstOrDefault(x => x.Code.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
            if (worldCurrencyCode != null)
            {
                WorldCurrencyResponse currencyResponse = await FiatCurrencyService.GetCurrencyValue(currencyCode);
                EmbedBuilder embed = await FiatCurrencyService.CreateWorldCurrencyEmbedAsync(currencyResponse, worldCurrencyCode.Name);
                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                await SendInvalidCurrencyCodeAsync(currencyCode);
            }
        }

        /// <summary>
        /// Replies with a message indicating an invalid currency code.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        private async Task SendInvalidCurrencyCodeAsync(string userInput)
        {
            string commandPrefix = Configuration["commandPrefix"];
            string currencyCommand = GetType().GetMethod(nameof(GetCurrencies)).GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
            await ReplyAsync($"El código {Format.Code(userInput)} no corresponde con ningún código de moneda válido. Para ver la lista de códigos de monedas disponibles, ejecutá {Format.Code($"{commandPrefix}{currencyCommand}")}.");
        }

        /// <summary>
        /// Replies with a message indicating missing parameters for <see cref="GetHistoricalCurrencyValues(string, string, string)"/>.
        /// </summary>
        private async Task SendMissingParameterForHistoricalCurrencyCommandAsync()
        {
            string commandPrefix = Configuration["commandPrefix"];
            string currencyCommand = GetType().GetMethod(nameof(GetCurrencies)).GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
            string historicalcurrencyCommand = GetType().GetMethod(nameof(GetHistoricalCurrencyValues)).GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
            await ReplyAsync($"Este comando requiere un código de moneda válido. Para ver la lista de códigos de monedas disponibles, ejecutá {Format.Code($"{commandPrefix}{currencyCommand}")}. Para conocer más acerca de este comando, ejecutá {Format.Code($"{commandPrefix}ayuda {historicalcurrencyCommand}")}.");
        }

        /// <summary>
        /// Replies with a message indicating one of the date parameters was not correcly specified.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        private async Task SendInvalidDateParameterAsync(string userInput)
        {
            string commandPrefix = Configuration["commandPrefix"];
            string currencyCommand = GetType().GetMethod(nameof(GetCurrencies)).GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
            await ReplyAsync($"La fecha '{Format.Bold(userInput)}' es inválida. Formatos de fecha aceptados: {Format.Code("AAAA/M/D")}, {Format.Code("AAAA-M-D")}, {Format.Code("D/M/AAAA")},{Format.Code("D-M-AAAA")}.");
        }

        /// <summary>
        /// Replies with a message indicating one of the date parameters was not correcly specified.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        private async Task SendInvalidDateRangeParametersAsync(DateTime? startDate, DateTime? endDate)
        {
            await ReplyAsync($"La fecha desde ({Format.Code((startDate?.Date ?? DateTime.Now.Date).ToString("dd/MM/yyyy"))}) debe ser menor o igual a la fecha hasta ({Format.Code((endDate?.Date ?? DateTime.Now.Date).ToString("dd/MM/yyyy"))}).");
        }

        /// <summary>
        /// Replies with a message indicating one of the date parameters was not correcly specified.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        private async Task SendNoDataForRangeAsync(DateTime? startDate, DateTime? endDate)
        {
            await ReplyAsync($"No hay datos históricos para el rango de fechas {Format.Code((startDate?.Date ?? DateTime.Now).ToString("dd/MM/yyyy"))} - {Format.Code((endDate?.Date ?? DateTime.Now).ToString("dd/MM/yyyy"))}.");
        }

        #endregion

        [Command("moneda", RunMode = RunMode.Async)]
        [Alias("m")]
        [Summary("Muestra el valor de una cotización o lista todos los códigos de monedas disponibles.")]
        [HelpUsageExample(false, "$moneda", "$m", "$moneda CAD", "$ct AUD")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetCurrencies
        (
            [Summary("Código de la moneda a mostrar. Si no se especifica, mostrará la lista de todos los códigos de monedas disponibles.")]
            string codigo = null
        )
        {
            try
            {
                IDisposable typingState = Context.Channel.EnterTypingState();
                List<WorldCurrencyCodeResponse> currenciesList = await FiatCurrencyService.GetWorldCurrenciesList();

                if (codigo != null)
                {
                    string currencyCode = Format.Sanitize(codigo).RemoveFormat(true).ToUpper().Trim();
                    await SendCurrencyValueAsync(currencyCode, currenciesList);
                    typingState.Dispose();
                }
                else
                {
                    string commandPrefix = Configuration["commandPrefix"];
                    int replyTimeout = Convert.ToInt32(Configuration["interactiveMessageReplyTimeout"]);
                    string currencyCommand = GetType().GetMethod(nameof(GetCurrencies)).GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;

                    List<EmbedBuilder> embeds = FiatCurrencyService.CreateWorldCurrencyListEmbedAsync(currenciesList, currencyCommand, Context.User.Username);
                    await SendPagedReplyAsync(embeds, true);
                    typingState.Dispose();

                    SocketMessage userResponse = await NextMessageAsync(timeout: TimeSpan.FromSeconds(replyTimeout));
                    if (userResponse != null)
                    {
                        string currencyCode = Format.Sanitize(userResponse.Content).RemoveFormat(true).Trim();
                        if (!currencyCode.StartsWith(commandPrefix))
                        {
                            using (Context.Channel.EnterTypingState())
                            {
                                await SendCurrencyValueAsync(currencyCode, currenciesList);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("historico", RunMode = RunMode.Async)]
        [Alias("h")]
        [Summary("Muestra valores históricos entre fechas para una moneda determinada. Formatos válidos de fecha: `AAAA`, `AAAA/M`, `AAAA-M`, `AAAA/M/D`, `AAAA-M-D`, `D/M/AAAA`, `D-M-AAAA`, `hoy`.")]
        [HelpUsageExample(false, "$historico USD", "$h EUR", "$h AUD 01/01/2010", "$h VES 2010 2012", "$historico CAD", "$historico CAD 2010-01-01 2020-12-31", "$historico ZAR 2015-08")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetHistoricalCurrencyValues
        (
            [Summary("Código de la moneda a mostrar.")]
            string codigo = null,
            [Summary("(Opcional) Fecha desde. Si no se especifica, se mostrará todo el histórico.")]
            string fechaDesde = null,
            [Summary("(Opcional) Fecha hasta. Si no se especifica, se tomará la fecha del día actual.")]
            string fechaHasta = null
        )
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    if (string.IsNullOrWhiteSpace(codigo))
                    {
                        await SendMissingParameterForHistoricalCurrencyCommandAsync();
                    }
                    else
                    {
                        string currencyCode = Format.Sanitize(codigo).RemoveFormat(true).ToUpper().Trim();
                        List<WorldCurrencyCodeResponse> historicalCurrencyCodeList = await FiatCurrencyService.GetWorldCurrenciesList();
                        WorldCurrencyCodeResponse worldCurrencyCodeResponse = historicalCurrencyCodeList.FirstOrDefault(x => x.Code.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
                        if (worldCurrencyCodeResponse != null)
                        {
                            DateTime? startDate = null;
                            DateTime? endDate = null;
                            if (string.IsNullOrWhiteSpace(fechaDesde) || FiatCurrencyService.ParseDate(fechaDesde, out startDate))
                            {
                                if (string.IsNullOrWhiteSpace(fechaHasta) || FiatCurrencyService.ParseDate(fechaHasta, out endDate))
                                {
                                    if ((startDate?.Date ?? DateTime.Now) <= (endDate?.Date ?? DateTime.Now))
                                    {
                                        List<WorldCurrencyResponse> historicalCurrencyValues = await FiatCurrencyService.GetHistoricalCurrencyValues(currencyCode, startDate, endDate);
                                        if (historicalCurrencyValues != null && historicalCurrencyValues.Count > 0)
                                        {
                                            List<EmbedBuilder> embeds = FiatCurrencyService.CreateHistoricalValuesEmbedsAsync(historicalCurrencyValues, worldCurrencyCodeResponse.Name, startDate?.Date, endDate?.Date);
                                            await SendPagedReplyAsync(embeds, true);
                                        }
                                        else
                                        {
                                            await SendNoDataForRangeAsync(startDate, endDate);
                                        }
                                    }
                                    else
                                    {
                                        await SendInvalidDateRangeParametersAsync(startDate, endDate);
                                    }
                                }
                                else
                                {
                                    await SendInvalidDateParameterAsync(fechaHasta);
                                }
                            }
                            else
                            {
                                await SendInvalidDateParameterAsync(fechaDesde);
                            }
                        }
                        else
                        {
                            await SendInvalidCurrencyCodeAsync(currencyCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }
    }
}