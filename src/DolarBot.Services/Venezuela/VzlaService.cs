using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using VenezuelaTypes = DolarBot.API.ApiCalls.DolarBotApi.VenezuelaTypes;

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
            return await Api.DolarBot.GetVzlaRates(VenezuelaTypes.Dollar);
        }

        /// <summary>
        /// Fetches all the bolivar to Euro rates.
        /// </summary>
        /// <returns>A <see cref="VzlaResponse"/> object.</returns>
        public async Task<VzlaResponse> GetEuroRates()
        {
            return await Api.DolarBot.GetVzlaRates(VenezuelaTypes.Euro);
        }

        #endregion

        #region Embeds

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="VzlaResponse"/>.
        /// </summary>
        /// <param name="vzlaResponse">The Venezuela response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public async Task<EmbedBuilder> CreateVzlaEmbedAsync(VzlaResponse vzlaResponse)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji currencyEmoji = GetEmoji(vzlaResponse.Type);
            Emoji bankEmoji = new Emoji(":bank:");
            Emoji moneyBagEmoji = new Emoji(":moneybag:");
            Emoji whatsappEmoji = new Emoji(emojis["whatsapp"]);

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            string thumbnailUrl = Configuration.GetSection("images").GetSection("venezuela")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];

            decimal bancosValue = decimal.TryParse(vzlaResponse?.Bancos, NumberStyles.Any, ApiCalls.DolarBotApi.GetApiCulture(), out decimal valorBancos) ? valorBancos : 0;
            decimal paraleloValue = decimal.TryParse(vzlaResponse?.Paralelo, NumberStyles.Any, ApiCalls.DolarBotApi.GetApiCulture(), out decimal valorParalelo) ? valorParalelo : 0;
            string bancosValueText = bancosValue > 0 ? Format.Bold($"B$ {valorBancos.ToString("N2", GlobalConfiguration.GetLocalCultureInfo())}") : "No informado";
            string paraleloValueText = paraleloValue > 0 ? Format.Bold($"B$ {valorParalelo.ToString("N2", GlobalConfiguration.GetLocalCultureInfo())}") : "No informado";
            
            string title = $"{GetName(vzlaResponse.Type).Capitalize()} Venezuela";
            string description = new StringBuilder()
                                 .AppendLine($"Cotizaciones disponibles del {Format.Bold(GetName(vzlaResponse.Type))} expresadas en {Format.Bold("bolívares venezolanos")}.")
                                 .AppendLineBreak()
                                 .AppendLine($"{bankEmoji} {Format.Bold("Bancos")}: {Format.Italics("Promedio de las cotizaciones bancarias")}.")
                                 .Append($"{moneyBagEmoji} {Format.Bold("Paralelo")}: {Format.Italics($"Cotización del {GetName(vzlaResponse.Type).ToLower()} paralelo")}.")
                                 .ToString();
            string lastUpdated = vzlaResponse.Fecha.ToString(vzlaResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string shareText = $"*{title}*{Environment.NewLine}{Environment.NewLine}Bancos: \t\tB$ *{bancosValue.ToString("N2", GlobalConfiguration.GetLocalCultureInfo())}*{Environment.NewLine}Paralelo: \t\tB$ *{paraleloValue.ToString("N2", GlobalConfiguration.GetLocalCultureInfo())}*{Environment.NewLine}Hora: \t\t{lastUpdated} (UTC {utcOffset})";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Venezuela)
                                                   .WithTitle(title)
                                                   .WithDescription(description.ToString().AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {lastUpdated} (UTC {utcOffset})",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"{bankEmoji} Bancos", $"{currencyEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {bancosValueText} {GlobalConfiguration.Constants.BLANK_SPACE}")
                                                   .AddInlineField($"{moneyBagEmoji} Paralelo", $"{currencyEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {paraleloValueText} {GlobalConfiguration.Constants.BLANK_SPACE}");

            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText, Api.Cuttly.ShortenUrl);
            embed = AddPlayStoreLink(embed);

            return embed;
        }

        /// <summary>
        /// Returns the corresponding name associated to the <paramref name="type"/> parameter.
        /// </summary>
        /// <param name="type">The type of conversion currency.</param>
        /// <returns>The corresponding name.</returns>
        private string GetName(VenezuelaTypes type)
        {
            return type switch
            {
                VenezuelaTypes.Dollar => "Dólar",
                VenezuelaTypes.Euro => "Euro",
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Returns the corresponding <see cref="Emoji"/> associated to the <paramref name="type"/> parameter.
        /// </summary>
        /// <param name="type">The type of conversion currency.</param>
        /// <returns></returns>
        private Emoji GetEmoji(VenezuelaTypes type)
        {
            var emojis = Configuration.GetSection("customEmojis");
            return type switch
            {
                VenezuelaTypes.Dollar => new Emoji("\uD83D\uDCB5"),
                VenezuelaTypes.Euro => new Emoji(":euro:"),
                _ => throw new NotImplementedException()
            };
        }

        #endregion

        #endregion
    }
}
