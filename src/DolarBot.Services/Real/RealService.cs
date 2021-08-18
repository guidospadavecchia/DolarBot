using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Banking;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
    public class RealService : BaseCurrencyService<RealResponse>
    {
        #region Constants
        private const string REAL_OFICIAL_TITLE = "Real Oficial";
        private const string REAL_AHORRO_TITLE = "Real Ahorro";
        private const string REAL_BLUE_TITLE = "Real Blue";
        #endregion

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
                Banks.Piano,
                Banks.Ciudad,
                Banks.Supervielle
            };
        }

        /// <summary>
        /// Converts a <see cref="Banks"/> object to its <see cref="RealTypes"/> equivalent.
        /// </summary>
        /// <param name="bank">The value to convert.</param>
        /// <returns>The converted value as <see cref="RealTypes"/>.</returns>
        private RealTypes ConvertToRealType(Banks bank)
        {
            return bank switch
            {
                Banks.Nacion => RealTypes.Nacion,
                Banks.BBVA => RealTypes.BBVA,
                Banks.Chaco => RealTypes.Chaco,
                Banks.Piano => RealTypes.Piano,
                Banks.Ciudad => RealTypes.Ciudad,
                Banks.Supervielle => RealTypes.Supervielle,
                _ => throw new ArgumentException("Unsupported Real type")
            };
        }

        #region API Calls

        /// <inheritdoc />
        public override async Task<RealResponse> GetByBank(Banks bank)
        {
            RealTypes realType = ConvertToRealType(bank);
            return await Api.DolarBot.GetRealRate(realType).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override async Task<RealResponse[]> GetAllBankRates()
        {
            List<Banks> banks = GetValidBanks().ToList();
            Task<RealResponse>[] tasks = new Task<RealResponse>[banks.Count];
            for (int i = 0; i < banks.Count; i++)
            {
                RealTypes realType = ConvertToRealType(banks[i]);
                tasks[i] = Api.DolarBot.GetRealRate(realType);
            }

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override async Task<RealResponse[]> GetAllStandardRates()
        {
            return await Task.WhenAll(GetRealOficial(), GetRealAhorro(), GetRealBlue()).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for official Real.
        /// </summary>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealOficial()
        {
            return await Api.DolarBot.GetRealRate(RealTypes.Oficial).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for Real Ahorro.
        /// </summary>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealAhorro()
        {
            return await Api.DolarBot.GetRealRate(RealTypes.Ahorro).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for Real Blue.
        /// </summary>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealBlue()
        {
            return await Api.DolarBot.GetRealRate(RealTypes.Blue).ConfigureAwait(false);
        }

        #endregion

        #region Embed

        /// <inheritdoc />
        public override EmbedBuilder CreateEmbed(RealResponse[] realResponses)
        {
            string realImageUrl = Configuration.GetSection("images").GetSection("real")["64"];
            return CreateEmbed(realResponses, $"Cotizaciones disponibles del {Format.Bold("Real")} expresadas en {Format.Bold("pesos argentinos")}.", realImageUrl);
        }

        /// <inheritdoc />
        public override EmbedBuilder CreateEmbed(RealResponse[] realResponses, string description, string thumbnailUrl)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji realEmoji = new Emoji(emojis["real"]);
            Emoji clockEmoji = new Emoji("\u23F0");
            Emoji buyEmoji = new Emoji(emojis["buyYellow"]);
            Emoji sellEmoji = new Emoji(emojis["sellYellow"]);
            Emoji sellWithTaxesEmoji = new Emoji(emojis["sellWithTaxesYellow"]);

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Real)
                                                   .WithTitle("Cotizaciones del Real")
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($" C = Compra | V = Venta | A = Venta con impuestos | {clockEmoji} = Last updated (UTC {utcOffset})");

            for (int i = 0; i < realResponses.Length; i++)
            {
                RealResponse response = realResponses[i];
                string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
                string title = GetTitle(response.Type);
                string lastUpdated = response.Fecha.ToString("dd/MM - HH:mm");
                string buyPrice = decimal.TryParse(response?.Compra, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal compra) ? compra.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellPrice = decimal.TryParse(response?.Venta, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal venta) ? venta.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellWithTaxesPrice = response?.VentaAhorro != null ? (decimal.TryParse(response?.VentaAhorro, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal ventaAhorro) ? ventaAhorro.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?") : null;

                if (buyPrice != "?" || sellPrice != "?" || (sellWithTaxesPrice != null && sellWithTaxesPrice != "?"))
                {
                    StringBuilder sbField = new StringBuilder()
                                            .AppendLine($"{realEmoji} {blankSpace} {buyEmoji} {Format.Bold($"$ {buyPrice}")}")
                                            .AppendLine($"{realEmoji} {blankSpace} {sellEmoji} {Format.Bold($"$ {sellPrice}")}");
                    if (sellWithTaxesPrice != null)
                    {
                        sbField.AppendLine($"{realEmoji} {blankSpace} {sellWithTaxesEmoji} {Format.Bold($"$ {sellWithTaxesPrice}")}");
                    }
                    sbField.AppendLine($"{clockEmoji} {blankSpace} {lastUpdated}");

                    embed.AddInlineField(title, sbField.AppendLineBreak().ToString());
                }
            }

            embed = AddPlayStoreLink(embed);

            return embed;
        }

        /// <inheritdoc />
        public override async Task<EmbedBuilder> CreateEmbedAsync(RealResponse realResponse, string description, string title = null, string thumbnailUrl = null)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji realEmoji = new Emoji(emojis["real"]);
            Emoji whatsappEmoji = new Emoji(emojis["whatsapp"]);

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            string realImageUrl = thumbnailUrl ?? Configuration.GetSection("images").GetSection("real")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = title ?? GetTitle(realResponse.Type);
            string lastUpdated = realResponse.Fecha.ToString(realResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string buyPrice = decimal.TryParse(realResponse?.Compra, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal compra) ? compra.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string sellPrice = decimal.TryParse(realResponse?.Venta, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal venta) ? venta.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

            string buyInlineField = Format.Bold($"{realEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {buyPrice}");
            string sellInlineField = Format.Bold($"{realEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {sellPrice}");
            string shareText = $"*{(realResponse.VentaAhorro != null ? $"Real - {embedTitle}" : embedTitle)}*{Environment.NewLine}{Environment.NewLine}Compra: \t\t$ *{buyPrice}*{Environment.NewLine}Venta: \t\t$ *{sellPrice}*";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Real)
                                                   .WithTitle(embedTitle)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(realImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} (UTC {utcOffset})", footerImageUrl)
                                                   .AddInlineField("Compra", buyInlineField)
                                                   .AddInlineField("Venta", sellInlineField);
            if (realResponse.VentaAhorro != null)
            {
                string sellPriceWithTaxes = decimal.TryParse(realResponse?.VentaAhorro, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal ventaAhorro) ? ventaAhorro.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellWithTaxesInlineField = Format.Bold($"{realEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {sellPriceWithTaxes}");
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
        /// <param name="realType">The real type.</param>
        /// <returns>The corresponding title.</returns>
        private string GetTitle(RealTypes realType)
        {
            return realType switch
            {
                RealTypes.Oficial => REAL_OFICIAL_TITLE,
                RealTypes.Ahorro => REAL_AHORRO_TITLE,
                RealTypes.Blue => REAL_BLUE_TITLE,
                _ => Enum.TryParse(realType.ToString(), out Banks bank) ? bank.GetDescription() : throw new ArgumentException($"Unable to get title from '{realType}'."),
            };
        }

        #endregion

        #endregion
    }
}
