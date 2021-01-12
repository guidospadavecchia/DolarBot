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
            return await Api.DolarBot.GetVzlaRates(VenezuelaTypes.Dollar).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches all the bolivar to Euro rates.
        /// </summary>
        /// <returns>A <see cref="VzlaResponse"/> object.</returns>
        public async Task<VzlaResponse> GetEuroRates()
        {
            return await Api.DolarBot.GetVzlaRates(VenezuelaTypes.Euro).ConfigureAwait(false);
        }

        #endregion

        #region Embeds

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="VzlaResponse"/>.
        /// </summary>
        /// <param name="vzlaResponse">The Venezuela response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateVzlaEmbed(VzlaResponse vzlaResponse)
        {
            Emoji currencyEmoji = GetEmoji(vzlaResponse.Type);
            Emoji bankEmoji = new Emoji(":bank:");
            Emoji moneyBagEmoji = new Emoji(":moneybag:");
            Emoji colombiaEmoji = new Emoji(":flag_co:");
            string thumbnailUrl = Configuration.GetSection("images").GetSection("venezuela")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];

            string bancosValue = decimal.TryParse(vzlaResponse?.Bancos, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal valorBancos) ? Format.Bold($"B$ {valorBancos.ToString("N2", GlobalConfiguration.GetLocalCultureInfo())}") : "No informado";
            string paraleloValue = decimal.TryParse(vzlaResponse?.Paralelo, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal valorParalelo) ? Format.Bold($"B$ {valorParalelo.ToString("N2", GlobalConfiguration.GetLocalCultureInfo())}") : "No informado";
            string cucutaValue = decimal.TryParse(vzlaResponse?.Cucuta, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal valorCucuta) ? Format.Bold($"B$ {valorCucuta.ToString("N2", GlobalConfiguration.GetLocalCultureInfo())}") : "No informado";
            
            string title = $"{GetName(vzlaResponse.Type).Capitalize()} Venezuela";
            string description = new StringBuilder()
                                 .AppendLine($"Cotizaciones disponibles del {Format.Bold(GetName(vzlaResponse.Type))} expresadas en {Format.Bold("bolívares venezolanos")}.")
                                 .AppendLineBreak()
                                 .AppendLine($"{bankEmoji} {Format.Bold("Bancos")}: {Format.Italics("Promedio de las cotizaciones bancarias")}.")
                                 .AppendLine($"{moneyBagEmoji} {Format.Bold("Paralelo")}: {Format.Italics($"Cotización del {GetName(vzlaResponse.Type).ToLower()} paralelo")}.")
                                 .Append($"{colombiaEmoji} {Format.Bold("Cúcuta")}: {Format.Italics($"Cotización del {GetName(vzlaResponse.Type).ToLower()} en la ciudad de Cúcuta, Colombia")}.")
                                 .ToString();

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Venezuela)
                                                   .WithTitle(title)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {TimeZoneInfo.ConvertTimeFromUtc(vzlaResponse.Fecha, GlobalConfiguration.GetLocalTimeZoneInfo()):dd/MM/yyyy - HH:mm}",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"{bankEmoji} Bancos", $"{currencyEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {bancosValue} {GlobalConfiguration.Constants.BLANK_SPACE}")
                                                   .AddInlineField($"{moneyBagEmoji} Paralelo", $"{currencyEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {paraleloValue} {GlobalConfiguration.Constants.BLANK_SPACE}")
                                                   .AddInlineField($"{colombiaEmoji} Cúcuta", $"{currencyEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {cucutaValue} {GlobalConfiguration.Constants.BLANK_SPACE}".AppendLineBreak());
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
