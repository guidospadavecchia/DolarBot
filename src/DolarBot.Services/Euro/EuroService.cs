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
using EuroTypes = DolarBot.API.ApiCalls.DolarBotApi.EuroTypes;

namespace DolarBot.Services.Euro
{
    /// <summary>
    /// Contains several methods to process Euro commands.
    /// </summary>
    public class EuroService : BaseCurrencyService
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="EuroService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public EuroService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override Banks[] GetValidBanks()
        {
            return new[]
            {
                Banks.Nacion,
                Banks.Galicia,
                Banks.BBVA,
                Banks.Hipotecario,
                Banks.Chaco,
                Banks.Pampa
            };
        }

        #region API Calls

        /// <summary>
        /// Fetches a single rate by bank. Only accepts banks returned by <see cref="GetValidBanks"/>.
        /// </summary>
        /// <param name="bank">The bank who's rate is to be retrieved.</param>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroByBank(Banks bank)
        {
            return bank switch
            {
                Banks.Nacion => await GetEuroNacion().ConfigureAwait(false),
                Banks.Galicia => await GetEuroGalicia().ConfigureAwait(false),
                Banks.BBVA => await GetEuroBBVA().ConfigureAwait(false),
                Banks.Hipotecario => await GetEuroHipotecario().ConfigureAwait(false),
                Banks.Chaco => await GetEuroChaco().ConfigureAwait(false),
                Banks.Pampa => await GetEuroPampa().ConfigureAwait(false),
                _ => throw new ArgumentException("Invalid bank for currency.")
            };
        }

        /// <summary>
        /// Fetches all available euro prices.
        /// </summary>
        /// <returns>An array of <see cref="EuroResponse"/> objects.</returns>
        public async Task<EuroResponse[]> GetAllEuroRates()
        {
            return await Task.WhenAll(GetEuroNacion(),
                                      GetEuroGalicia(),
                                      GetEuroBBVA(),
                                      GetEuroHipotecario(),
                                      GetEuroChaco(),
                                      GetEuroPampa()).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for euro from bank Nacion.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroNacion()
        {
            return await Api.DolarBot.GetEuroRate(EuroTypes.Nacion).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for euro from bank Galicia.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroGalicia()
        {
            return await Api.DolarBot.GetEuroRate(EuroTypes.Galicia).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for euro from bank BBVA.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroBBVA()
        {
            return await Api.DolarBot.GetEuroRate(EuroTypes.BBVA).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for euro from bank Hipotecario.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroHipotecario()
        {
            return await Api.DolarBot.GetEuroRate(EuroTypes.Hipotecario).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for euro from bank Nuevo Banco del Chaco.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroChaco()
        {
            return await Api.DolarBot.GetEuroRate(EuroTypes.Chaco).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for euro from bank Banco de La Pampa.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroPampa()
        {
            return await Api.DolarBot.GetEuroRate(EuroTypes.Pampa).ConfigureAwait(false);
        }

        #endregion

        #region Embed

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple euro responses specifying a custom description and thumbnail URL.
        /// </summary>
        /// <param name="euroResponses">The euro responses to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="thumbnailUrl">The URL of the embed's thumbnail image.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateEuroEmbed(EuroResponse[] euroResponses, string description, string thumbnailUrl)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji euroEmoji = new Emoji(":euro:");
            Emoji clockEmoji = new Emoji("\u23F0");
            Emoji buyEmoji = new Emoji(":regional_indicator_c:");
            Emoji sellEmoji = new Emoji(":regional_indicator_v:");
            Emoji sellWithTaxesEmoji = new Emoji(":regional_indicator_a:");

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Euro)
                                                   .WithTitle("Cotizaciones del Euro")
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($" C = Compra | V = Venta | A = Venta con impuestos | {clockEmoji} = Last updated (UTC {utcOffset})");

            for (int i = 0; i < euroResponses.Length; i++)
            {
                EuroResponse response = euroResponses[i];
                string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
                string title = GetTitle(response);
                string lastUpdated = response.Fecha.ToString("dd/MM - HH:mm");
                string buyPrice = decimal.TryParse(response?.Compra, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal compra) ? compra.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellPrice = decimal.TryParse(response?.Venta, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal venta) ? venta.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellWithTaxesPrice = response?.VentaAhorro != null ? (decimal.TryParse(response?.VentaAhorro, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal ventaAhorro) ? ventaAhorro.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?") : null;

                if (buyPrice != "?" || sellPrice != "?" || (sellWithTaxesPrice != null && sellWithTaxesPrice != "?"))
                {
                    StringBuilder sbField = new StringBuilder()
                                            .AppendLine($"{euroEmoji} {blankSpace} {buyEmoji} {Format.Bold($"$ {buyPrice}")}")
                                            .AppendLine($"{euroEmoji} {blankSpace} {sellEmoji} {Format.Bold($"$ {sellPrice}")}");
                    if (sellWithTaxesPrice != null)
                    {
                        sbField.AppendLine($"{euroEmoji} {blankSpace} {sellWithTaxesEmoji} {Format.Bold($"$ {sellWithTaxesPrice}")}");
                    }
                    sbField.AppendLine($"{clockEmoji} {blankSpace} {lastUpdated}");

                    embed.AddInlineField(title, sbField.AppendLineBreak().ToString());
                }
            }

            embed = AddPlayStoreLink(embed);

            return embed;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a single euro response specifying a custom description, title and thumbnail URL.
        /// </summary>
        /// <param name="euroResponse">>The euro response to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="title">Optional. The embed's title.</param>
        /// <param name="thumbnailUrl">Optional. The embed's thumbnail URL.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public async Task<EmbedBuilder> CreateEuroEmbedAsync(EuroResponse euroResponse, string description, string title = null, string thumbnailUrl = null)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji euroEmoji = new Emoji(":euro:");
            Emoji whatsappEmoji = new Emoji(emojis["whatsapp"]);

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            string euroImageUrl = thumbnailUrl ?? Configuration.GetSection("images").GetSection("euro")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = title ?? GetTitle(euroResponse);
            string lastUpdated = euroResponse.Fecha.ToString(euroResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string buyPrice = decimal.TryParse(euroResponse?.Compra, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal compra) ? compra.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string sellPrice = decimal.TryParse(euroResponse?.Venta, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal venta) ? venta.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

            string buyInlineField = Format.Bold($"{euroEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {buyPrice}");
            string sellInlineField = Format.Bold($"{euroEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {sellPrice}");
            string shareText = $"*{(euroResponse.VentaAhorro != null ? $"Euro - {embedTitle}" : embedTitle)}*{Environment.NewLine}{Environment.NewLine}Compra: \t\t$ *{buyPrice}*{Environment.NewLine}Venta: \t\t$ *{sellPrice}*";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Euro)
                                                   .WithTitle(embedTitle)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(euroImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} (UTC {utcOffset})", footerImageUrl)
                                                   .AddInlineField("Compra", buyInlineField)
                                                   .AddInlineField("Venta", sellInlineField);
            if (euroResponse.VentaAhorro != null)
            {
                string sellPriceWithTaxes = decimal.TryParse(euroResponse?.VentaAhorro, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal ventaAhorro) ? ventaAhorro.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellWithTaxesInlineField = Format.Bold($"{euroEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {sellPriceWithTaxes}");
                embed.AddInlineField("Venta con Impuestos", sellWithTaxesInlineField);
                shareText += $"{Environment.NewLine}Venta c/imp: \t$ *{sellPriceWithTaxes}*";
            }

            shareText += $"{Environment.NewLine}Hora: \t\t{lastUpdated} (UTC {utcOffset})";
            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText, Api.Cuttly.ShortenUrl);

            embed = AddPlayStoreLink(embed);

            return embed;
        }

        /// <summary>
        /// Returns the title depending on the response type.
        /// </summary>
        /// <param name="euroResponse">The euro response.</param>
        /// <returns>The corresponding title.</returns>
        private string GetTitle(EuroResponse euroResponse)
        {
            return euroResponse.Type switch
            {
                EuroTypes.Nacion => Banks.Nacion.GetDescription(),
                EuroTypes.Galicia => Banks.Galicia.GetDescription(),
                EuroTypes.BBVA => Banks.BBVA.GetDescription(),
                EuroTypes.Hipotecario => Banks.Hipotecario.GetDescription(),
                EuroTypes.Chaco => Banks.Chaco.GetDescription(),
                EuroTypes.Pampa => Banks.Pampa.GetDescription(),
                _ => throw new ArgumentException($"Unable to get title from '{euroResponse.Type}'.")
            };
        }

        #endregion

        #endregion
    }
}
