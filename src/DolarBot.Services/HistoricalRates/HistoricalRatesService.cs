﻿using Discord;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<HistoricalRatesResponse> GetHistoricalRates(HistoricalRatesParamEndpoints historicalRateParam)
        {
            return await Api.DolarBot.GetHistoricalRates(historicalRateParam);
        }

        #endregion

        #region Embed

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a single dollar response specifying a custom description, title and thumbnail URL.
        /// </summary>
        /// <param name="historicalRatesResponse">The historical rates response to show.</param>
        /// <param name="historicalRatesParam">The parameter type for historical rates.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateHistoricalRatesEmbed(HistoricalRatesResponse historicalRatesResponse, HistoricalRatesParamEndpoints historicalRatesParam)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji upEmoji = new(emojis["arrowUpRed"]);
            Emoji downEmoji = new(emojis["arrowDownGreen"]);
            Emoji neutralEmoji = new(emojis["neutral"]);
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;
            string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
            string chartImageUrl = Configuration.GetSection("images").GetSection("chart")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = GetTitle(historicalRatesParam);
            string embedDescription = GetDescription(historicalRatesParam);
            Color embedColor = GetColor(historicalRatesParam);
            string lastUpdated = historicalRatesResponse.Fecha.ToString(historicalRatesResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");

            StringBuilder sbField = new();
            var monthlyRates = historicalRatesResponse.Meses.OrderBy(x => Convert.ToInt32(x.Anio)).ThenBy(x => Convert.ToInt32(x.Mes));
            for (int i = 0; i < monthlyRates.Count(); i++)
            {
                HistoricalMonthlyRate month = monthlyRates.ElementAt(i);
                string monthName = GlobalConfiguration.GetLocalCultureInfo().DateTimeFormat.GetMonthName(Convert.ToInt32(month.Mes)).Capitalize();
                bool monthRateIsNumeric = decimal.TryParse(month.Valor, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal monthRate);
                string monthRateText = monthRateIsNumeric ? monthRate.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

                Emoji fieldEmoji = neutralEmoji;
                if (i > 0)
                {
                    HistoricalMonthlyRate previousMonth = monthlyRates.ElementAt(i - 1);
                    bool previousMonthRateIsNumeric = decimal.TryParse(previousMonth.Valor, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal previousMonthRate);
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
                                                   .WithFooter($"Ultima actualización: {lastUpdated} (UTC {utcOffset})", footerImageUrl)
                                                   .AddField(fieldTitle, sbField.AppendLineBreak().ToString());

            return embed.AddPlayStoreLink(Configuration, true)
                        .AddDonationLink(Configuration, true);
        }

        /// <summary>
        /// Gets the corresponding embed title from the <paramref name="historicalRatesParam"/>.
        /// </summary>
        /// <param name="historicalRatesParam">The parameter type.</param>
        /// <returns>The embed's title.</returns>
        private static string GetTitle(HistoricalRatesParamEndpoints historicalRatesParam)
        {
            return historicalRatesParam switch
            {
                HistoricalRatesParamEndpoints.Dolar => $"Evolución mensual del dólar oficial",
                HistoricalRatesParamEndpoints.DolarAhorro => $"Evolución mensual del dólar ahorro",
                HistoricalRatesParamEndpoints.DolarBlue => $"Evolución mensual del dólar blue",
                HistoricalRatesParamEndpoints.Euro => $"Evolución mensual del Euro",
                HistoricalRatesParamEndpoints.EuroAhorro => $"Evolución mensual del Euro ahorro",
                HistoricalRatesParamEndpoints.Real => $"Evolución mensual del Real",
                HistoricalRatesParamEndpoints.RealAhorro => $"Evolución mensual del Real ahorro",
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Gets the corresponding embed description from the <paramref name="historicalRatesParam"/>.
        /// </summary>
        /// <param name="historicalRatesParam">The parameter type.</param>
        /// <returns>The embed's description.</returns>
        private static string GetDescription(HistoricalRatesParamEndpoints historicalRatesParam)
        {
            return historicalRatesParam switch
            {
                HistoricalRatesParamEndpoints.Dolar => $"Evolución anual de las cotizaciones promedio por mes del {Format.Bold("dólar oficial")}, expresadas en {Format.Bold("pesos argentinos")}.",
                HistoricalRatesParamEndpoints.DolarAhorro => $"Evolución anual de las cotizaciones promedio por mes del {Format.Bold("dólar oficial")} con impuestos, expresadas en {Format.Bold("pesos argentinos")}.",
                HistoricalRatesParamEndpoints.DolarBlue => $"Evolución anual de las cotizaciones promedio por mes del {Format.Bold("dólar blue")}, expresadas en {Format.Bold("pesos argentinos")}.",
                HistoricalRatesParamEndpoints.Euro => $"Evolución anual de las cotizaciones promedio por mes del {Format.Bold("Euro")}, expresadas en {Format.Bold("pesos argentinos")}.",
                HistoricalRatesParamEndpoints.EuroAhorro => $"Evolución anual de las cotizaciones promedio por mes del {Format.Bold("Euro")} con impuestos, expresadas en {Format.Bold("pesos argentinos")}.",
                HistoricalRatesParamEndpoints.Real => $"Evolución anual de las cotizaciones promedio por mes del {Format.Bold("Real")}, expresadas en {Format.Bold("pesos argentinos")}.",
                HistoricalRatesParamEndpoints.RealAhorro => $"Evolución anual de las cotizaciones promedio por mes del {Format.Bold("Real")} con impuestos, expresadas en {Format.Bold("pesos argentinos")}.",
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Gets the corresponding color from the <paramref name="historicalRatesParam"/>.
        /// </summary>
        /// <param name="historicalRatesParam">The parameter type.</param>
        /// <returns>A <see cref="Color"/> object.</returns>
        private static Color GetColor(HistoricalRatesParamEndpoints historicalRatesParam)
        {
            return historicalRatesParam switch
            {
                HistoricalRatesParamEndpoints.Dolar => GlobalConfiguration.Colors.Main,
                HistoricalRatesParamEndpoints.DolarAhorro => GlobalConfiguration.Colors.Main,
                HistoricalRatesParamEndpoints.DolarBlue => GlobalConfiguration.Colors.Main,
                HistoricalRatesParamEndpoints.Euro => GlobalConfiguration.Colors.Euro,
                HistoricalRatesParamEndpoints.EuroAhorro => GlobalConfiguration.Colors.Euro,
                HistoricalRatesParamEndpoints.Real => GlobalConfiguration.Colors.Real,
                HistoricalRatesParamEndpoints.RealAhorro => GlobalConfiguration.Colors.Real,
                _ => throw new NotImplementedException()
            };
        }
        #endregion

        #endregion
    }
}
