using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DolarBot.API;
using DolarBot.API.Attributes;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Crypto;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DolarBot.API.ApiCalls.DolarBotApi;
using EmbedPage = Discord.Addons.Interactive.PaginatedMessage.Page;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains cryptocurrency related commands.
    /// </summary>
    [HelpOrder(6)]
    [HelpTitle("Crypto")]
    public class CryptoModule : BaseCryptoModule
    {
        #region Constants
        /// <summary>
        /// How many currencies to fit into each <see cref="EmbedPage"/>.
        /// </summary>
        private const int CURRENCIES_PER_PAGE = 12;
        /// <summary>
        /// How many inline fields to show per <see cref="EmbedPage"/>.
        /// </summary>
        private const int INLINE_FIELDS_PER_PAGE = 2;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public CryptoModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger, api) { }
        #endregion

        #region Methods

        /// <summary>
        /// Replies with an embed message for a single cryptocurrency value.
        /// </summary>
        /// <param name="searchText">The text to be searched.</param>
        /// <param name="cryptoCurrenciesList">The collection of valid currency codes.</param>
        /// <param name="allowMultipleResults">Indicates wether the search result can return multiple results.</param>
        private async Task SendCryptoCurrencyValueAsync(string searchText, List<CryptoCodeResponse> cryptoCurrenciesList, bool allowMultipleResults = true, IDisposable typingState = null)
        {
            List<CryptoCodeResponse> cryptoCurrencyCodeResponses = CryptoService.FilterByText(cryptoCurrenciesList, searchText);
            if (allowMultipleResults && cryptoCurrencyCodeResponses.Count > 1)
            {
                string title = $"Multiples resultados para {Format.Code(searchText)}";
                string description = $"La búsqueda retornó multiples resultados.";
                await SendCryptoCurrencyListAsync(cryptoCurrencyCodeResponses, true, title, description, typingState);
            }
            else if (cryptoCurrencyCodeResponses.Count == 1)
            {
                CryptoCodeResponse cryptoCurrencyCodeResponse = cryptoCurrencyCodeResponses.First();
                List<CryptoCurrencies> cryptocurrencies = Enum.GetValues(typeof(CryptoCurrencies)).Cast<CryptoCurrencies>().ToList();
                bool isEnumerated = cryptocurrencies.Any(x => x.GetAttribute<CryptoCodeAttribute>()?.Code.Equals(cryptoCurrencyCodeResponse.Code, StringComparison.OrdinalIgnoreCase) ?? false);
                if (isEnumerated)
                {
                    CryptoCurrencies cryptoCurrency = cryptocurrencies.First(x => x.GetAttribute<CryptoCodeAttribute>().Code.Equals(cryptoCurrencyCodeResponse.Code, StringComparison.OrdinalIgnoreCase));
                    CryptoResponse cryptoResponse = await CryptoService.GetCryptoRateByCode(cryptoCurrency);
                    await SendCryptoReply(cryptoResponse);
                }
                else
                {
                    CryptoResponse cryptoResponse = await CryptoService.GetCryptoRateByCode(cryptoCurrencyCodeResponse.Code);
                    EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(cryptoResponse, cryptoCurrencyCodeResponse.Name);
                    await ReplyAsync(embed: embed.Build());
                }
            }
            else
            {
                string commandPrefix = Configuration["commandPrefix"];
                string currencyCommand = GetType().GetMethod(nameof(GetCryptoRatesAsync)).GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
                await ReplyAsync($"El código {Format.Code(searchText)} no corresponde con ningún identificador válido. Para ver la lista de identificadores de criptomonedas disponibles, ejecutá {Format.Code($"{commandPrefix}{currencyCommand}")}.");
            }

            if (typingState != null)
            {
                typingState.Dispose();
            }
        }

        /// <summary>
        /// Replies with a paginated embed message containing all the cryptocurrency codes.
        /// </summary>
        /// <param name="currenciesList">The collection of valid currency codes.</param>
        /// <param name="isSubSearch">Indicates wether the <paramref name="currenciesList"/> is a subset of the initial collection.</param>
        /// <param name="typingState">Optional. The active typing state.</param>
        private async Task SendCryptoCurrencyListAsync(List<CryptoCodeResponse> currenciesList, bool isSubSearch = false, string title = null, string description = null, IDisposable typingState = null)
        {
            string commandPrefix = Configuration["commandPrefix"];
            int replyTimeout = Convert.ToInt32(Configuration["interactiveMessageReplyTimeout"]);
            string currencyCommand = GetType().GetMethod(nameof(GetCryptoRatesAsync)).GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;

            var emojis = Configuration.GetSection("customEmojis");
            Emoji cryptoEmoji = new Emoji(emojis["cryptoCoin"]);
            string coinsImageUrl = Configuration.GetSection("images").GetSection("crypto")["default"];
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();

            int pageCount = 0;
            int totalPages = (int)Math.Ceiling(Convert.ToDecimal(currenciesList.Count) / CURRENCIES_PER_PAGE);
            List<IEnumerable<CryptoCodeResponse>> currenciesListPages = currenciesList.ChunkBy(CURRENCIES_PER_PAGE);

            List<EmbedBuilder> embeds = new List<EmbedBuilder>();
            List<EmbedPage> pages = new List<EmbedPage>();

            foreach (IEnumerable<CryptoCodeResponse> currenciesPage in currenciesListPages)
            {
                List<IEnumerable<CryptoCodeResponse>> chunks = currenciesPage.ChunkBy(CURRENCIES_PER_PAGE / INLINE_FIELDS_PER_PAGE);
                EmbedBuilder embed = new EmbedBuilder();
                foreach (IEnumerable<CryptoCodeResponse> chunk in chunks)
                {
                    string currencyList = string.Join(Environment.NewLine, chunk.Select(x => $"{cryptoEmoji} {Format.Bold($"[{x.Symbol}]")} {Format.Code(x.Code)}: {Format.Italics(x.Name)}."));
                    embed = embed.AddInlineField(GlobalConfiguration.Constants.BLANK_SPACE, currencyList);
                }

                embed = embed.AddField(GlobalConfiguration.Constants.BLANK_SPACE, $"{Format.Bold(Context.User.Username)}, para ver una cotización, respondé a este mensaje antes de las {Format.Bold(TimeZoneInfo.ConvertTime(DateTime.Now.AddSeconds(replyTimeout), localTimeZone).ToString("HH:mm:ss"))} con el {(!isSubSearch ? $"{Format.Bold("código")} ó " : string.Empty)}{Format.Bold("identificador")} de la criptomoneda.{Environment.NewLine}Por ejemplo: {Format.Code(currenciesList.First().Code)}.");
                if (!isSubSearch)
                {
                    embed = embed.AddField(GlobalConfiguration.Constants.BLANK_SPACE, $"{Format.Bold("Tip")}: {Format.Italics($"Si ya sabés el identificador de la criptomoneda, podés indicárselo al comando directamente.{Environment.NewLine}Por ejemplo:")} {Format.Code($"{commandPrefix}{currencyCommand} {currenciesList.First().Code}")}.");
                }
                embeds.Add(embed);
            }

            foreach (EmbedBuilder embed in embeds)
            {
                pages.Add(new EmbedPage
                {
                    Title = title ?? "Criptomonedas disponibles",
                    Description = description ?? $"Identificadores de criptomonedas disponibles para utilizar como parámetro del comando {Format.Code($"{commandPrefix}{currencyCommand}")}.",
                    Fields = embed.Fields,
                    Color = GlobalConfiguration.Colors.Crypto,
                    FooterOverride = new EmbedFooterBuilder
                    {
                        Text = $"Página {++pageCount} de {totalPages}"
                    },
                    ThumbnailUrl = coinsImageUrl
                });
            }

            await SendPagedReplyAsync(pages, true);
            if (typingState != null)
            {
                typingState.Dispose();
            }

            SocketMessage userResponse = await NextMessageAsync(timeout: TimeSpan.FromSeconds(replyTimeout));
            if (userResponse != null)
            {
                string currencyCode = Format.Sanitize(userResponse.Content).RemoveFormat(true).Trim();
                if (!currencyCode.StartsWith(commandPrefix))
                {
                    using (Context.Channel.EnterTypingState())
                    {
                        await SendCryptoCurrencyValueAsync(currencyCode, currenciesList, !isSubSearch, typingState);
                    }
                }
            }
        }

        #endregion

        [Command("crypto", RunMode = RunMode.Async)]
        [Alias("ct")]
        [Summary("Muestra el valor de una cotización o lista todos los códigos de criptomonedas disponibles.")]
        [HelpUsageExample(false, "$crypto", "$ct", "$crypto bitcoin", "$ct ethereum")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetCryptoRatesAsync(
            [Summary("Texto de búsqueda de la criptomoneda a mostrar (código, identificador o nombre). Si no se especifica, mostrará la lista de todos los códigos de criptomonedas disponibles.")]
            string input = null)
        {
            try
            {
                IDisposable typingState = Context.Channel.EnterTypingState();
                List<CryptoCodeResponse> currenciesList = await CryptoService.GetCryptoCodeList();

                if (input != null)
                {
                    string currencyCode = Format.Sanitize(input).RemoveFormat(true).ToUpper().Trim();
                    await SendCryptoCurrencyValueAsync(currencyCode, currenciesList, typingState: typingState);
                }
                else
                {
                    await SendCryptoCurrencyListAsync(currenciesList, typingState: typingState);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("binance", RunMode = RunMode.Async)]
        [Alias("bnb")]
        [Summary("Muestra la cotización del Binance Coin (BNB) en pesos y dólares.")]
        [HelpUsageExample(false, "$binance", "$bnb")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetBinanceCoinRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetBinanceCoinRate();
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("bitcoin", RunMode = RunMode.Async)]
        [Alias("btc")]
        [Summary("Muestra la cotización del Bitcoin (BTC) en pesos y dólares.")]
        [HelpUsageExample(false, "$bitcoin", "$btc")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetBitcoinRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetBitcoinRate();
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("bitcoincash", RunMode = RunMode.Async)]
        [Alias("bch")]
        [Summary("Muestra la cotización del Bitcoin Cash (BCH) en pesos y dólares.")]
        [HelpUsageExample(false, "$bitcoincash", "$bch")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetBitcoinCashRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetBitcoinCashRate();
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("cardano", RunMode = RunMode.Async)]
        [Alias("ada")]
        [Summary("Muestra la cotización del Cardano (ADA) en pesos y dólares.")]
        [HelpUsageExample(false, "$cardano", "$ada")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetCardanoRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetCardanoRate();
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("chainlink", RunMode = RunMode.Async)]
        [Alias("link")]
        [Summary("Muestra la cotización del Chainlink (LINK) en pesos y dólares.")]
        [HelpUsageExample(false, "$chainlink", "$link")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetChainlinkRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetChainlinkRate();
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }
    }
}