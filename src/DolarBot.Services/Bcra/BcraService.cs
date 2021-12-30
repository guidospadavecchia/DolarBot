using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Threading.Tasks;
using BcraValues = DolarBot.API.ApiCalls.DolarBotApi.BcraValues;

namespace DolarBot.Services.Bcra
{
    /// <summary>
    /// Contains several methods to process Argentine Central Bank related commands.
    /// </summary>
    public class BcraService : BaseService
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="BcraService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public BcraService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        #region API Calls

        /// <summary>
        /// Fetches the country-risk rate.
        /// </summary>
        /// <returns>A <see cref="CountryRiskResponse"/> object.</returns>
        public async Task<CountryRiskResponse> GetCountryRisk()
        {
            return await Api.DolarBot.GetCountryRiskValue();
        }

        /// <summary>
        /// Fetches the BCRA federal reserves.
        /// </summary>
        /// <returns>A <see cref="BcraResponse"/> object.</returns>
        public async Task<BcraResponse> GetReserves()
        {
            return await Api.DolarBot.GetBcraValue(BcraValues.Reservas);
        }

        /// <summary>
        /// Fetches the total circulating money.
        /// </summary>
        /// <returns>A <see cref="BcraResponse"/> object.</returns>
        public async Task<BcraResponse> GetCirculatingMoney()
        {
            return await Api.DolarBot.GetBcraValue(BcraValues.Circulante);
        }

        #endregion

        #region Embeds

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="CountryRiskResponse"/>.
        /// </summary>
        /// <param name="riesgoPaisResponse">The Riesgo Pais response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public async Task<EmbedBuilder> CreateCountryRiskEmbedAsync(CountryRiskResponse riesgoPaisResponse)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji chartEmoji = new Emoji("\uD83D\uDCC8");
            Emoji whatsappEmoji = new Emoji(emojis["whatsapp"]);
            string riskImageUrl = Configuration.GetSection("images").GetSection("risk")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;
            string lastUpdated = riesgoPaisResponse.Fecha.ToString(riesgoPaisResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            bool isNumber = double.TryParse(riesgoPaisResponse?.Valor, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out double valor);
            string value;
            if (isNumber)
            {
                int convertedValue = (int)Math.Round(valor, MidpointRounding.AwayFromZero);
                value = convertedValue.ToString("n0", GlobalConfiguration.GetLocalCultureInfo());
            }
            else
            {
                value = "No informado";
            }

            string shareText = $"*Riesgo país*{Environment.NewLine}{Environment.NewLine}{(isNumber ? $"*{value}* puntos" : value)}{Environment.NewLine}Hora: {lastUpdated}";
            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle("Riesgo País")
                                                   .WithDescription($"Valor del {Format.Bold("riesgo país")} de la República Argentina.".AppendLineBreak())
                                                   .WithThumbnailUrl(riskImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {lastUpdated} (UTC {utcOffset})",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"Valor", $"{Format.Bold($"{chartEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {value}")} puntos");
            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText, Api.Cuttly.ShortenUrl);
            embed = AddPlayStoreLink(embed);
            return embed;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="BcraResponse"/>.
        /// </summary>
        /// <param name="bcraResponse">The BCRA response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public async Task<EmbedBuilder> CreateReservesEmbedAsync(BcraResponse bcraResponse)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji moneyBagEmoji = new Emoji(":moneybag:");
            Emoji whatsappEmoji = new Emoji(emojis["whatsapp"]);
            string reservesImageUrl = Configuration.GetSection("images").GetSection("reserves")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;
            string lastUpdated = bcraResponse.Fecha.ToString(bcraResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            bool isNumber = double.TryParse(bcraResponse?.Valor, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out double valor);
            string text;
            string value;
            if (isNumber)
            {
                long convertedValue = (long)Math.Round(valor, MidpointRounding.AwayFromZero);
                value = convertedValue.ToString("n2", GlobalConfiguration.GetLocalCultureInfo());
                text = $"{Format.Bold($"{moneyBagEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} US$ {value}")}";
            }
            else
            {
                value = "No Informado";
                text = $"{Format.Bold($"{moneyBagEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {value}")}";
            }

            string shareText = $"*Reservas del tesoro*{Environment.NewLine}{Environment.NewLine}{(isNumber ? $"US$ *{value}*" : value)}{Environment.NewLine}Hora: {lastUpdated}";
            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle("Reservas BCRA")
                                                   .WithDescription($"Reservas totales del {Format.Bold("BCRA (Banco Central de la República Argentina)")} expresado en {Format.Bold("dólares estadounidenses")}.".AppendLineBreak())
                                                   .WithThumbnailUrl(reservesImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {lastUpdated} (UTC {utcOffset})",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"Valor", text);
            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText, Api.Cuttly.ShortenUrl);
            embed = AddPlayStoreLink(embed);

            return embed;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="BcraResponse"/>.
        /// </summary>
        /// <param name="bcraResponse">The BCRA response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public async Task<EmbedBuilder> CreateCirculatingMoneyEmbedAsync(BcraResponse bcraResponse)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji circulatingMoneyEmoji = new Emoji(":money_with_wings:");
            Emoji whatsappEmoji = new Emoji(emojis["whatsapp"]);
            string reservesImageUrl = Configuration.GetSection("images").GetSection("money")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;
            string lastUpdated = bcraResponse.Fecha.ToString(bcraResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            bool isNumber = double.TryParse(bcraResponse?.Valor, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out double valor);
            string text;
            string value;
            if (isNumber)
            {
                long convertedValue = (long)Math.Round(valor, MidpointRounding.AwayFromZero);
                value = convertedValue.ToString("n2", GlobalConfiguration.GetLocalCultureInfo());
                text = $"{Format.Bold($"{circulatingMoneyEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {value}")}";
            }
            else
            {
                value = "No informado";
                text = $"{Format.Bold($"{circulatingMoneyEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {value}")}";
            }

            string shareText = $"*Pesos en circulación*{Environment.NewLine}{Environment.NewLine}{(isNumber ? $"$ *{value}*" : value)}{Environment.NewLine}Hora: {lastUpdated} (UTC {utcOffset})";
            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle("Pesos en circulación")
                                                   .WithDescription($"Cantidad total aproximada de {Format.Bold("pesos argentinos")} en circulación.".AppendLineBreak())
                                                   .WithThumbnailUrl(reservesImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {lastUpdated} (UTC {utcOffset})",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"Valor", text);
            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText, Api.Cuttly.ShortenUrl);
            embed = AddPlayStoreLink(embed);

            return embed;
        }

        #endregion

        #endregion
    }
}
