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
        private const string REAL_TARJETA_TITLE = "Real Tarjeta";
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
                Banks.Bancos,
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
        private static RealEndpoints ConvertToRealType(Banks bank)
        {
            return bank switch
            {
                Banks.Nacion => RealEndpoints.Nacion,
                Banks.BBVA => RealEndpoints.BBVA,
                Banks.Chaco => RealEndpoints.Chaco,
                Banks.Piano => RealEndpoints.Piano,
                Banks.Ciudad => RealEndpoints.Ciudad,
                Banks.Supervielle => RealEndpoints.Supervielle,
                _ => throw new ArgumentException("Unsupported Real type")
            };
        }

        #region API Calls

        /// <inheritdoc />
        public override async Task<RealResponse> GetByBank(Banks bank)
        {
            RealEndpoints realType = ConvertToRealType(bank);
            return await Api.DolarBot.GetRealRate(realType);
        }

        /// <inheritdoc />
        public override async Task<RealResponse[]> GetAllBankRates()
        {
            List<Banks> banks = GetValidBanks().Where(b => b != Banks.Bancos).ToList();
            Task<RealResponse>[] tasks = new Task<RealResponse>[banks.Count];
            for (int i = 0; i < banks.Count; i++)
            {
                RealEndpoints realType = ConvertToRealType(banks[i]);
                tasks[i] = Api.DolarBot.GetRealRate(realType);
            }

            return await Task.WhenAll(tasks);
        }

        /// <inheritdoc />
        public override async Task<RealResponse[]> GetAllStandardRates()
        {
            return await Task.WhenAll(GetRealOficial(), GetRealAhorro(), GetRealTarjeta(), GetRealBlue());
        }

        /// <summary>
        /// Fetches the price for official Real.
        /// </summary>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealOficial()
        {
            return await Api.DolarBot.GetRealRate(RealEndpoints.Oficial);
        }

        /// <summary>
        /// Fetches the price for Real Ahorro.
        /// </summary>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealAhorro()
        {
            return await Api.DolarBot.GetRealRate(RealEndpoints.Ahorro);
        }

        /// <summary>
        /// Fetches the price for Real Tarjeta.
        /// </summary>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealTarjeta()
        {
            return await Api.DolarBot.GetRealRate(RealEndpoints.Tarjeta);
        }

        /// <summary>
        /// Fetches the price for Real Blue.
        /// </summary>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealBlue()
        {
            return await Api.DolarBot.GetRealRate(RealEndpoints.Blue);
        }

        #endregion

        #region Embed

        /// <inheritdoc />
        public override EmbedBuilder CreateEmbed(RealResponse[] realResponses, decimal amount = 1)
        {
            string realImageUrl = Configuration.GetSection("images").GetSection("real")["64"];
            return CreateEmbed(realResponses, $"Cotizaciones disponibles del {Format.Bold("Real")} expresadas en {Format.Bold("pesos argentinos")}.", realImageUrl, amount);
        }

        /// <inheritdoc />
        public override EmbedBuilder CreateEmbed(RealResponse[] realResponses, string description, string thumbnailUrl, decimal amount = 1)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji realEmoji = new(emojis["real"]);
            Emoji clockEmoji = new("\u23F0");
            Emoji buyEmoji = new(emojis["buyYellow"]);
            Emoji sellEmoji = new(emojis["sellYellow"]);
            Emoji sellWithTaxesEmoji = new(emojis["sellWithTaxesYellow"]);
            Emoji amountEmoji = Emoji.Parse(":moneybag:");

            string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;
            string amountField = Format.Bold($"{amountEmoji} {blankSpace} {amount} BRL").AppendLineBreak();

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Real)
                                                   .WithTitle("Cotizaciones del Real")
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($" C = Compra | V = Venta | A = Venta + Impuestos | {clockEmoji} = Last updated (UTC {utcOffset})")
                                                   .AddField("Monto", amountField);

            for (int i = 0; i < realResponses.Length; i++)
            {
                RealResponse response = realResponses[i];
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

            return embed.AddPlayStoreLink(Configuration);
        }

        /// <inheritdoc />
        public override async Task<EmbedBuilder> CreateEmbedAsync(RealResponse realResponse, string description, decimal amount = 1, string title = null, string thumbnailUrl = null)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji realEmoji = new(emojis["real"]);
            Emoji whatsappEmoji = new(emojis["whatsapp"]);
            Emoji amountEmoji = Emoji.Parse(":moneybag:");

            string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            string realImageUrl = thumbnailUrl ?? Configuration.GetSection("images").GetSection("real")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = title ?? GetTitle(realResponse.Type);
            string lastUpdated = realResponse.Fecha.ToString(realResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");

            decimal? buyPriceValue = decimal.TryParse(realResponse?.Compra, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal buyValue) ? buyValue * amount : null;
            decimal? sellPriceValue = decimal.TryParse(realResponse?.Venta, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal sellValue) ? sellValue * amount : null;
            string buyPrice = buyPriceValue.HasValue ? buyPriceValue.Value.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string sellPrice = sellPriceValue.HasValue ? sellPriceValue.Value.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

            string amountField = Format.Bold($"{amountEmoji} {blankSpace} {amount} BRL").AppendLineBreak();
            string buyInlineField = Format.Bold($"{realEmoji} {blankSpace} $ {buyPrice}");
            string sellInlineField = Format.Bold($"{realEmoji} {blankSpace} $ {sellPrice}");
            string shareText = $"*{(realResponse.VentaAhorro != null ? $"Real - {embedTitle}" : embedTitle)}*{Environment.NewLine}{Environment.NewLine}*{amount} BRL*{Environment.NewLine}Compra: \t\t$ *{buyPrice}*{Environment.NewLine}Venta: \t\t$ *{sellPrice}*";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Real)
                                                   .WithTitle(embedTitle)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(realImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} (UTC {utcOffset})", footerImageUrl)
                                                   .AddField("Monto", amountField)
                                                   .AddInlineField("Compra", buyInlineField);
            if (realResponse.VentaAhorro != null)
            {
                decimal? sellPriceWithTaxesValue = decimal.TryParse(realResponse?.VentaAhorro, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal sellWithTaxesValue) ? sellWithTaxesValue * amount : null;
                string sellPriceWithTaxes = sellPriceWithTaxesValue.HasValue ? sellPriceWithTaxesValue.Value.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellWithTaxesInlineField = Format.Bold($"{realEmoji} {blankSpace} $ {sellPriceWithTaxes}");
                embed = embed.AddInlineField("Venta", sellInlineField)
                             .AddInlineField("Venta + Impuestos", sellWithTaxesInlineField.AppendLineBreak());
                shareText += $"{Environment.NewLine}Venta c/imp: \t$ *{sellPriceWithTaxes}*";
            }
            else
            {
                embed = embed.AddInlineField("Venta", sellInlineField.AppendLineBreak());
            }

            shareText += $"{Environment.NewLine}Hora: \t\t{lastUpdated} (UTC {utcOffset})";
            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText, Api.Cuttly.ShortenUrl);
            return embed.AddPlayStoreLink(Configuration);
        }

        /// <summary>
        /// Returns the title depending on the response type.
        /// </summary>
        /// <param name="realType">The real type.</param>
        /// <returns>The corresponding title.</returns>
        private static string GetTitle(RealEndpoints realType)
        {
            return realType switch
            {
                RealEndpoints.Oficial => REAL_OFICIAL_TITLE,
                RealEndpoints.Ahorro => REAL_AHORRO_TITLE,
                RealEndpoints.Tarjeta => REAL_TARJETA_TITLE,
                RealEndpoints.Blue => REAL_BLUE_TITLE,
                _ => Enum.TryParse(realType.ToString(), out Banks bank) ? bank.GetDescription() : throw new ArgumentException($"Unable to get title from '{realType}'."),
            };
        }

        #endregion

        #endregion
    }
}
