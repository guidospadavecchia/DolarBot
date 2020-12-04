using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using HistoricalRatesParams = DolarBot.API.ApiCalls.DolarArgentinaApi.HistoricalRatesParams;

namespace DolarBot.Services.HistoricalRates
{
    /// <summary>
    /// Contains several methods to process historical rates commands.
    /// </summary>
    public class HistoricalRatesService
    {
        #region Vars

        /// <summary>
        /// Provides access to application settings.
        /// </summary>
        protected readonly IConfiguration Configuration;

        /// <summary>
        /// Provides access to the different APIs.
        /// </summary>
        protected readonly ApiCalls Api;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="HistoricalRatesService"/> object with the provided configuration, api object and embed color.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public HistoricalRatesService(IConfiguration configuration, ApiCalls api)
        {
            Configuration = configuration;
            Api = api;
        }

        #endregion

        #region Methods

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
                bool monthRateIsNumeric = decimal.TryParse(month.Valor, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal monthRate);
                string monthRateText = monthRateIsNumeric ? monthRate.ToString("F2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

                Emoji fieldEmoji = neutralEmoji;
                if (i > 0)
                {
                    HistoricalMonthlyRate previousMonth = monthlyRates.ElementAt(i - 1);
                    bool previousMonthRateIsNumeric = decimal.TryParse(previousMonth.Valor, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal previousMonthRate);
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
    }
}
