﻿using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using static DolarBot.API.ApiCalls.DolarArgentinaApi;

namespace DolarBot.Modules.Services.Dolar
{
    /// <summary>
    /// Contains several methods to process dollar commands.
    /// </summary>
    public class DolarService
    {
        #region Constants
        private const string DOLAR_OFICIAL_TITLE = "Dólar Oficial";
        private const string DOLAR_AHORRO_TITLE = "Dólar Ahorro";
        private const string DOLAR_BLUE_TITLE = "Dólar Blue";
        private const string DOLAR_BOLSA_TITLE = "Dólar Bolsa (MEP)";
        private const string DOLAR_PROMEDIO_TITLE = "Dólar Promedio";
        private const string DOLAR_CCL_TITLE = "Contado con Liqui";
        #endregion

        #region Vars

        /// <summary>
        /// Provides access to application settings.
        /// </summary>
        protected readonly IConfiguration Configuration;

        /// <summary>
        /// Provides access to the different APIs.
        /// </summary>
        protected readonly ApiCalls Api;

        /// <summary>
        /// Color for the embed messages.
        /// </summary>
        private readonly Color EmbedColor;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="DolarService"/> object with the provided configuration, api object and embed color.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="embedColor">The color to use for embed messages.</param>
        public DolarService(IConfiguration configuration, ApiCalls api, Color embedColor)
        {
            Configuration = configuration;
            EmbedColor = embedColor;
            Api = api;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple dollar responses.
        /// </summary>
        /// <param name="dollarResponses">The dollar responses to show.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateDollarEmbed(DolarResponse[] dollarResponses)
        {
            string dollarImageUrl = Configuration.GetSection("images").GetSection("dollar")["64"];
            return CreateDollarEmbed(dollarResponses, $"Cotizaciones disponibles expresadas en {Format.Bold("pesos argentinos")}.", dollarImageUrl);
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple dollar responses specifying a custom description and thumbnail URL.
        /// </summary>
        /// <param name="dollarResponses">The dollar responses to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="thumbnailUrl">The URL of the embed's thumbnail image.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateDollarEmbed(DolarResponse[] dollarResponses, string description, string thumbnailUrl)
        {
            Emoji dollarEmoji = new Emoji("\uD83D\uDCB5");
            Emoji clockEmoji = new Emoji("\u23F0");

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();

            EmbedBuilder embed = new EmbedBuilder().WithColor(EmbedColor)
                                                   .WithTitle("Cotizaciones del Dólar")
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($"{clockEmoji} = Última actualización ({localTimeZone.StandardName})");

            for (int i = 0; i < dollarResponses.Length; i++)
            {
                DolarResponse response = dollarResponses[i];
                string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
                string title = GetTitle(response);
                string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(response.Fecha, localTimeZone).ToString("dd/MM - HH:mm");
                string buyPrice = decimal.TryParse(response?.Compra, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal compra) ? $"${compra:F}" : "?";
                string sellPrice = decimal.TryParse(response?.Venta, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal venta) ? $"${venta:F}" : "?";

                if (buyPrice != "?" || sellPrice != "?")
                {
                    StringBuilder sbField = new StringBuilder()
                                            .AppendLine($"{dollarEmoji} {blankSpace} Compra: {Format.Bold(buyPrice)} {blankSpace}")
                                            .AppendLine($"{dollarEmoji} {blankSpace} Venta: {Format.Bold(sellPrice)} {blankSpace}")
                                            .AppendLine($"{clockEmoji} {blankSpace} {lastUpdated} {blankSpace}  ");
                    embed.AddInlineField(title, sbField.ToString().AppendLineBreak());
                }
            }

            return embed;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a single dollar response specifying a custom description, title and thumbnail URL.
        /// </summary>
        /// <param name="dollarResponse">>The dollar response to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="title">Optional. The embed's title.</param>
        /// <param name="thumbnailUrl">Optional. The embed's thumbnail URL.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateDollarEmbed(DolarResponse dollarResponse, string description, string title = null, string thumbnailUrl = null)
        {
            Emoji dollarEmoji = new Emoji("\uD83D\uDCB5");
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            string dollarImageUrl = thumbnailUrl ?? Configuration.GetSection("images").GetSection("dollar")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = title ?? GetTitle(dollarResponse);
            string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(dollarResponse.Fecha, localTimeZone).ToString(dollarResponse.Fecha.Date == DateTime.UtcNow.Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string buyPrice = decimal.TryParse(dollarResponse?.Compra, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal compra) ? $"${compra:F}" : null;
            string sellPrice = decimal.TryParse(dollarResponse?.Venta, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal venta) ? $"${venta:F}" : null;

            EmbedBuilder embed = new EmbedBuilder().WithColor(EmbedColor)
                                                   .WithTitle(embedTitle)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(dollarImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} ({localTimeZone.StandardName})", footerImageUrl)
                                                   .AddInlineField("Compra", Format.Bold($"{dollarEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {buyPrice}"))
                                                   .AddInlineField("Venta", Format.Bold($"{dollarEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {sellPrice}".AppendLineBreak()));
            return embed;
        }

        /// <summary>
        /// Converts a <see cref="Banks"/> object to its <see cref="DollarType"/> equivalent and returns its thumbnail URL.
        /// </summary>
        /// <param name="bank">The value to convert.</param>
        /// <param name="thumbnailUrl">The thumbnail URL corresponding to the bank.</param>
        /// <returns>The converted value as <see cref="DollarType"/>.</returns>
        public DollarType GetBankInformation(Banks bank, out string thumbnailUrl)
        {
            switch (bank)
            {
                case Banks.Nacion:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["nacion"];
                    return DollarType.Nacion;
                case Banks.BBVA:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["bbva"];
                    return DollarType.BBVA;
                case Banks.Piano:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["piano"];
                    return DollarType.Piano;
                case Banks.Hipotecario:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["hipotecario"];
                    return DollarType.Hipotecario;
                case Banks.Galicia:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["galicia"];
                    return DollarType.Galicia;
                case Banks.Santander:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["santander"];
                    return DollarType.Santander;
                case Banks.Ciudad:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["ciudad"];
                    return DollarType.Ciudad;
                case Banks.Supervielle:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["supervielle"];
                    return DollarType.Supervielle;
                case Banks.Patagonia:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["patagonia"];
                    return DollarType.Patagonia;
                case Banks.Comafi:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["comafi"];
                    return DollarType.Comafi;
                case Banks.BIND:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["bind"];
                    return DollarType.BIND;
                case Banks.Bancor:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["bancor"];
                    return DollarType.Bancor;
                case Banks.Chaco:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["chaco"];
                    return DollarType.Chaco;
                case Banks.Pampa:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["pampa"];
                    return DollarType.Pampa;
                default:
                    thumbnailUrl = string.Empty;
                    return DollarType.Oficial;
            }
        }

        /// <summary>
        /// Returns the title depending on the response type.
        /// </summary>
        /// <param name="dollarResponse">The dollar response.</param>
        /// <returns>The corresponding title.</returns>
        private string GetTitle(DolarResponse dollarResponse)
        {
            return dollarResponse.Type switch
            {
                DollarType.Oficial => DOLAR_OFICIAL_TITLE,
                DollarType.Ahorro => DOLAR_AHORRO_TITLE,
                DollarType.Blue => DOLAR_BLUE_TITLE,
                DollarType.Bolsa => DOLAR_BOLSA_TITLE,
                DollarType.Promedio => DOLAR_PROMEDIO_TITLE,
                DollarType.ContadoConLiqui => DOLAR_CCL_TITLE,
                DollarType.Nacion => Banks.Nacion.GetDescription(),
                DollarType.BBVA => Banks.BBVA.GetDescription(),
                DollarType.Piano => Banks.Piano.GetDescription(),
                DollarType.Hipotecario => Banks.Hipotecario.GetDescription(),
                DollarType.Galicia => Banks.Galicia.GetDescription(),
                DollarType.Santander => Banks.Santander.GetDescription(),
                DollarType.Ciudad => Banks.Ciudad.GetDescription(),
                DollarType.Supervielle => Banks.Supervielle.GetDescription(),
                DollarType.Patagonia => Banks.Patagonia.GetDescription(),
                DollarType.Comafi => Banks.Comafi.GetDescription(),
                DollarType.BIND => Banks.BIND.GetDescription(),
                DollarType.Bancor => Banks.Bancor.GetDescription(),
                DollarType.Chaco => Banks.Chaco.GetDescription(),
                DollarType.Pampa => Banks.Pampa.GetDescription(),
                _ => throw new ArgumentException($"Unable to get title from '{dollarResponse.Type}'.")
            };
        }

        #endregion
    }
}
