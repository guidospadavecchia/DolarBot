using Discord;
using DolarBot.API;
using DolarBot.API.Enums;
using DolarBot.API.Models;
using DolarBot.API.Services.DolarBotApi;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace DolarBot.Services.Venezuela
{
    /// <summary>
    /// Contains several methods to process Venezuela rates related commands.
    /// </summary>
    public class VzlaService : BaseService
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="VzlaService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public VzlaService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        #region API Calls

        /// <summary>
        /// Fetches all the bolivar to Dollar rates.
        /// </summary>
        /// <returns>A <see cref="VzlaResponse"/> object.</returns>
        public async Task<VzlaResponse> GetDollarRates()
        {
            return await Api.DolarBot.GetVzlaRates(VenezuelaEndpoints.Dollar);
        }

        /// <summary>
        /// Fetches all the bolivar to Euro rates.
        /// </summary>
        /// <returns>A <see cref="VzlaResponse"/> object.</returns>
        public async Task<VzlaResponse> GetEuroRates()
        {
            return await Api.DolarBot.GetVzlaRates(VenezuelaEndpoints.Euro);
        }

        #endregion

        #region Embeds

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="VzlaResponse"/>.
        /// </summary>
        /// <param name="vzlaResponse">The Venezuela response.</param>
        /// <param name="amount">The amount to rate against.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public async Task<EmbedBuilder> CreateVzlaEmbedAsync(VzlaResponse vzlaResponse, decimal amount = 1)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji currencyEmoji = GetEmoji(vzlaResponse.Type);
            Emoji bankEmoji = new(":bank:");
            Emoji moneyEmoji = new(":money_with_wings:");
            Emoji whatsappEmoji = new(emojis["whatsapp"]);
            Emoji amountEmoji = Emoji.Parse(":moneybag:");

            string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            string thumbnailUrl = Configuration.GetSection("images").GetSection("venezuela")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string currencyCode = vzlaResponse.Type switch
            {
                VenezuelaEndpoints.Dollar => "USD",
                VenezuelaEndpoints.Euro => "EUR",
                _ => throw new NotImplementedException(),
            };

            decimal? bancosValue = decimal.TryParse(vzlaResponse?.Bancos, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal b) ? b * amount : null;
            decimal? paraleloValue = decimal.TryParse(vzlaResponse?.Paralelo, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal p) ? p * amount : null;
            string bancosValueText = bancosValue.HasValue ? Format.Bold($"B$ {bancosValue.Value.ToString("N2", GlobalConfiguration.GetLocalCultureInfo())}") : "No informado";
            string paraleloValueText = paraleloValue.HasValue ? Format.Bold($"B$ {paraleloValue.Value.ToString("N2", GlobalConfiguration.GetLocalCultureInfo())}") : "No informado";

            string title = $"{GetName(vzlaResponse.Type).Capitalize()} Venezuela";
            string description = new StringBuilder()
                                 .AppendLine($"Cotizaciones disponibles del {Format.Bold(GetName(vzlaResponse.Type))} expresadas en {Format.Bold("bolívares venezolanos")}.")
                                 .AppendLineBreak()
                                 .AppendLine($"{bankEmoji} {Format.Bold("Bancos")}: {Format.Italics("Promedio de las cotizaciones bancarias")}.")
                                 .Append($"{moneyEmoji} {Format.Bold("Paralelo")}: {Format.Italics($"Cotización del {GetName(vzlaResponse.Type).ToLower()} paralelo")}.")
                                 .ToString();
            string lastUpdated = vzlaResponse.Fecha.ToString(vzlaResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");

            string amountField = Format.Bold($"{amountEmoji} {blankSpace} {amount} {currencyCode}").AppendLineBreak();
            string shareText = $"*{title}*{Environment.NewLine}{Environment.NewLine}*{amount} {currencyCode}*{Environment.NewLine}Bancos: \t\tB$ *{bancosValue.GetValueOrDefault().ToString("N2", GlobalConfiguration.GetLocalCultureInfo())}*{Environment.NewLine}Paralelo: \t\tB$ *{paraleloValue.GetValueOrDefault().ToString("N2", GlobalConfiguration.GetLocalCultureInfo())}*{Environment.NewLine}Hora: \t\t{lastUpdated} (UTC {utcOffset})";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Venezuela)
                                                   .WithTitle(title)
                                                   .WithDescription(description.ToString().AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {lastUpdated} (UTC {utcOffset})",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddField("Monto", amountField)
                                                   .AddInlineField($"{bankEmoji} Bancos", $"{currencyEmoji} {blankSpace} {bancosValueText} {blankSpace}")
                                                   .AddInlineField($"{moneyEmoji} Paralelo", $"{currencyEmoji} {blankSpace} {paraleloValueText} {blankSpace}".AppendLineBreak());

            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText);
            return embed.AddPlayStoreLink(Configuration, true)
                        .AddDonationLink(Configuration, true);
        }

        /// <summary>
        /// Returns the corresponding name associated to the <paramref name="type"/> parameter.
        /// </summary>
        /// <param name="type">The type of conversion currency.</param>
        /// <returns>The corresponding name.</returns>
        private static string GetName(VenezuelaEndpoints type)
        {
            return type switch
            {
                VenezuelaEndpoints.Dollar => "Dólar",
                VenezuelaEndpoints.Euro => "Euro",
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Returns the corresponding <see cref="Emoji"/> associated to the <paramref name="type"/> parameter.
        /// </summary>
        /// <param name="type">The type of conversion currency.</param>
        /// <returns></returns>
        private static Emoji GetEmoji(VenezuelaEndpoints type)
        {
            return type switch
            {
                VenezuelaEndpoints.Dollar => new Emoji("\uD83D\uDCB5"),
                VenezuelaEndpoints.Euro => new Emoji(":euro:"),
                _ => throw new NotImplementedException()
            };
        }

        #endregion

        #endregion
    }
}
