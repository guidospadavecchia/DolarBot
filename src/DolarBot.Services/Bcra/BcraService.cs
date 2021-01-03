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
            return await Api.DolarBot.GetRiesgoPais().ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the BCRA federal reserves.
        /// </summary>
        /// <returns>A <see cref="BcraResponse"/> object.</returns>
        public async Task<BcraResponse> GetReserves()
        {
            return await Api.DolarBot.GetBcraValue(BcraValues.Reservas).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the total circulating money.
        /// </summary>
        /// <returns>A <see cref="BcraResponse"/> object.</returns>
        public async Task<BcraResponse> GetCirculatingMoney()
        {
            return await Api.DolarBot.GetBcraValue(BcraValues.Circulante).ConfigureAwait(false);
        }

        #endregion

        #region Embeds

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="CountryRiskResponse"/>.
        /// </summary>
        /// <param name="riesgoPaisResponse">The Riesgo Pais response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateCountryRiskEmbed(CountryRiskResponse riesgoPaisResponse)
        {
            Emoji chartEmoji = new Emoji("\uD83D\uDCC8");
            string riskImageUrl = Configuration.GetSection("images").GetSection("risk")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
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

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle("Riesgo País")
                                                   .WithDescription($"Valor del {Format.Bold("riesgo país")} de la República Argentina.".AppendLineBreak())
                                                   .WithThumbnailUrl(riskImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {TimeZoneInfo.ConvertTimeFromUtc(riesgoPaisResponse.Fecha, GlobalConfiguration.GetLocalTimeZoneInfo()):dd/MM/yyyy - HH:mm}",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"Valor", $"{Format.Bold($"{chartEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {value}")} puntos".AppendLineBreak());
            return embed;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="BcraResponse"/>.
        /// </summary>
        /// <param name="bcraResponse">The BCRA response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateReservesEmbed(BcraResponse bcraResponse)
        {
            Emoji moneyBagEmoji = new Emoji(":moneybag:");
            string reservesImageUrl = Configuration.GetSection("images").GetSection("reserves")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];

            bool isNumber = double.TryParse(bcraResponse?.Valor, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out double valor);
            string text;
            if (isNumber)
            {
                long convertedValue = (long)Math.Round(valor, MidpointRounding.AwayFromZero);
                string value = convertedValue.ToString("n0", GlobalConfiguration.GetLocalCultureInfo());
                text = $"{Format.Bold($"{moneyBagEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} US$ {value}")}";
            }
            else
            {
                text = $"{Format.Bold($"{moneyBagEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} No informado")}";
            }

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle("Reservas BCRA")
                                                   .WithDescription($"Reservas totales del {Format.Bold("BCRA (Banco Central de la República Argentina)")} expresado en {Format.Bold("dólares estadounidenses")}.".AppendLineBreak())
                                                   .WithThumbnailUrl(reservesImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {TimeZoneInfo.ConvertTimeFromUtc(bcraResponse.Fecha, GlobalConfiguration.GetLocalTimeZoneInfo()):dd/MM/yyyy - HH:mm}",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"Valor", text.AppendLineBreak());
            return embed;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="BcraResponse"/>.
        /// </summary>
        /// <param name="bcraResponse">The BCRA response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateCirculatingMoneyEmbed(BcraResponse bcraResponse)
        {
            Emoji circulatingMoneyEmoji = new Emoji(":money_with_wings:");
            string reservesImageUrl = Configuration.GetSection("images").GetSection("money")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];

            bool isNumber = double.TryParse(bcraResponse?.Valor, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out double valor);
            string text;
            if (isNumber)
            {
                long convertedValue = (long)Math.Round(valor, MidpointRounding.AwayFromZero);
                string value = convertedValue.ToString("n0", GlobalConfiguration.GetLocalCultureInfo());
                text = $"{Format.Bold($"{circulatingMoneyEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {value}")}";
            }
            else
            {
                text = $"{Format.Bold($"{circulatingMoneyEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} No informado")}";
            }

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle("Pesos en circulación")
                                                   .WithDescription($"Cantidad total de {Format.Bold("pesos argentinos")} en circulación.".AppendLineBreak())
                                                   .WithThumbnailUrl(reservesImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {TimeZoneInfo.ConvertTimeFromUtc(bcraResponse.Fecha, GlobalConfiguration.GetLocalTimeZoneInfo()):dd/MM/yyyy - HH:mm}",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"Valor", text.AppendLineBreak());
            return embed;
        }

        #endregion

        #endregion
    }
}
