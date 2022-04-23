using Discord;
using DolarBot.API;
using DolarBot.API.Enums;
using DolarBot.API.Models;
using DolarBot.API.Services.DolarBotApi;
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

namespace DolarBot.Services.Euro
{
    /// <summary>
    /// Contains several methods to process Euro commands.
    /// </summary>
    public class EuroService : BaseCurrencyService<EuroResponse>
    {
        #region Constants
        private const string EURO_OFICIAL_TITLE = "Euro Oficial";
        private const string EURO_AHORRO_TITLE = "Euro Ahorro";
        private const string EURO_BLUE_TITLE = "Euro Blue";
        #endregion

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
                Banks.Bancos,
                Banks.Nacion,
                Banks.Galicia,
                Banks.BBVA,
                Banks.Hipotecario,
                Banks.Chaco,
                Banks.Pampa,
                Banks.Piano,
                Banks.Santander,
                Banks.Ciudad,
                Banks.Supervielle,
                Banks.Patagonia,
                Banks.Comafi,
                Banks.Reba,
                Banks.Roela
            };
        }

        /// <summary>
        /// Converts a <see cref="Banks"/> object to its <see cref="EuroTypes"/> equivalent.
        /// </summary>
        /// <param name="bank">The value to convert.</param>
        /// <returns>The converted value as <see cref="EuroTypes"/>.</returns>
        private static EuroEndpoints ConvertToEuroType(Banks bank)
        {
            return bank switch
            {
                Banks.Nacion => EuroEndpoints.Nacion,
                Banks.Galicia => EuroEndpoints.Galicia,
                Banks.BBVA => EuroEndpoints.BBVA,
                Banks.Hipotecario => EuroEndpoints.Hipotecario,
                Banks.Chaco => EuroEndpoints.Chaco,
                Banks.Pampa => EuroEndpoints.Pampa,
                Banks.Piano => EuroEndpoints.Piano,
                Banks.Santander => EuroEndpoints.Santander,
                Banks.Ciudad => EuroEndpoints.Ciudad,
                Banks.Supervielle => EuroEndpoints.Supervielle,
                Banks.Patagonia => EuroEndpoints.Patagonia,
                Banks.Comafi => EuroEndpoints.Comafi,
                Banks.Reba => EuroEndpoints.Reba,
                Banks.Roela => EuroEndpoints.Roela,
                _ => throw new ArgumentException("Unsupported Euro type")
            };
        }

        #region API Calls

        /// <inheritdoc />
        public override async Task<EuroResponse> GetByBank(Banks bank)
        {
            EuroEndpoints euroType = ConvertToEuroType(bank);
            return await Api.DolarBot.GetEuroRate(euroType);
        }

        /// <inheritdoc />
        public override async Task<EuroResponse[]> GetAllStandardRates()
        {
            return await Task.WhenAll(GetEuroOficial(), GetEuroAhorro(), GetEuroBlue());
        }

        /// <inheritdoc />
        public override async Task<EuroResponse[]> GetAllBankRates()
        {
            List<Banks> banks = GetValidBanks().Where(b => b != Banks.Bancos).ToList();
            Task<EuroResponse>[] tasks = new Task<EuroResponse>[banks.Count];
            for (int i = 0; i < banks.Count; i++)
            {
                EuroEndpoints euroType = ConvertToEuroType(banks[i]);
                tasks[i] = Api.DolarBot.GetEuroRate(euroType);
            }

            return await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Fetches the price for official Euro.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroOficial()
        {
            return await Api.DolarBot.GetEuroRate(EuroEndpoints.Oficial);
        }

        /// <summary>
        /// Fetches the price for Euro Ahorro.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroAhorro()
        {
            return await Api.DolarBot.GetEuroRate(EuroEndpoints.Ahorro);
        }

        /// <summary>
        /// Fetches the price for Euro Blue.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroBlue()
        {
            return await Api.DolarBot.GetEuroRate(EuroEndpoints.Blue);
        }

        #endregion

        #region Embed       

        /// <inheritdoc />
        public override EmbedBuilder CreateEmbed(EuroResponse[] euroResponses)
        {
            string euroImageUrl = Configuration.GetSection("images").GetSection("euro")["64"];
            return CreateEmbed(euroResponses, $"Cotizaciones disponibles del {Format.Bold("Euro")} expresadas en {Format.Bold("pesos argentinos")}.", euroImageUrl);
        }

        /// <inheritdoc />
        public override EmbedBuilder CreateEmbed(EuroResponse[] euroResponses, string description, string thumbnailUrl)
        {
            Emoji euroEmoji = new(":euro:");
            Emoji clockEmoji = new("\u23F0");
            Emoji buyEmoji = new(":regional_indicator_c:");
            Emoji sellEmoji = new(":regional_indicator_v:");
            Emoji sellWithTaxesEmoji = new(":regional_indicator_a:");

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
                string title = GetTitle(response.Type);
                string lastUpdated = response.Fecha.ToString("dd/MM - HH:mm");
                string buyPrice = decimal.TryParse(response?.Compra, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal compra) ? compra.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellPrice = decimal.TryParse(response?.Venta, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal venta) ? venta.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellWithTaxesPrice = response?.VentaAhorro != null ? (decimal.TryParse(response?.VentaAhorro, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal ventaAhorro) ? ventaAhorro.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?") : null;

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

        /// <inheritdoc />
        public override async Task<EmbedBuilder> CreateEmbedAsync(EuroResponse euroResponse, string description, string title = null, string thumbnailUrl = null)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji euroEmoji = new(":euro:");
            Emoji whatsappEmoji = new(emojis["whatsapp"]);

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            string euroImageUrl = thumbnailUrl ?? Configuration.GetSection("images").GetSection("euro")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = title ?? GetTitle(euroResponse.Type);
            string lastUpdated = euroResponse.Fecha.ToString(euroResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string buyPrice = decimal.TryParse(euroResponse?.Compra, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal compra) ? compra.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string sellPrice = decimal.TryParse(euroResponse?.Venta, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal venta) ? venta.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

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
                string sellPriceWithTaxes = decimal.TryParse(euroResponse?.VentaAhorro, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal ventaAhorro) ? ventaAhorro.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
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
        /// <param name="euroType">The euro type.</param>
        /// <returns>The corresponding title.</returns>
        private static string GetTitle(EuroEndpoints euroType)
        {
            return euroType switch
            {
                EuroEndpoints.Oficial => EURO_OFICIAL_TITLE,
                EuroEndpoints.Ahorro => EURO_AHORRO_TITLE,
                EuroEndpoints.Blue => EURO_BLUE_TITLE,
                _ => Enum.TryParse(euroType.ToString(), out Banks bank) ? bank.GetDescription() : throw new ArgumentException($"Unable to get title from '{euroType}'."),
            };
        }

        #endregion

        #endregion
    }
}
