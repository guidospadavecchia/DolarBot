using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace DolarBot.Services.Currencies
{
    /// <summary>
    /// Contains several methods to process world currency related commands.
    /// </summary>
    public class FiatCurrencyService : BaseService
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="FiatCurrencyService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public FiatCurrencyService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        #region API Calls

        /// <summary>
        /// Fetches all the available world currency codes.
        /// </summary>
        /// <returns>A collection of <see cref="WorldCurrencyCodeResponse"/> objects.</returns>
        public async Task<List<WorldCurrencyCodeResponse>> GetWorldCurrenciesList()
        {
            return await Api.DolarBot.GetWorldCurrenciesList();
        }

        /// <summary>
        /// Fetches a single currency rate.
        /// </summary>
        /// <param name="currencyCode">The currency 3-digit code.</param>
        /// <returns>A <see cref="WorldCurrencyResponse"/> object.</returns>
        public async Task<WorldCurrencyResponse> GetCurrencyValue(string currencyCode)
        {
            return await Api.DolarBot.GetWorldCurrencyValue(currencyCode);
        }

        #endregion

        #region Embeds

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="WorldCurrencyResponse"/>.
        /// </summary>
        /// <param name="worldCurrencyResponse">The world currency response.</param>
        /// <param name="currencyName">The currency name.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public async Task<EmbedBuilder> CreateWorldCurrencyEmbedAsync(WorldCurrencyResponse worldCurrencyResponse, string currencyName)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji currencyEmoji = new Emoji(":moneybag:");
            Emoji whatsappEmoji = new Emoji(emojis["whatsapp"]);
            string currencyImageUrl = Configuration.GetSection("images").GetSection("coins")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;
            string lastUpdated = worldCurrencyResponse.Fecha.ToString(worldCurrencyResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string value = decimal.TryParse(worldCurrencyResponse?.Valor, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal valor) ? valor.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";


            string shareText = $"*{currencyName} ({worldCurrencyResponse.Code})*{Environment.NewLine}{Environment.NewLine}Valor: \t$ *{value}*{Environment.NewLine}Hora: \t{lastUpdated} (UTC {utcOffset})";
            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Currency)
                                                   .WithTitle($"{currencyName} ({worldCurrencyResponse.Code})")
                                                   .WithDescription($"Cotización de {Format.Bold($"{currencyName} ({worldCurrencyResponse.Code})")} expresada en {Format.Bold("pesos argentinos")}.".AppendLineBreak())
                                                   .WithThumbnailUrl(currencyImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {lastUpdated} (UTC {utcOffset})",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"Valor", $"{Format.Bold($"{currencyEmoji} ${GlobalConfiguration.Constants.BLANK_SPACE} {value}")}");
            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText, Api.Cuttly.ShortenUrl);
            embed = AddPlayStoreLink(embed);
            return embed;
        }

        #endregion

        #endregion
    }
}
