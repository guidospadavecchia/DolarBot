using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Services.Banking;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Text;
using static DolarBot.API.ApiCalls.DolarArgentinaApi;

namespace DolarBot.Modules.Services.Real
{
    /// <summary>
    /// Contains several methods to process Real (Brazil) commands.
    /// </summary>
    public class RealService
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

        /// <summary>
        /// Color for the embed messages.
        /// </summary>
        private readonly Color EmbedColor;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="RealService"/> object with the provided configuration, api object and embed color.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="embedColor">The color to use for embed messages.</param>
        public RealService(IConfiguration configuration, ApiCalls api, Color embedColor)
        {
            Configuration = configuration;
            EmbedColor = embedColor;
            Api = api;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple Real responses.
        /// </summary>
        /// <param name="realResponses">The Real responses to show.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateRealEmbed(RealResponse[] realResponses)
        {
            string realImageUrl = Configuration.GetSection("images").GetSection("real")["64"];
            return CreateRealEmbed(realResponses, $"Cotizaciones disponibles del {Format.Bold("Real")} expresadas en {Format.Bold("pesos argentinos")}.", realImageUrl);
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple Real responses specifying a custom description and thumbnail URL.
        /// </summary>
        /// <param name="realResponses">The Real responses to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="thumbnailUrl">The URL of the embed's thumbnail image.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateRealEmbed(RealResponse[] realResponses, string description, string thumbnailUrl)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji realEmoji = new Emoji(emojis["real"]);
            Emoji clockEmoji = new Emoji("\u23F0");

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();

            EmbedBuilder embed = new EmbedBuilder().WithColor(EmbedColor)
                                                   .WithTitle("Cotizaciones del Real")
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($"{clockEmoji} = Última actualización ({localTimeZone.StandardName})");

            for (int i = 0; i < realResponses.Length; i++)
            {
                RealResponse response = realResponses[i];
                string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
                string title = GetTitle(response);
                string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(response.Fecha, localTimeZone).ToString("dd/MM - HH:mm");
                string buyPrice = decimal.TryParse(response?.Compra, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal compra) ? compra.ToString("F", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellPrice = decimal.TryParse(response?.Venta, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal venta) ? venta.ToString("F", GlobalConfiguration.GetLocalCultureInfo()) : "?";

                if (buyPrice != "?" || sellPrice != "?")
                {
                    StringBuilder sbField = new StringBuilder()
                                            .AppendLine($"{realEmoji} {blankSpace} Compra: {Format.Bold($"$ {buyPrice}")} {blankSpace}")
                                            .AppendLine($"{realEmoji} {blankSpace} Venta: {Format.Bold($"$ {sellPrice}")} {blankSpace}")
                                            .AppendLine($"{clockEmoji} {blankSpace} {lastUpdated} {blankSpace}  ");
                    embed.AddInlineField(title, sbField.ToString().AppendLineBreak());
                }
            }

            return embed;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a single Real response specifying a custom description, title and thumbnail URL.
        /// </summary>
        /// <param name="realResponse">>The Real response to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="title">Optional. The embed's title.</param>
        /// <param name="thumbnailUrl">Optional. The embed's thumbnail URL.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateRealEmbed(RealResponse realResponse, string description, string title = null, string thumbnailUrl = null)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji realEmoji = new Emoji(emojis["real"]);
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            string realImageUrl = thumbnailUrl ?? Configuration.GetSection("images").GetSection("real")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = title ?? GetTitle(realResponse);
            string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(realResponse.Fecha, localTimeZone).ToString(realResponse.Fecha.Date == DateTime.UtcNow.Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string buyPrice = decimal.TryParse(realResponse?.Compra, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal compra) ? compra.ToString("F", GlobalConfiguration.GetLocalCultureInfo()) : null;
            string sellPrice = decimal.TryParse(realResponse?.Venta, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal venta) ? venta.ToString("F", GlobalConfiguration.GetLocalCultureInfo()) : null;

            EmbedBuilder embed = new EmbedBuilder().WithColor(EmbedColor)
                                                   .WithTitle(embedTitle)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(realImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} ({localTimeZone.StandardName})", footerImageUrl)
                                                   .AddInlineField("Compra", Format.Bold($"{realEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {buyPrice}"))
                                                   .AddInlineField("Venta", Format.Bold($"{realEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {sellPrice}".AppendLineBreak()));
            return embed;
        }

        /// <summary>
        /// Returns the title depending on the response type.
        /// </summary>
        /// <param name="realResponse">The Real response.</param>
        /// <returns>The corresponding title.</returns>
        private string GetTitle(RealResponse realResponse)
        {
            return realResponse.Type switch
            {
                RealTypes.Nacion => Banks.Nacion.GetDescription(),
                RealTypes.BBVA => Banks.BBVA.GetDescription(),
                RealTypes.Chaco => Banks.Chaco.GetDescription(),
                _ => throw new ArgumentException($"Unable to get title from '{realResponse.Type}'.")
            };
        }

        #endregion
    }
}
