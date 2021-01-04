using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Banking;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealTypes = DolarBot.API.ApiCalls.DolarBotApi.RealTypes;

namespace DolarBot.Services.Real
{
    /// <summary>
    /// Contains several methods to process Real (Brazil) commands.
    /// </summary>
    public class RealService : BaseCurrencyService
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="RealService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public RealService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override Banks[] GetValidBanks()
        {
            return new[]
            {
                Banks.Nacion,
                Banks.BBVA,
                Banks.Chaco,
            };
        }

        #region API Calls

        /// <summary>
        /// Fetches a single rate by bank. Only accepts banks returned by <see cref="GetValidBanks"/>.
        /// </summary>
        /// <param name="bank">The bank who's rate is to be retrieved.</param>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealByBank(Banks bank)
        {
            return bank switch
            {
                Banks.Nacion => await GetRealNacion().ConfigureAwait(false),
                Banks.BBVA => await GetRealBBVA().ConfigureAwait(false),
                Banks.Chaco => await GetRealChaco().ConfigureAwait(false),
                _ => throw new ArgumentException("Invalid bank for currency.")
            };
        }

        /// <summary>
        /// Fetches a single rate by bank with taxes applied. Only accepts banks returned by <see cref="GetValidBanks"/>.
        /// </summary>
        /// <param name="bank">The bank who's rate is to be retrieved.</param>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealAhorroByBank(Banks bank)
        {
            RealResponse realResponse = await GetRealByBank(bank).ConfigureAwait(false);
            return (RealResponse)ApplyTaxes(realResponse);
        }

        /// <summary>
        /// Fetches all available Real prices.
        /// </summary>
        /// <returns>An array of <see cref="RealResponse"/> objects.</returns>
        public async Task<RealResponse[]> GetAllRealRates()
        {
            return await Task.WhenAll(GetRealNacion(),
                                      GetRealBBVA(),
                                      GetRealChaco()).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches all available Real Ahorro prices.
        /// </summary>
        /// <returns>An array of <see cref="RealResponse"/> objects.</returns>
        public async Task<RealResponse[]> GetAllRealAhorroRates()
        {
            RealResponse[] realResponses = await GetAllRealRates().ConfigureAwait(false);
            return realResponses.Select(x => (RealResponse)ApplyTaxes(x)).ToArray();
        }

        /// <summary>
        /// Fetches the price for Real from bank Nacion.
        /// </summary>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealNacion()
        {
            return await Api.DolarBot.GetRealRate(RealTypes.Nacion).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for Real from bank BBVA.
        /// </summary>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealBBVA()
        {
            return await Api.DolarBot.GetRealRate(RealTypes.BBVA).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for Real from Nuevo Banco del Chaco.
        /// </summary>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealChaco()
        {
            return await Api.DolarBot.GetRealRate(RealTypes.Chaco).ConfigureAwait(false);
        }

        #endregion

        #region Embed

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple Real responses specifying a custom description and thumbnail URL.
        /// </summary>
        /// <param name="realResponses">The Real responses to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="thumbnailUrl">The URL of the embed's thumbnail image.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateRealEmbed(RealResponse[] realResponses, string description, string thumbnailUrl)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji realEmoji = new Emoji(emojis["real"]);
            Emoji clockEmoji = new Emoji("\u23F0");
            Emoji buyEmoji = new Emoji(emojis["buyYellow"]);
            Emoji sellEmoji = new Emoji(emojis["sellYellow"]);

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Real)
                                                   .WithTitle("Cotizaciones del Real")
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($" C = Compra | V = Venta | {clockEmoji} = Última actualización ({localTimeZone.StandardName})");

            for (int i = 0; i < realResponses.Length; i++)
            {
                RealResponse response = realResponses[i];
                string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
                string title = GetTitle(response);
                string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(response.Fecha, localTimeZone).ToString("dd/MM - HH:mm");
                string buyPrice = decimal.TryParse(response?.Compra, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal compra) ? compra.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellPrice = decimal.TryParse(response?.Venta, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal venta) ? venta.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

                if (buyPrice != "?" || sellPrice != "?")
                {
                    StringBuilder sbField = new StringBuilder()
                                            .AppendLine($"{realEmoji} {blankSpace} {buyEmoji} {Format.Bold($"$ {buyPrice}")}")
                                            .AppendLine($"{realEmoji} {blankSpace} {sellEmoji} {Format.Bold($"$ {sellPrice}")}")
                                            .AppendLine($"{clockEmoji} {blankSpace} {lastUpdated}");
                    embed.AddInlineField(title, sbField.AppendLineBreak().ToString());
                }
            }

            return embed;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a single Real response specifying a custom description, title and thumbnail URL.
        /// </summary>
        /// <param name="realResponse">>The Real response to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="title">Optional. The embed's title.</param>
        /// <param name="thumbnailUrl">Optional. The embed's thumbnail URL.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateRealEmbed(RealResponse realResponse, string description, string title = null, string thumbnailUrl = null)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji realEmoji = new Emoji(emojis["real"]);
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            string realImageUrl = thumbnailUrl ?? Configuration.GetSection("images").GetSection("real")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = title ?? GetTitle(realResponse);
            string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(realResponse.Fecha, localTimeZone).ToString(realResponse.Fecha.Date == DateTime.UtcNow.Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string buyPrice = decimal.TryParse(realResponse?.Compra, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal compra) ? compra.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string sellPrice = decimal.TryParse(realResponse?.Venta, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal venta) ? venta.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Real)
                                                   .WithTitle(embedTitle)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(realImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} ({localTimeZone.StandardName})", footerImageUrl)
                                                   .AddInlineField("Compra", Format.Bold($"{realEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {buyPrice}"))
                                                   .AddInlineField("Venta", Format.Bold($"{realEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {sellPrice}".AppendLineBreak()));
            return embed;
        }

        /// <summary>
        /// Returns the title depending on the response type.
        /// </summary>
        /// <param name="realResponse">The Real response.</param>
        /// <returns>The corresponding title.</returns>
        private string GetTitle(RealResponse realResponse)
        {
            return realResponse.Type switch
            {
                RealTypes.Nacion => Banks.Nacion.GetDescription(),
                RealTypes.BBVA => Banks.BBVA.GetDescription(),
                RealTypes.Chaco => Banks.Chaco.GetDescription(),
                _ => throw new ArgumentException($"Unable to get title from '{realResponse.Type}'.")
            };
        }
        
        #endregion

        #endregion
    }
}
