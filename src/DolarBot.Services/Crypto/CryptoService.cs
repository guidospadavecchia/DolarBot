using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.API.Services.DolarBotApi;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Services.Crypto
{
    /// <summary>
    /// Contains several methods to process Euro commands.
    /// </summary>
    public class CryptoService : BaseService
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

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="CryptoService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public CryptoService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        /// <summary>
        /// Filters the <paramref name="cryptoCurrenciesList"/> searching first by code (equals), then by symbol (equals) and lastly by name (contains).
        /// </summary>
        /// <param name="cryptoCurrenciesList">The cryptocurrency list.</param>
        /// <param name="searchText">The text to search.</param>
        /// <returns></returns>
        public static List<CryptoCodeResponse> FilterByText(List<CryptoCodeResponse> cryptoCurrenciesList, string searchText)
        {
            CryptoCodeResponse cryptoCodeResponse = cryptoCurrenciesList.FirstOrDefault(x => x.Code.Equals(searchText, StringComparison.OrdinalIgnoreCase));
            if (cryptoCodeResponse != null)
            {
                return new List<CryptoCodeResponse>() { cryptoCodeResponse };
            }
            else
            {
                List<CryptoCodeResponse> cryptoResults = cryptoCurrenciesList.Where(x => x.Symbol.Equals(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
                if (cryptoResults.Count > 0)
                {
                    return cryptoResults;
                }
                else
                {
                    return searchText.Length >= 3 ? cryptoCurrenciesList.Where(x => x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList() : new List<CryptoCodeResponse>();
                }
            }
        }

        #region API Calls

        /// <summary>
        /// Fetches all the available cryptocurrency codes.
        /// </summary>
        /// <returns>A collection of <see cref="CryptoCodeResponse"/> objects.</returns>
        public async Task<List<CryptoCodeResponse>> GetCryptoCodeList()
        {
            return await Api.DolarBot.GetCryptoCurrenciesList();
        }

        /// <summary>
        /// Fetches a single cryptocurrency rate.
        /// </summary>
        /// <param name="cryptoCurrencyCode">The cryptocurrency code as a string.</param>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetCryptoRateByCode(string cryptoCurrencyCode)
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(cryptoCurrencyCode);
        }

        #endregion

        #region Embed

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a single crypto response.
        /// </summary>
        /// <param name="cryptoResponse">The crypto response object.</param>
        /// <param name="cryptoCurrencyName">A custom cryptocurrency name.</param>
        /// <param name="quantity">The cryptocurrency quantity.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public async Task<EmbedBuilder> CreateCryptoEmbedAsync(CryptoResponse cryptoResponse, string cryptoCurrencyName, decimal quantity = 1)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji cryptoEmoji = new(emojis["cryptoCoin"]);
            Emoji argentinaEmoji = new(":flag_ar:");
            Emoji usaEmoji = new(":flag_us:");
            Emoji dollarEmoji = Emoji.Parse(":dollar:");
            Emoji moneyBagEmoji = Emoji.Parse(":moneybag:");
            Emoji whatsappEmoji = new(emojis["whatsapp"]);

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string cryptoCode = cryptoResponse.Code.Length > 10 ? $"{cryptoResponse.Code[..7]}..." : cryptoResponse.Code;
            string lastUpdated = cryptoResponse.Date.ToString(cryptoResponse.Date.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");

            decimal? arsPriceValue = decimal.TryParse(cryptoResponse?.ARS, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal ars) ? ars * quantity : null;
            decimal? arsPriceWithTaxesValue = decimal.TryParse(cryptoResponse?.ARSTaxed, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal arsTaxed) ? arsTaxed * quantity : null;
            decimal? usdPriceValue = decimal.TryParse(cryptoResponse?.USD, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal usd) ? usd * quantity : null;
            decimal? usd24hVolume = decimal.TryParse(cryptoResponse?.Usd24hVolume, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal volume24h) ? volume24h : null;
            decimal? usd24hChange = decimal.TryParse(cryptoResponse?.Usd24hChange, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal change24h) ? change24h : null;
            decimal? usdMarketCap = decimal.TryParse(cryptoResponse?.UsdMarketCap, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal marketCap) ? marketCap : null;

            string arsPrice = arsPriceValue.HasValue ? arsPriceValue.Value.ToString($"N{(arsPriceValue.Value < 1 ? 8 : 2)}", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string arsPriceWithTaxes = arsPriceWithTaxesValue.HasValue ? arsPriceWithTaxesValue.Value.ToString($"N{(arsPriceWithTaxesValue.Value < 1 ? 8 : 2)}", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string usdPrice = usdPriceValue.HasValue ? usdPriceValue.Value.ToString($"N{(usdPriceValue.Value < 1 ? 8 : 2)}", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string shareText = $"*{cryptoCurrencyName} ({cryptoCode})*{Environment.NewLine}{Environment.NewLine}*{quantity} {cryptoCode}*{Environment.NewLine}Dólares: \t\tUS$ *{usdPrice}*{Environment.NewLine}Pesos: \t\t$ *{arsPrice}*{Environment.NewLine}Pesos c/Imp: \t$ *{arsPriceWithTaxes}*{Environment.NewLine}Hora: \t\t{lastUpdated} (UTC {utcOffset})";
            string usd24hVolumeText = usd24hVolume.HasValue ? $"{dollarEmoji} {Format.Bold($"$ {usd24hVolume.Value.Format()}")}" : "?";
            string marketCapText = usdMarketCap.HasValue ? $"{moneyBagEmoji} {Format.Bold($"$ {usdMarketCap.Value.Format()}")}" : "?";

            decimal variation24h = usd24hChange.GetValueOrDefault();
            Emoji variationEmoji = variation24h > 0 ? new(emojis["arrowUpGreen"]) : (variation24h < 0 ? new Emoji(emojis["arrowDownRed"]) : new Emoji(emojis["neutral"]));

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Crypto)
                                                   .WithTitle($"{cryptoCurrencyName} ({cryptoCode})")
                                                   .WithDescription($"Cotización de {Format.Bold(cryptoCurrencyName)} ({Format.Bold(cryptoResponse.Code)}) expresada en {Format.Bold("pesos argentinos")} y {Format.Bold("dólares estadounidenses")}.".AppendLineBreak())
                                                   .WithThumbnailUrl(cryptoResponse.Image.Large)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} (UTC {utcOffset})", footerImageUrl)
                                                   .AddInlineField("Variación 24hs", $"{variationEmoji} {Format.Bold($"{variation24h}%")}")
                                                   .AddInlineField("Volumen 24hs (USD)", usd24hVolumeText)
                                                   .AddInlineField("Cap. de mercado (USD)", marketCapText.AppendLineBreak())
                                                   .AddField($"{usaEmoji} USD", $"{cryptoEmoji} {Format.Bold($"{quantity} {cryptoCode}")} = {Format.Bold($"US$ {usdPrice}")}".AppendLineBreak())
                                                   .AddInlineField($"{argentinaEmoji} ARS", $"{cryptoEmoji} {Format.Bold($"{quantity} {cryptoCode}")} = {Format.Bold($"$ {arsPrice}")} {GlobalConfiguration.Constants.BLANK_SPACE}")
                                                   .AddInlineField($"{argentinaEmoji} ARS con Impuestos", $"{cryptoEmoji} {Format.Bold($"{quantity} {cryptoCode}")} = {Format.Bold($"$ {arsPriceWithTaxes}")} {GlobalConfiguration.Constants.BLANK_SPACE}".AppendLineBreak());

            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText, Api.Cuttly.ShortenUrl);
            return embed.AddPlayStoreLink(Configuration);
        }

        /// <summary>
        /// Creates a collection of <see cref="EmbedBuilder"/> objects representing a list of cryptocurrency codes.
        /// </summary>
        /// <param name="currenciesList">A collection of <see cref="CryptoCodeResponse"/> objects.</param>
        /// <param name="currencyCommand">The executing currency command.</param>
        /// <param name="isSubSearch">Indicates wether the <paramref name="currenciesList"/> is a subset of the initial collection.</param>
        /// <param name="username">The executing user name.</param>
        /// <param name="title">The embed's title.</param>
        /// <param name="description">The embed's description.</param>
        /// <returns>A list of embeds ready to be built.</returns>
        public List<EmbedBuilder> CreateCryptoListEmbedAsync(List<CryptoCodeResponse> currenciesList, string currencyCommand, bool isSubSearch, string username, string title = null, string description = null)
        {
            int replyTimeout = Convert.ToInt32(Configuration["interactiveMessageReplyTimeout"]);

            var emojis = Configuration.GetSection("customEmojis");
            Emoji cryptoEmoji = new(emojis["cryptoCoin"]);
            string coinsImageUrl = Configuration.GetSection("images").GetSection("crypto")["default"];
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();

            int pageCount = 0;
            int totalPages = (int)Math.Ceiling(Convert.ToDecimal(currenciesList.Count) / CURRENCIES_PER_PAGE);
            List<IEnumerable<CryptoCodeResponse>> currenciesListPages = currenciesList.ChunkBy(CURRENCIES_PER_PAGE);

            List<EmbedBuilder> embeds = new();
            foreach (IEnumerable<CryptoCodeResponse> currenciesPage in currenciesListPages)
            {
                List<IEnumerable<CryptoCodeResponse>> chunks = currenciesPage.ChunkBy(CURRENCIES_PER_PAGE / INLINE_FIELDS_PER_PAGE);
                EmbedBuilder embed = new EmbedBuilder()
                                     .WithColor(GlobalConfiguration.Colors.Crypto)
                                     .WithThumbnailUrl(coinsImageUrl)
                                     .WithTitle(title ?? "Criptomonedas disponibles")
                                     .WithDescription(description ?? $"Identificadores de criptomonedas disponibles para utilizar como parámetro del comando {Format.Code($"/{currencyCommand}")}.")
                                     .WithFooter($"Página {++pageCount} de {totalPages}");
                foreach (IEnumerable<CryptoCodeResponse> chunk in chunks)
                {
                    string currencyList = string.Join(Environment.NewLine, chunk.Select(x => $"{cryptoEmoji} {Format.Bold($"[{x.Symbol}]")} {Format.Code(x.Code)}: {Format.Italics(x.Name)}."));
                    embed = embed.AddInlineField(GlobalConfiguration.Constants.BLANK_SPACE, currencyList);
                }

                embed = embed.AddField(GlobalConfiguration.Constants.BLANK_SPACE, $"{Format.Bold(username)}, para ver una cotización, respondé a este mensaje antes de las {Format.Bold(TimeZoneInfo.ConvertTime(DateTime.Now.AddSeconds(replyTimeout), localTimeZone).ToString("HH:mm:ss"))} con el {(!isSubSearch ? $"{Format.Bold("código")} ó " : string.Empty)}{Format.Bold("identificador")} de la criptomoneda.{Environment.NewLine}Por ejemplo: {Format.Code(currenciesList.First().Code)}.");
                if (!isSubSearch)
                {
                    embed = embed.AddField(GlobalConfiguration.Constants.BLANK_SPACE, $"{Format.Bold("Tip")}: {Format.Italics($"Si ya sabés el identificador de la criptomoneda, podés indicárselo al comando directamente.{Environment.NewLine}Por ejemplo:")} {Format.Code($"/{currencyCommand} {currenciesList.First().Code}")}.");
                }
                embeds.Add(embed);
            }

            return embeds;
        }

        /// <summary>
        /// Creates a collection of <see cref="EmbedBuilder"/> objects representing a list of cryptocurrency codes.
        /// </summary>
        /// <param name="currenciesList">A collection of <see cref="CryptoCodeResponse"/> objects.</param>
        /// <param name="currencyCommand">The executing currency command.</param>
        /// <returns>A list of embeds ready to be built.</returns>
        public List<EmbedBuilder> CreateCryptoListEmbedAsync(List<CryptoCodeResponse> currenciesList, string currencyCommand)
        {
            int replyTimeout = Convert.ToInt32(Configuration["interactiveMessageReplyTimeout"]);

            var emojis = Configuration.GetSection("customEmojis");
            Emoji cryptoEmoji = new(emojis["cryptoCoin"]);
            string coinsImageUrl = Configuration.GetSection("images").GetSection("crypto")["default"];
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();

            int pageCount = 0;
            int totalPages = (int)Math.Ceiling(Convert.ToDecimal(currenciesList.Count) / CURRENCIES_PER_PAGE);
            List<IEnumerable<CryptoCodeResponse>> currenciesListPages = currenciesList.ChunkBy(CURRENCIES_PER_PAGE);

            List<EmbedBuilder> embeds = new();
            foreach (IEnumerable<CryptoCodeResponse> currenciesPage in currenciesListPages)
            {
                List<IEnumerable<CryptoCodeResponse>> chunks = currenciesPage.ChunkBy(CURRENCIES_PER_PAGE / INLINE_FIELDS_PER_PAGE);
                EmbedBuilder embed = new EmbedBuilder()
                                     .WithColor(GlobalConfiguration.Colors.Crypto)
                                     .WithThumbnailUrl(coinsImageUrl)
                                     .WithTitle("Criptomonedas disponibles")
                                     .WithDescription($"Identificadores de criptomonedas disponibles para utilizar como parámetro del comando {Format.Code($"/{currencyCommand}")}.")
                                     .WithFooter($"Página {++pageCount} de {totalPages}");
                foreach (IEnumerable<CryptoCodeResponse> chunk in chunks)
                {
                    string currencyList = string.Join(Environment.NewLine, chunk.Select(x => $"{cryptoEmoji} {Format.Bold($"[{x.Symbol}]")} {Format.Code(x.Code)}: {Format.Italics(x.Name)}."));
                    embed = embed.AddInlineField(GlobalConfiguration.Constants.BLANK_SPACE, currencyList);
                }
                embeds.Add(embed);
            }

            return embeds;
        }

        #endregion

        #endregion
    }
}
