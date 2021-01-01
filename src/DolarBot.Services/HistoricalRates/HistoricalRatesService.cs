using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HistoricalRatesParams = DolarBot.API.ApiCalls.DolarBotApi.HistoricalRatesParams;

namespace DolarBot.Services.HistoricalRates
{
    /// <summary>
    /// Contains several methods to process historical rates commands.
    /// </summary>
    public class HistoricalRatesService : BaseService
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="HistoricalRatesService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public HistoricalRatesService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        #region API Calls

        /// <summary>
        /// Fetches historical rates for a particular type, specified by the <paramref name="historicalRateParam"/> parameter.
        /// </summary>
        /// <param name="historicalRateParam">The type of historical rates to retrieve.</param>
        /// <returns>A single <see cref="HistoricalRatesResponse"/> object.</returns>
        public async Task<HistoricalRatesResponse> GetHistoricalRates(HistoricalRatesParams historicalRateParam)
        {
            return await Api.DolarBot.GetHistoricalRates(historicalRateParam).ConfigureAwait(false);
        }

        #endregion

        #region Embed

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a single dollar response specifying a custom description, title and thumbnail URL.
        /// </summary>
        /// <param name="historicalRatesResponse">The historical rates response to show.</param>
        /// <param name="historicalRatesParam">The parameter type for historical rates.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateHistoricalRatesEmbed(HistoricalRatesResponse historicalRatesResponse, HistoricalRatesParams historicalRatesParam)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji upEmoji = new Emoji(emojis["arrowUpRed"]);
            Emoji downEmoji = new Emoji(emojis["arrowDownGreen"]);
            Emoji neutralEmoji = new Emoji(emojis["neutral"]);
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
            string chartImageUrl = Configuration.GetSection("images").GetSection("chart")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = GetTitle(historicalRatesParam);
            string embedDescription = GetDescription(historicalRatesParam);
            Color embedColor = GetColor(historicalRatesParam);
            string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(historicalRatesResponse.Fecha, localTimeZone).ToString(historicalRatesResponse.Fecha.Date == DateTime.UtcNow.Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");

            StringBuilder sbField = new StringBuilder();
            var monthlyRates = historicalRatesResponse.Meses.OrderBy(x => Convert.ToInt32(x.Anio)).ThenBy(x => Convert.ToInt32(x.Mes));
            for (int i = 0; i < monthlyRates.Count(); i++)
            {
                HistoricalMonthlyRate month = monthlyRates.ElementAt(i);
                string monthName = GlobalConfiguration.GetLocalCultureInfo().DateTimeFormat.GetMonthName(Convert.ToInt32(month.Mes)).Capitalize();
                bool monthRateIsNumeric = decimal.TryParse(month.Valor, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal monthRate);
                string monthRateText = monthRateIsNumeric ? monthRate.ToString("F2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

                Emoji fieldEmoji = neutralEmoji;
                if (i > 0)
                {
                    HistoricalMonthlyRate previousMonth = monthlyRates.ElementAt(i - 1);
                    bool previousMonthRateIsNumeric = decimal.TryParse(previousMonth.Valor, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal previousMonthRate);
                    if (monthRateIsNumeric && previousMonthRateIsNumeric)
                    {
                        if (monthRate >= previousMonthRate)
                        {
                            fieldEmoji = monthRate > previousMonthRate ? upEmoji : neutralEmoji;
                        }
                        else
                        {
                            fieldEmoji = downEmoji;
                        }
                    }
                }

                string fieldText = $"{fieldEmoji} {blankSpace} {Format.Bold($"$ {monthRate}")} - {Format.Code($"{monthName} {month.Anio}")} {blankSpace}";
                sbField = sbField.AppendLine(fieldText);
            }

            HistoricalMonthlyRate firstMonth = monthlyRates.First();
            HistoricalMonthlyRate lastMonth = monthlyRates.Last();
            string firstMonthName = GlobalConfiguration.GetLocalCultureInfo().DateTimeFormat.GetMonthName(Convert.ToInt32(firstMonth.Mes)).Capitalize();
            string lastMonthName = GlobalConfiguration.GetLocalCultureInfo().DateTimeFormat.GetMonthName(Convert.ToInt32(lastMonth.Mes)).Capitalize();
            string fieldTitle = $"{firstMonthName} {firstMonth.Anio} - {lastMonthName} {lastMonth.Anio}";

            EmbedBuilder embed = new EmbedBuilder().WithColor(embedColor)
                                                   .WithTitle(embedTitle)
                                                   .WithDescription(embedDescription.AppendLineBreak())
                                                   .WithThumbnailUrl(chartImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} ({localTimeZone.StandardName})", footerImageUrl)
                                                   .AddField(fieldTitle, sbField.AppendLineBreak().ToString());
            return embed;
        }

        /// <summary>
        /// Gets the corresponding embed title from the <paramref name="historicalRatesParam"/>.
        /// </summary>
        /// <param name="historicalRatesParam">The parameter type.</param>
        /// <returns>The embed's title.</returns>
        private string GetTitle(HistoricalRatesParams historicalRatesParam)
        {
            return historicalRatesParam switch
            {
                HistoricalRatesParams.Dolar => $"Evolución mensual del dólar oficial",
                HistoricalRatesParams.DolarBlue => $"Evolución mensual del dólar blue",
                HistoricalRatesParams.Euro => $"Evolución mensual del Euro",
                HistoricalRatesParams.Real => $"Evolución mensual del Real",
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Gets the corresponding embed description from the <paramref name="historicalRatesParam"/>.
        /// </summary>
        /// <param name="historicalRatesParam">The parameter type.</param>
        /// <returns>The embed's description.</returns>
        private string GetDescription(HistoricalRatesParams historicalRatesParam)
        {
            return historicalRatesParam switch
            {
                HistoricalRatesParams.Dolar => $"Evolución anual de las cotizaciones promedio por mes del {Format.Bold("dólar oficial")}, expresadas en {Format.Bold("pesos argentinos")}.",
                HistoricalRatesParams.DolarBlue => $"Evolución anual de las cotizaciones promedio por mes del {Format.Bold("dólar blue")}, expresadas en {Format.Bold("pesos argentinos")}.",
                HistoricalRatesParams.Euro => $"Evolución anual de las cotizaciones promedio por mes del {Format.Bold("Euro")}, expresadas en {Format.Bold("pesos argentinos")}.",
                HistoricalRatesParams.Real => $"Evolución anual de las cotizaciones promedio por mes del {Format.Bold("Real")}, expresadas en {Format.Bold("pesos argentinos")}.",
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Gets the corresponding color from the <paramref name="historicalRatesParam"/>.
        /// </summary>
        /// <param name="historicalRatesParam">The parameter type.</param>
        /// <returns>A <see cref="Color"/> object.</returns>
        private Color GetColor(HistoricalRatesParams historicalRatesParam)
        {
            return historicalRatesParam switch
            {
                HistoricalRatesParams.Dolar => GlobalConfiguration.Colors.Main,
                HistoricalRatesParams.DolarBlue => GlobalConfiguration.Colors.Main,
                HistoricalRatesParams.Euro => GlobalConfiguration.Colors.Euro,
                HistoricalRatesParams.Real => GlobalConfiguration.Colors.Real,
                _ => throw new NotImplementedException()
            };
        } 
        #endregion

        #endregion
    }
}
