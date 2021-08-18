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
using DollarTypes = DolarBot.API.ApiCalls.DolarBotApi.DollarTypes;

namespace DolarBot.Services.Dolar
{
    /// <summary>
    /// Contains several methods to process dollar commands.
    /// </summary>
    public class DolarService : BaseCurrencyService<DollarResponse>
    {
        #region Constants
        private const string DOLAR_OFICIAL_TITLE = "Dólar Oficial";
        private const string DOLAR_AHORRO_TITLE = "Dólar Ahorro";
        private const string DOLAR_BLUE_TITLE = "Dólar Blue";
        private const string DOLAR_BOLSA_TITLE = "Dólar Bolsa (MEP)";
        private const string DOLAR_PROMEDIO_TITLE = "Dólar Promedio";
        private const string DOLAR_CCL_TITLE = "Contado con Liqui";
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="DolarService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public DolarService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }
        #endregion

        #region Methods

        /// <inheritdoc />
        public override Banks[] GetValidBanks()
        {
            return Enum.GetValues(typeof(Banks)).Cast<Banks>().ToArray();
        }

        /// <summary>
        /// Converts a <see cref="Banks"/> object to its <see cref="DollarTypes"/> equivalent.
        /// </summary>
        /// <param name="bank">The value to convert.</param>
        /// <returns>The converted value as <see cref="DollarTypes"/>.</returns>
        private DollarTypes ConvertToDollarType(Banks bank)
        {
            return bank switch
            {
                Banks.Nacion => DollarTypes.Nacion,
                Banks.BBVA => DollarTypes.BBVA,
                Banks.Piano => DollarTypes.Piano,
                Banks.Hipotecario => DollarTypes.Hipotecario,
                Banks.Galicia => DollarTypes.Galicia,
                Banks.Santander => DollarTypes.Santander,
                Banks.Ciudad => DollarTypes.Ciudad,
                Banks.Supervielle => DollarTypes.Supervielle,
                Banks.Patagonia => DollarTypes.Patagonia,
                Banks.Comafi => DollarTypes.Comafi,
                Banks.Bancor => DollarTypes.Bancor,
                Banks.Chaco => DollarTypes.Chaco,
                Banks.Pampa => DollarTypes.Pampa,
                Banks.Provincia => DollarTypes.Provincia,
                Banks.ICBC => DollarTypes.ICBC,
                Banks.Reba => DollarTypes.Reba,
                Banks.Roela => DollarTypes.Roela,
                _ => throw new ArgumentException("Unsupported Dollar type")
            };
        }

        #region API Calls

        /// <inheritdoc />
        public override async Task<DollarResponse[]> GetAllStandardRates()
        {
            return await Task.WhenAll(GetDollarOficial(),
                                      GetDollarAhorro(),
                                      GetDollarBlue(),
                                      GetDollarBolsa(),
                                      GetDollarPromedio(),
                                      GetDollarContadoConLiqui()).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override async Task<DollarResponse[]> GetAllBankRates()
        {
            List<Banks> banks = GetValidBanks().Where(b => b != Banks.Bancos).ToList();
            Task<DollarResponse>[] tasks = new Task<DollarResponse>[banks.Count];
            for (int i = 0; i < banks.Count; i++)
            {
                DollarTypes dollarType = ConvertToDollarType(banks[i]);
                tasks[i] = Api.DolarBot.GetDollarRate(dollarType);
            }

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override async Task<DollarResponse> GetByBank(Banks bank)
        {
            DollarTypes dollarType = ConvertToDollarType(bank);
            return await Api.DolarBot.GetDollarRate(dollarType).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for dollar Oficial.
        /// </summary>
        /// <returns>A single <see cref="DollarResponse"/>.</returns>
        public async Task<DollarResponse> GetDollarOficial()
        {
            return await Api.DolarBot.GetDollarRate(DollarTypes.Oficial).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for dollar Ahorro.
        /// </summary>
        /// <returns>A single <see cref="DollarResponse"/>.</returns>
        public async Task<DollarResponse> GetDollarAhorro()
        {
            return await Api.DolarBot.GetDollarRate(DollarTypes.Ahorro).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for dollar Blue.
        /// </summary>
        /// <returns>A single <see cref="DollarResponse"/>.</returns>
        public async Task<DollarResponse> GetDollarBlue()
        {
            return await Api.DolarBot.GetDollarRate(DollarTypes.Blue).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for dollar Promedio.
        /// </summary>
        /// <returns>A single <see cref="DollarResponse"/>.</returns>
        public async Task<DollarResponse> GetDollarPromedio()
        {
            return await Api.DolarBot.GetDollarRate(DollarTypes.Promedio).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for dollar Bolsa.
        /// </summary>
        /// <returns>A single <see cref="DollarResponse"/>.</returns>
        public async Task<DollarResponse> GetDollarBolsa()
        {
            return await Api.DolarBot.GetDollarRate(DollarTypes.Bolsa).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for dollar Contado con Liquidación.
        /// </summary>
        /// <returns>A single <see cref="DollarResponse"/>.</returns>
        public async Task<DollarResponse> GetDollarContadoConLiqui()
        {
            return await Api.DolarBot.GetDollarRate(DollarTypes.ContadoConLiqui).ConfigureAwait(false);
        }

        #endregion

        #region Embeds

        /// <inheritdoc />
        public override EmbedBuilder CreateEmbed(DollarResponse[] dollarResponses)
        {
            string dollarImageUrl = Configuration.GetSection("images").GetSection("dollar")["64"];
            return CreateEmbed(dollarResponses, $"Cotizaciones disponibles del {Format.Bold("dólar")} expresadas en {Format.Bold("pesos argentinos")}.", dollarImageUrl);
        }

        /// <inheritdoc />
        public override EmbedBuilder CreateEmbed(DollarResponse[] dollarResponses, string description, string thumbnailUrl)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji dollarEmoji = new Emoji("\uD83D\uDCB5");
            Emoji clockEmoji = new Emoji("\u23F0");
            Emoji buyEmoji = new Emoji(emojis["buyGreen"]);
            Emoji sellEmoji = new Emoji(emojis["sellGreen"]);
            Emoji sellWithTaxesEmoji = new Emoji(emojis["sellWithTaxesGreen"]);

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle("Cotizaciones del Dólar")
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($" C = Compra | V = Venta | A = Venta con impuestos | {clockEmoji} = Last updated (UTC {utcOffset})");

            for (int i = 0; i < dollarResponses.Length; i++)
            {
                DollarResponse response = dollarResponses[i];
                string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
                string title = GetTitle(response.Type);
                string lastUpdated = response.Fecha.ToString("dd/MM - HH:mm");
                string buyPrice = decimal.TryParse(response?.Compra, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal compra) ? compra.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellPrice = decimal.TryParse(response?.Venta, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal venta) ? venta.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellWithTaxesPrice = response?.VentaAhorro != null ? (decimal.TryParse(response?.VentaAhorro, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal ventaAhorro) ? ventaAhorro.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?") : null;

                if (buyPrice != "?" || sellPrice != "?" || (sellWithTaxesPrice != null && sellWithTaxesPrice != "?"))
                {
                    StringBuilder sbField = new StringBuilder()
                                            .AppendLine($"{dollarEmoji} {blankSpace} {buyEmoji} {Format.Bold($"$ {buyPrice}")}")
                                            .AppendLine($"{dollarEmoji} {blankSpace} {sellEmoji} {Format.Bold($"$ {sellPrice}")}");
                    if (sellWithTaxesPrice != null)
                    {
                        sbField.AppendLine($"{dollarEmoji} {blankSpace} {sellWithTaxesEmoji} {Format.Bold($"$ {sellWithTaxesPrice}")}");
                    }
                    sbField.AppendLine($"{clockEmoji} {blankSpace} {lastUpdated}");

                    embed.AddInlineField(title, sbField.AppendLineBreak().ToString());
                }
            }

            embed = AddPlayStoreLink(embed);

            return embed;
        }

        /// <inheritdoc />
        public override async Task<EmbedBuilder> CreateEmbedAsync(DollarResponse dollarResponse, string description, string title = null, string thumbnailUrl = null)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji dollarEmoji = new Emoji("\uD83D\uDCB5");
            Emoji whatsappEmoji = new Emoji(emojis["whatsapp"]);

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            string dollarImageUrl = thumbnailUrl ?? Configuration.GetSection("images").GetSection("dollar")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = title ?? GetTitle(dollarResponse.Type);
            string lastUpdated = dollarResponse.Fecha.ToString(dollarResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string buyPrice = decimal.TryParse(dollarResponse?.Compra, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal compra) ? compra.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string sellPrice = decimal.TryParse(dollarResponse?.Venta, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal venta) ? venta.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

            string buyInlineField = Format.Bold($"{dollarEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {buyPrice}");
            string sellInlineField = Format.Bold($"{dollarEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {sellPrice}");
            string shareText = $"*{(dollarResponse.VentaAhorro != null ? $"Dólar - {embedTitle}" : embedTitle)}*{Environment.NewLine}{Environment.NewLine}Compra: \t\t$ *{buyPrice}*{Environment.NewLine}Venta: \t\t$ *{sellPrice}*";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle(embedTitle)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(dollarImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} (UTC {utcOffset})", footerImageUrl)
                                                   .AddInlineField("Compra", buyInlineField)
                                                   .AddInlineField("Venta", sellInlineField);
            if (dollarResponse.VentaAhorro != null)
            {
                string sellPriceWithTaxes = decimal.TryParse(dollarResponse?.VentaAhorro, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal ventaAhorro) ? ventaAhorro.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellWithTaxesInlineField = Format.Bold($"{dollarEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {sellPriceWithTaxes}");
                embed.AddInlineField("Venta con Impuestos", sellWithTaxesInlineField);
                shareText += $"{Environment.NewLine}Venta c/imp: \t$ *{sellPriceWithTaxes}*";
            }

            shareText += $"{Environment.NewLine}Hora: \t\t{lastUpdated} (UTC {utcOffset})";
            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText, Api.Cuttly.ShortenUrl);

            embed = AddPlayStoreLink(embed);

            return embed;
        }

        /// <summary>
        /// Returns the title depending on the dollar type.
        /// </summary>
        /// <param name="dollarType">The dollar type.</param>
        /// <returns>The corresponding title.</returns>
        private string GetTitle(DollarTypes dollarType)
        {
            return dollarType switch
            {
                DollarTypes.Oficial => DOLAR_OFICIAL_TITLE,
                DollarTypes.Ahorro => DOLAR_AHORRO_TITLE,
                DollarTypes.Blue => DOLAR_BLUE_TITLE,
                DollarTypes.Bolsa => DOLAR_BOLSA_TITLE,
                DollarTypes.Promedio => DOLAR_PROMEDIO_TITLE,
                DollarTypes.ContadoConLiqui => DOLAR_CCL_TITLE,
                _ => Enum.TryParse(dollarType.ToString(), out Banks bank) ? bank.GetDescription() : throw new ArgumentException($"Unable to get title from '{dollarType}'."),
            };
        }

        #endregion

        #endregion
    }
}
