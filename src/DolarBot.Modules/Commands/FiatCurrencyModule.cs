using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Currencies;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmbedPage = Discord.Addons.Interactive.PaginatedMessage.Page;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains cryptocurrency related commands.
    /// </summary>
    [HelpOrder(4)]
    [HelpTitle("Cotizaciones del Mundo")]
    public class FiatCurrencyModule : BaseInteractiveModule
    {
        #region Constants
        /// <summary>
        /// How many currencies to fit into each <see cref="EmbedPage"/>.
        /// </summary>
        private const int CURRENCIES_PER_PAGE = 25; 
        #endregion

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
                string commandPrefix = Configuration["commandPrefix"];
                string currencyCommand = GetType().GetMethod(nameof(GetCurrencies)).GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
                await ReplyAsync($"El código {Format.Code(currencyCode)} no corresponde con ningún código válido. Para ver la lista de códigos de monedas disponibles, ejecutá {Format.Code($"{commandPrefix}{currencyCommand}")}.");
            }
        }

        #endregion

        [Command("moneda", RunMode = RunMode.Async)]
        [Alias("m")]
        [Summary("Muestra el valor de una cotización o lista todos los códigos de monedas disponibles.")]
        [HelpUsageExample(false, "$moneda", "$m", "$moneda CAD", "$ct AUD")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetCurrencies(
            [Summary("Código de la moneda a mostrar. Si no se especifica, mostrará la lista de todos los códigos de monedas disponibles.")]
            string codigo = null)
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

                    Emoji coinEmoji = new Emoji(":coin:");
                    string currencyCommand = GetType().GetMethod(nameof(GetCurrencies)).GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
                    string coinsImageUrl = Configuration.GetSection("images").GetSection("coins")["64"];
                    TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();

                    int pageCount = 0;
                    int totalPages = (int)Math.Ceiling(Convert.ToDecimal(currenciesList.Count) / CURRENCIES_PER_PAGE);
                    List<IEnumerable<WorldCurrencyCodeResponse>> currenciesListPages = currenciesList.ChunkBy(CURRENCIES_PER_PAGE);

                    List<EmbedBuilder> embeds = new List<EmbedBuilder>();
                    List<EmbedPage> pages = new List<EmbedPage>();

                    foreach (IEnumerable<WorldCurrencyCodeResponse> currenciesPage in currenciesListPages)
                    {
                        string currencyList = string.Join(Environment.NewLine, currenciesPage.Select(x => $"{coinEmoji} {Format.Code(x.Code)}: {Format.Italics(x.Name)}."));
                        EmbedBuilder embed = new EmbedBuilder().AddField(GlobalConfiguration.Constants.BLANK_SPACE, currencyList)
                                                               .AddField(GlobalConfiguration.Constants.BLANK_SPACE, $"{Format.Bold(Context.User.Username)}, para ver una cotización, respondé a este mensaje antes de las {Format.Bold(TimeZoneInfo.ConvertTime(DateTime.Now.AddSeconds(replyTimeout), localTimeZone).ToString("HH:mm:ss"))} con el {Format.Bold("código de 3 dígitos")} de la moneda.{Environment.NewLine}Por ejemplo: {Format.Code(currenciesList.First().Code)}.")
                                                               .AddField(GlobalConfiguration.Constants.BLANK_SPACE, $"{Format.Bold("Tip")}: {Format.Italics("Si ya sabés el código de la moneda, podés indicárselo al comando directamente, por ejemplo:")} {Format.Code($"{commandPrefix}{currencyCommand} {currenciesList.First().Code}")}.");
                        embeds.Add(embed);
                    }

                    foreach (EmbedBuilder embed in embeds)
                    {
                        pages.Add(new EmbedPage
                        {
                            Description = $"Códigos de monedas disponibles para utilizar como parámetro del comando {Format.Code($"{commandPrefix}{currencyCommand}")}.",
                            Title = "Monedas del mundo disponibles",
                            Fields = embed.Fields,
                            Color = GlobalConfiguration.Colors.Currency,
                            FooterOverride = new EmbedFooterBuilder
                            {
                                Text = $"Página {++pageCount} de {totalPages}"
                            },
                            ThumbnailUrl = coinsImageUrl
                        });
                    }

                    await SendPagedReplyAsync(pages, true);
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
    }
}