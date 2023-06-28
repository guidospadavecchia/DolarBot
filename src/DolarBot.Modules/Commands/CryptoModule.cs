using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Crypto;
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
    [HelpOrder(6)]
    [HelpTitle("Crypto")]
    public class CryptoModule : BaseCryptoModule
    {
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
                CryptoResponse cryptoResponse = await CryptoService.GetCryptoRateByCode(cryptoCurrencyCodeResponse.Code);
                await SendCryptoReply(cryptoResponse, cryptoCurrencyCodeResponse.Name);
            }
            else
            {
                string commandPrefix = Configuration["commandPrefix"];
                string currencyCommand = GetType().GetMethod(nameof(GetCryptoRatesAsync)).GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
                await ReplyAsync($"El código {Format.Code(searchText)} no corresponde con ningún identificador válido. Para ver la lista de identificadores de criptomonedas disponibles, ejecutá {Format.Code($"{commandPrefix}{currencyCommand}")}.");
            }

            typingState?.Dispose();
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

            List<EmbedBuilder> embeds = CryptoService.CreateCryptoListEmbedAsync(currenciesList, currencyCommand, isSubSearch, Context.User.Username, title, description);
            await SendPagedReplyAsync(embeds, true);
            typingState?.Dispose();

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
                    CryptoResponse result = await CryptoService.GetCryptoRateByCode("binancecoin");
                    await SendCryptoReply(result, "Binance Coin");
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
                    CryptoResponse result = await CryptoService.GetCryptoRateByCode("bitcoin");
                    await SendCryptoReply(result, "Bitcoin");
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
                    CryptoResponse result = await CryptoService.GetCryptoRateByCode("bitcoin-cash");
                    await SendCryptoReply(result, "Bitcoin Cash");
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
                    CryptoResponse result = await CryptoService.GetCryptoRateByCode("cardano");
                    await SendCryptoReply(result, "Cardano");
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
                    CryptoResponse result = await CryptoService.GetCryptoRateByCode("chainlink");
                    await SendCryptoReply(result, "Chainlink");
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }
    }
}