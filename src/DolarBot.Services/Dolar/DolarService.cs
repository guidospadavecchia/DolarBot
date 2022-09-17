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
        private const string DOLAR_TARJETA_TITLE = "Dólar Tarjeta";
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
        /// Converts a <see cref="Banks"/> object to its <see cref="DollarEndpoints"/> equivalent.
        /// </summary>
        /// <param name="bank">The value to convert.</param>
        /// <returns>The converted value as <see cref="DollarEndpoints"/>.</returns>
        private static DollarEndpoints ConvertToDollarType(Banks bank)
        {
            return bank switch
            {
                Banks.Nacion => DollarEndpoints.Nacion,
                Banks.BBVA => DollarEndpoints.BBVA,
                Banks.Piano => DollarEndpoints.Piano,
                Banks.Hipotecario => DollarEndpoints.Hipotecario,
                Banks.Galicia => DollarEndpoints.Galicia,
                Banks.Santander => DollarEndpoints.Santander,
                Banks.Ciudad => DollarEndpoints.Ciudad,
                Banks.Supervielle => DollarEndpoints.Supervielle,
                Banks.Patagonia => DollarEndpoints.Patagonia,
                Banks.Comafi => DollarEndpoints.Comafi,
                Banks.Bancor => DollarEndpoints.Bancor,
                Banks.Chaco => DollarEndpoints.Chaco,
                Banks.Pampa => DollarEndpoints.Pampa,
                Banks.Provincia => DollarEndpoints.Provincia,
                Banks.ICBC => DollarEndpoints.ICBC,
                Banks.Reba => DollarEndpoints.Reba,
                Banks.Roela => DollarEndpoints.Roela,
                _ => throw new ArgumentException("Unsupported Dollar type")
            };
        }

        #region API Calls

        /// <inheritdoc />
        public override async Task<DollarResponse[]> GetAllStandardRates()
        {
            return await Task.WhenAll(GetDollarOficial(),
                                      GetDollarAhorro(),
                                      GetDollarTarjeta(),
                                      GetDollarBlue(),
                                      GetDollarBolsa(),
                                      GetDollarPromedio(),
                                      GetDollarContadoConLiqui());
        }

        /// <inheritdoc />
        public override async Task<DollarResponse[]> GetAllBankRates()
        {
            List<Banks> banks = GetValidBanks().Where(b => b != Banks.Bancos).ToList();
            Task<DollarResponse>[] tasks = new Task<DollarResponse>[banks.Count];
            for (int i = 0; i < banks.Count; i++)
            {
                DollarEndpoints dollarType = ConvertToDollarType(banks[i]);
                tasks[i] = Api.DolarBot.GetDollarRate(dollarType);
            }

            return await Task.WhenAll(tasks);
        }

        /// <inheritdoc />
        public override async Task<DollarResponse> GetByBank(Banks bank)
        {
            DollarEndpoints dollarType = ConvertToDollarType(bank);
            return await Api.DolarBot.GetDollarRate(dollarType);
        }

        /// <summary>
        /// Fetches the price for dollar Oficial.
        /// </summary>
        /// <returns>A single <see cref="DollarResponse"/>.</returns>
        public async Task<DollarResponse> GetDollarOficial()
        {
            return await Api.DolarBot.GetDollarRate(DollarEndpoints.Oficial);
        }

        /// <summary>
        /// Fetches the price for dollar Ahorro.
        /// </summary>
        /// <returns>A single <see cref="DollarResponse"/>.</returns>
        public async Task<DollarResponse> GetDollarAhorro()
        {
            return await Api.DolarBot.GetDollarRate(DollarEndpoints.Ahorro);
        }

        /// <summary>
        /// Fetches the price for dollar Tarjeta.
        /// </summary>
        /// <returns>A single <see cref="DollarResponse"/>.</returns>
        public async Task<DollarResponse> GetDollarTarjeta()
        {
            return await Api.DolarBot.GetDollarRate(DollarEndpoints.Tarjeta);
        }

        /// <summary>
        /// Fetches the price for dollar Blue.
        /// </summary>
        /// <returns>A single <see cref="DollarResponse"/>.</returns>
        public async Task<DollarResponse> GetDollarBlue()
        {
            return await Api.DolarBot.GetDollarRate(DollarEndpoints.Blue);
        }

        /// <summary>
        /// Fetches the price for dollar Promedio.
        /// </summary>
        /// <returns>A single <see cref="DollarResponse"/>.</returns>
        public async Task<DollarResponse> GetDollarPromedio()
        {
            return await Api.DolarBot.GetDollarRate(DollarEndpoints.Promedio);
        }

        /// <summary>
        /// Fetches the price for dollar Bolsa.
        /// </summary>
        /// <returns>A single <see cref="DollarResponse"/>.</returns>
        public async Task<DollarResponse> GetDollarBolsa()
        {
            return await Api.DolarBot.GetDollarRate(DollarEndpoints.Bolsa);
        }

        /// <summary>
        /// Fetches the price for dollar Contado con Liquidación.
        /// </summary>
        /// <returns>A single <see cref="DollarResponse"/>.</returns>
        public async Task<DollarResponse> GetDollarContadoConLiqui()
        {
            return await Api.DolarBot.GetDollarRate(DollarEndpoints.ContadoConLiqui);
        }

        #endregion

        #region Embeds

        /// <inheritdoc />
        public override EmbedBuilder CreateEmbed(DollarResponse[] dollarResponses, decimal amount = 1)
        {
            string dollarImageUrl = Configuration.GetSection("images").GetSection("dollar")["64"];
            return CreateEmbed(dollarResponses, $"Cotizaciones disponibles del {Format.Bold("dólar")} expresadas en {Format.Bold("pesos argentinos")}.", dollarImageUrl, amount);
        }

        /// <inheritdoc />
        public override EmbedBuilder CreateEmbed(DollarResponse[] dollarResponses, string description, string thumbnailUrl, decimal amount = 1)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji dollarEmoji = new("\uD83D\uDCB5");
            Emoji clockEmoji = new("\u23F0");
            Emoji buyEmoji = new(emojis["buyGreen"]);
            Emoji sellEmoji = new(emojis["sellGreen"]);
            Emoji sellWithTaxesEmoji = new(emojis["sellWithTaxesGreen"]);
            Emoji amountEmoji = Emoji.Parse(":moneybag:");

            string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;
            string amountField = Format.Bold($"{amountEmoji} {blankSpace} {amount} USD").AppendLineBreak();

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle("Cotizaciones del Dólar")
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($" C = Compra | V = Venta | A = Venta + Impuestos | {clockEmoji} = Last updated (UTC {utcOffset})")
                                                   .AddField("Monto", amountField);

            for (int i = 0; i < dollarResponses.Length; i++)
            {
                DollarResponse response = dollarResponses[i];
                string title = GetTitle(response.Type);
                string lastUpdated = response.Fecha.ToString("dd/MM - HH:mm");

                decimal? buyPriceValue = decimal.TryParse(response?.Compra, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal buyValue) ? buyValue * amount : null;
                decimal? sellPriceValue = decimal.TryParse(response?.Venta, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal sellValue) ? sellValue * amount : null;
                decimal? sellPriceWithTaxesValue = decimal.TryParse(response?.VentaAhorro, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal sellWithTaxesValue) ? sellWithTaxesValue * amount : null;

                if (buyPriceValue.HasValue || sellPriceValue.HasValue)
                {
                    string buyPrice = buyPriceValue.HasValue ? buyPriceValue.Value.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                    string sellPrice = sellPriceValue.HasValue ? sellPriceValue.Value.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                    string sellWithTaxesPrice = response?.VentaAhorro != null ? (sellPriceWithTaxesValue.HasValue ? sellPriceWithTaxesValue.Value.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?") : null;
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

            return embed.AddPlayStoreLink(Configuration);
        }

        /// <inheritdoc />
        public override async Task<EmbedBuilder> CreateEmbedAsync(DollarResponse dollarResponse, string description, decimal amount = 1, string title = null, string thumbnailUrl = null)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji dollarEmoji = new("\uD83D\uDCB5");
            Emoji whatsappEmoji = new(emojis["whatsapp"]);
            Emoji amountEmoji = Emoji.Parse(":moneybag:");

            string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            string dollarImageUrl = thumbnailUrl ?? Configuration.GetSection("images").GetSection("dollar")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = title ?? GetTitle(dollarResponse.Type);
            string lastUpdated = dollarResponse.Fecha.ToString(dollarResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");

            decimal? buyPriceValue = decimal.TryParse(dollarResponse?.Compra, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal buyValue) ? buyValue * amount : null;
            decimal? sellPriceValue = decimal.TryParse(dollarResponse?.Venta, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal sellValue) ? sellValue * amount : null;
            string buyPrice = buyPriceValue.HasValue ? buyPriceValue.Value.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string sellPrice = sellPriceValue.HasValue ? sellPriceValue.Value.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

            string amountField = Format.Bold($"{amountEmoji} {blankSpace} {amount} USD").AppendLineBreak();
            string buyInlineField = Format.Bold($"{dollarEmoji} {blankSpace} $ {buyPrice}");
            string sellInlineField = Format.Bold($"{dollarEmoji} {blankSpace} $ {sellPrice}");
            string shareText = $"*{(dollarResponse.VentaAhorro != null ? $"Dólar - {embedTitle}" : embedTitle)}*{Environment.NewLine}{Environment.NewLine}*{amount} USD*{Environment.NewLine}Compra: \t\t$ *{buyPrice}*{Environment.NewLine}Venta: \t\t$ *{sellPrice}*";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle(embedTitle)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(dollarImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} (UTC {utcOffset})", footerImageUrl)
                                                   .AddField("Monto", amountField)
                                                   .AddInlineField("Compra", buyInlineField);
            if (dollarResponse.VentaAhorro != null)
            {
                decimal? sellPriceWithTaxesValue = decimal.TryParse(dollarResponse?.VentaAhorro, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal sellWithTaxesValue) ? sellWithTaxesValue * amount : null;
                string sellPriceWithTaxes = sellPriceWithTaxesValue.HasValue ? sellPriceWithTaxesValue.Value.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellWithTaxesInlineField = Format.Bold($"{dollarEmoji} {blankSpace} $ {sellPriceWithTaxes}");
                embed = embed.AddInlineField("Venta", sellInlineField)
                             .AddInlineField("Venta + Impuestos", sellWithTaxesInlineField.AppendLineBreak());
                shareText += $"{Environment.NewLine}Venta c/imp: \t$ *{sellPriceWithTaxes}*";
            }
            else
            {
                embed.AddInlineField("Venta", sellInlineField.AppendLineBreak());
            }

            shareText += $"{Environment.NewLine}Hora: \t\t{lastUpdated} (UTC {utcOffset})";
            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText, Api.Cuttly.ShortenUrl);

            return embed.AddPlayStoreLink(Configuration);
        }

        /// <summary>
        /// Returns the title depending on the dollar type.
        /// </summary>
        /// <param name="dollarType">The dollar type.</param>
        /// <returns>The corresponding title.</returns>
        private static string GetTitle(DollarEndpoints dollarType)
        {
            return dollarType switch
            {
                DollarEndpoints.Oficial => DOLAR_OFICIAL_TITLE,
                DollarEndpoints.Ahorro => DOLAR_AHORRO_TITLE,
                DollarEndpoints.Tarjeta => DOLAR_TARJETA_TITLE,
                DollarEndpoints.Blue => DOLAR_BLUE_TITLE,
                DollarEndpoints.Bolsa => DOLAR_BOLSA_TITLE,
                DollarEndpoints.Promedio => DOLAR_PROMEDIO_TITLE,
                DollarEndpoints.ContadoConLiqui => DOLAR_CCL_TITLE,
                _ => Enum.TryParse(dollarType.ToString(), out Banks bank) ? bank.GetDescription() : throw new ArgumentException($"Unable to get title from '{dollarType}'."),
            };
        }

        #endregion

        #endregion
    }
}
