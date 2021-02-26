using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Threading.Tasks;
using CryptoCurrencies = DolarBot.API.ApiCalls.DolarBotApi.CryptoCurrencies;

namespace DolarBot.Services.Crypto
{
    /// <summary>
    /// Contains several methods to process Euro commands.
    /// </summary>
    public class CryptoService : BaseService
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="CryptoService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public CryptoService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        #region API Calls

        /// <summary>
        /// Fetches the price for Bitcoin (BTC).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetBitcoinRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Bitcoin).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for Bitcoin Cash (BCH).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetBitcoinCashRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.BitcoinCash).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for Ethereum (ETH).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetEthereumRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Ethereum).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for Monero (XMR).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetMoneroRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Monero).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for Litecoin (LTC).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetLitecoinRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Litecoin).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for Ripple (XRP).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetRippleRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Ripple).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for Dash (DASH).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetDashRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Dash).ConfigureAwait(false);
        }

        #endregion

        #region Embed

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a single crypto response.
        /// </summary>
        /// <param name="cryptoResponse">The crypto response object.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateCryptoEmbed(CryptoResponse cryptoResponse)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji cryptoEmoji = new Emoji(emojis["cryptoCoin"]);
            Emoji argentinaEmoji = new Emoji(":flag_ar:");
            Emoji usaEmoji = new Emoji(":flag_us:");
            Emoji whatsappEmoji = new Emoji(emojis["whatsapp"]);
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;
            string thumbnailUrl = Configuration.GetSection("images").GetSection("crypto")[cryptoResponse.Currency.ToString().ToLower()];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string lastUpdated = cryptoResponse.Fecha.ToString(cryptoResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string arsPrice = decimal.TryParse(cryptoResponse?.ARS, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal ars) ? ars.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string arsPriceWithTaxes = decimal.TryParse(cryptoResponse?.ARSTaxed, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal arsTaxed) ? arsTaxed.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string usdPrice = decimal.TryParse(cryptoResponse?.USD, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal usd) ? usd.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string shareText = $"*{cryptoResponse.Currency.ToString().Capitalize()} ({cryptoResponse.CurrencyCode})*{Environment.NewLine}{Environment.NewLine}Dólares: \t\tUS$ *{usdPrice}*{Environment.NewLine}Pesos: \t\t$ *{arsPrice}*{Environment.NewLine}Pesos c/Imp: \t$ *{arsPriceWithTaxes}*{Environment.NewLine}Hora: \t\t{lastUpdated} (UTC {utcOffset})";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GetColor(cryptoResponse.Currency))
                                                   .WithTitle($"{cryptoResponse.Currency.ToString().Capitalize()} ({cryptoResponse.CurrencyCode})")
                                                   .WithDescription($"Cotización de {Format.Bold(cryptoResponse.Currency.ToString().Capitalize())} ({Format.Bold(cryptoResponse.CurrencyCode)}) expresada en {Format.Bold("pesos argentinos")} y {Format.Bold("dólares estadounidenses")}.".AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} (UTC {utcOffset})", footerImageUrl)
                                                   .AddField($"{usaEmoji} USD", $"{cryptoEmoji} {Format.Bold($"1 {cryptoResponse.CurrencyCode}")} = {Format.Bold($"US$ {usdPrice}")}".AppendLineBreak())
                                                   .AddInlineField($"{argentinaEmoji} ARS", $"{cryptoEmoji} {Format.Bold($"1 {cryptoResponse.CurrencyCode}")} = {Format.Bold($"$ {arsPrice}")} {GlobalConfiguration.Constants.BLANK_SPACE}")
                                                   .AddInlineField($"{argentinaEmoji} ARS con Impuestos", $"{cryptoEmoji} {Format.Bold($"1 {cryptoResponse.CurrencyCode}")} = {Format.Bold($"$ {arsPriceWithTaxes}")} {GlobalConfiguration.Constants.BLANK_SPACE}")
                                                   .AddFieldWhatsAppShare(whatsappEmoji, shareText);
            return embed;
        }

        /// <summary>
        /// Returns the corresponding color associated to the <paramref name="cryptoCurrency"/> parameter.
        /// </summary>
        /// <param name="cryptoCurrency">The cryptocurrency.</param>
        /// <returns>The corresponding color.</returns>
        private Color GetColor(CryptoCurrencies cryptoCurrency)
        {
            return cryptoCurrency switch
            {
                CryptoCurrencies.Bitcoin => GlobalConfiguration.Colors.Bitcoin,
                CryptoCurrencies.BitcoinCash => GlobalConfiguration.Colors.BitcoinCash,
                CryptoCurrencies.Dash => GlobalConfiguration.Colors.Dash,
                CryptoCurrencies.Ethereum => GlobalConfiguration.Colors.Ethereum,
                CryptoCurrencies.Litecoin => GlobalConfiguration.Colors.Litecoin,
                CryptoCurrencies.Monero => GlobalConfiguration.Colors.Monero,
                CryptoCurrencies.Ripple => GlobalConfiguration.Colors.Ripple,
                _ => throw new NotImplementedException()
            };
        }

        #endregion

        #endregion
    }
}
