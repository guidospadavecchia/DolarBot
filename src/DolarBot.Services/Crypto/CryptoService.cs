using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Fetches a single cryptocurrency rate.
        /// </summary>
        /// <param name="cryptoCurrencyCode">The cryptocurrency code as a standarized <see cref="CryptoCurrencies"/> value.</param>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetCryptoRateByCode(CryptoCurrencies cryptoCurrencyCode)
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(cryptoCurrencyCode);
        }

        /// <summary>
        /// Fetches the price for Binance Coin (BNB).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetBinanceCoinRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.BinanceCoin);
        }

        /// <summary>
        /// Fetches the price for Bitcoin (BTC).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetBitcoinRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Bitcoin);
        }

        /// <summary>
        /// Fetches the price for Bitcoin Cash (BCH).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetBitcoinCashRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.BitcoinCash);
        }

        /// <summary>
        /// Fetches the price for Cardano (ADA).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetCardanoRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Cardano);
        }

        /// <summary>
        /// Fetches the price for Chainlink (LINK).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetChainlinkRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Chainlink);
        }

        /// <summary>
        /// Fetches the price for DAI (DAI).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetDaiRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.DAI);
        }

        /// <summary>
        /// Fetches the price for Dash (DASH).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetDashRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Dash);
        }

        /// <summary>
        /// Fetches the price for Dogecoin (DOGE).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetDogecoinRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.DogeCoin);
        }

        /// <summary>
        /// Fetches the price for Ethereum (ETH).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetEthereumRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Ethereum);
        }

        /// <summary>
        /// Fetches the price for Monero (XMR).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetMoneroRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Monero);
        }

        /// <summary>
        /// Fetches the price for Litecoin (LTC).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetLitecoinRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Litecoin);
        }

        /// <summary>
        /// Fetches the price for Polkadot (DOT).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetPolkadotRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Polkadot);
        }

        /// <summary>
        /// Fetches the price for Ripple (XRP).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetRippleRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Ripple);
        }

        /// <summary>
        /// Fetches the price for Stellar (XLM).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetStellarRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Stellar);
        }

        /// <summary>
        /// Fetches the price for Tether (USDT).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetTetherRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Tether);
        }

        /// <summary>
        /// Fetches the price for Theta (THETA).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetThetaRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Theta);
        }

        /// <summary>
        /// Fetches the price for Uniswap (UNI).
        /// </summary>
        /// <returns>A single <see cref="CryptoResponse"/>.</returns>
        public async Task<CryptoResponse> GetUniswapRate()
        {
            return await Api.DolarBot.GetCryptoCurrencyRate(CryptoCurrencies.Uniswap);
        }

        #endregion

        #region Embed

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a single crypto response.
        /// </summary>
        /// <param name="cryptoResponse">The crypto response object.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public async Task<EmbedBuilder> CreateCryptoEmbedAsync(CryptoResponse cryptoResponse, string cryptoCurrencyName = null)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji cryptoEmoji = new Emoji(emojis["cryptoCoin"]);
            Emoji argentinaEmoji = new Emoji(":flag_ar:");
            Emoji usaEmoji = new Emoji(":flag_us:");
            Emoji whatsappEmoji = new Emoji(emojis["whatsapp"]);
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;
            string thumbnailUrl = Configuration.GetSection("images").GetSection("crypto")[cryptoResponse.Currency?.ToString().ToLower() ?? "default"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string cryptoCode = cryptoResponse.Code.Length > 10 ? $"{cryptoResponse.Code.Substring(0, 7)}..." : cryptoResponse.Code;
            string lastUpdated = cryptoResponse.Fecha.ToString(cryptoResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string arsPrice = decimal.TryParse(cryptoResponse?.ARS, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal ars) ? ars.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string arsPriceWithTaxes = decimal.TryParse(cryptoResponse?.ARSTaxed, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal arsTaxed) ? arsTaxed.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string usdPrice = decimal.TryParse(cryptoResponse?.USD, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal usd) ? usd.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string shareText = $"*{cryptoCurrencyName ?? cryptoResponse.Currency.ToString().Capitalize()} ({cryptoCode})*{Environment.NewLine}{Environment.NewLine}Dólares: \t\tUS$ *{usdPrice}*{Environment.NewLine}Pesos: \t\t$ *{arsPrice}*{Environment.NewLine}Pesos c/Imp: \t$ *{arsPriceWithTaxes}*{Environment.NewLine}Hora: \t\t{lastUpdated} (UTC {utcOffset})";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GetColor(cryptoResponse.Currency))
                                                   .WithTitle($"{cryptoCurrencyName ?? cryptoResponse.Currency.ToString().Capitalize()} ({cryptoCode})")
                                                   .WithDescription($"Cotización de {Format.Bold(cryptoCurrencyName ?? cryptoResponse.Currency.ToString().Capitalize())} ({Format.Bold(cryptoResponse.Code)}) expresada en {Format.Bold("pesos argentinos")} y {Format.Bold("dólares estadounidenses")}.".AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} (UTC {utcOffset})", footerImageUrl)
                                                   .AddField($"{usaEmoji} USD", $"{cryptoEmoji} {Format.Bold($"1 {cryptoCode}")} = {Format.Bold($"US$ {usdPrice}")}".AppendLineBreak())
                                                   .AddInlineField($"{argentinaEmoji} ARS", $"{cryptoEmoji} {Format.Bold($"1 {cryptoCode}")} = {Format.Bold($"$ {arsPrice}")} {GlobalConfiguration.Constants.BLANK_SPACE}")
                                                   .AddInlineField($"{argentinaEmoji} ARS con Impuestos", $"{cryptoEmoji} {Format.Bold($"1 {cryptoCode}")} = {Format.Bold($"$ {arsPriceWithTaxes}")} {GlobalConfiguration.Constants.BLANK_SPACE}");

            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText, Api.Cuttly.ShortenUrl);
            embed = AddPlayStoreLink(embed);

            return embed;
        }

        /// <summary>
        /// Returns the corresponding color associated to the <paramref name="cryptoCurrency"/> parameter.
        /// </summary>
        /// <param name="cryptoCurrency">The cryptocurrency.</param>
        /// <returns>The corresponding color.</returns>
        private Color GetColor(CryptoCurrencies? cryptoCurrency)
        {
            return cryptoCurrency switch
            {
                CryptoCurrencies.BinanceCoin => GlobalConfiguration.Colors.BinanceCoin,
                CryptoCurrencies.Bitcoin => GlobalConfiguration.Colors.Bitcoin,
                CryptoCurrencies.BitcoinCash => GlobalConfiguration.Colors.BitcoinCash,
                CryptoCurrencies.Cardano => GlobalConfiguration.Colors.Cardano,
                CryptoCurrencies.Chainlink => GlobalConfiguration.Colors.Chainlink,
                CryptoCurrencies.DAI => GlobalConfiguration.Colors.DAI,
                CryptoCurrencies.Dash => GlobalConfiguration.Colors.Dash,
                CryptoCurrencies.DogeCoin => GlobalConfiguration.Colors.Dogecoin,
                CryptoCurrencies.Ethereum => GlobalConfiguration.Colors.Ethereum,
                CryptoCurrencies.Litecoin => GlobalConfiguration.Colors.Litecoin,
                CryptoCurrencies.Monero => GlobalConfiguration.Colors.Monero,
                CryptoCurrencies.Polkadot => GlobalConfiguration.Colors.Polkadot,
                CryptoCurrencies.Ripple => GlobalConfiguration.Colors.Ripple,
                CryptoCurrencies.Stellar => GlobalConfiguration.Colors.Stellar,
                CryptoCurrencies.Tether => GlobalConfiguration.Colors.Tether,
                CryptoCurrencies.Theta => GlobalConfiguration.Colors.Theta,
                CryptoCurrencies.Uniswap => GlobalConfiguration.Colors.Uniswap,
                _ => GlobalConfiguration.Colors.Crypto
            };
        }

        #endregion

        #endregion
    }
}
