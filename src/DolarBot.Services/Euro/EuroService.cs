using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Banking;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Text;
using static DolarBot.API.ApiCalls.DolarArgentinaApi;

namespace DolarBot.Services.Euro
{
    /// <summary>
    /// Contains several methods to process Euro commands.
    /// </summary>
    public class EuroService
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
        /// Creates a new <see cref="EuroService"/> object with the provided configuration, api object and embed color.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public EuroService(IConfiguration configuration, ApiCalls api)
        {
            Configuration = configuration;
            Api = api;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple euro responses.
        /// </summary>
        /// <param name="euroResponses">The euro responses to show.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateEuroEmbed(EuroResponse[] euroResponses)
        {
            string euroImageUrl = Configuration.GetSection("images").GetSection("euro")["64"];
            return CreateEuroEmbed(euroResponses, $"Cotizaciones disponibles del {Format.Bold("Euro")} expresadas en {Format.Bold("pesos argentinos")}.", euroImageUrl);
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple euro responses with taxes included.
        /// </summary>
        /// <param name="euroResponses">The euro responses to show.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateEuroAhorroEmbed(EuroResponse[] euroResponses)
        {
            CultureInfo apiCulture = Api.DolarArgentina.GetApiCulture();
            string euroImageUrl = Configuration.GetSection("images").GetSection("euro")["64"];

            foreach (EuroResponse euroResponse in euroResponses)
            {
                decimal taxPercent = (decimal.Parse(Configuration["taxPercent"]) / 100) + 1;
                if (decimal.TryParse(euroResponse.Venta, NumberStyles.Any, apiCulture, out decimal venta))
                {
                    euroResponse.Venta = Convert.ToDecimal(venta * taxPercent, apiCulture).ToString("F", apiCulture);
                }
            }

            return CreateEuroEmbed(euroResponses, $"Cotizaciones disponibles del {Format.Bold("Euro")} incluyendo impuesto P.A.I.S. y retención de ganancias, expresadas en {Format.Bold("pesos argentinos")}.", euroImageUrl);
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple euro responses specifying a custom description and thumbnail URL.
        /// </summary>
        /// <param name="euroResponses">The euro responses to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="thumbnailUrl">The URL of the embed's thumbnail image.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateEuroEmbed(EuroResponse[] euroResponses, string description, string thumbnailUrl)
        {
            Emoji euroEmoji = new Emoji(":euro:");
            Emoji clockEmoji = new Emoji("\u23F0");

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Euro)
                                                   .WithTitle("Cotizaciones del Euro")
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($"{clockEmoji} = Última actualización ({localTimeZone.StandardName})");

            for (int i = 0; i < euroResponses.Length; i++)
            {
                EuroResponse response = euroResponses[i];
                string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
                string title = GetTitle(response);
                string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(response.Fecha, localTimeZone).ToString("dd/MM - HH:mm");
                string buyPrice = decimal.TryParse(response?.Compra, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal compra) ? compra.ToString("F", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellPrice = decimal.TryParse(response?.Venta, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal venta) ? venta.ToString("F", GlobalConfiguration.GetLocalCultureInfo()) : "?";

                if (buyPrice != "?" || sellPrice != "?")
                {
                    StringBuilder sbField = new StringBuilder()
                                            .AppendLine($"{euroEmoji} {blankSpace} Compra: {Format.Bold($"$ {buyPrice}")}")
                                            .AppendLine($"{euroEmoji} {blankSpace} Venta: {Format.Bold($"$ {sellPrice}")}")
                                            .AppendLine($"{clockEmoji} {blankSpace} {lastUpdated} {blankSpace}  ");
                    embed.AddInlineField(title, sbField.ToString().AppendLineBreak());
                }
            }

            return embed;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a single euro response specifying a custom description, title and thumbnail URL.
        /// </summary>
        /// <param name="euroResponse">>The euro response to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="title">Optional. The embed's title.</param>
        /// <param name="thumbnailUrl">Optional. The embed's thumbnail URL.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateEuroEmbed(EuroResponse euroResponse, string description, string title = null, string thumbnailUrl = null)
        {
            Emoji euroEmoji = new Emoji(":euro:");
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            string euroImageUrl = thumbnailUrl ?? Configuration.GetSection("images").GetSection("euro")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = title ?? GetTitle(euroResponse);
            string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(euroResponse.Fecha, localTimeZone).ToString(euroResponse.Fecha.Date == DateTime.UtcNow.Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string buyPrice = decimal.TryParse(euroResponse?.Compra, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal compra) ? compra.ToString("F", GlobalConfiguration.GetLocalCultureInfo()) : null;
            string sellPrice = decimal.TryParse(euroResponse?.Venta, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal venta) ? venta.ToString("F", GlobalConfiguration.GetLocalCultureInfo()) : null;

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Euro)
                                                   .WithTitle(embedTitle)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(euroImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} ({localTimeZone.StandardName})", footerImageUrl)
                                                   .AddInlineField("Compra", Format.Bold($"{euroEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {buyPrice}"))
                                                   .AddInlineField("Venta", Format.Bold($"{euroEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {sellPrice}".AppendLineBreak()));
            return embed;
        }

        /// <summary>
        /// Returns the title depending on the response type.
        /// </summary>
        /// <param name="euroResponse">The euro response.</param>
        /// <returns>The corresponding title.</returns>
        private string GetTitle(EuroResponse euroResponse)
        {
            return euroResponse.Type switch
            {
                EuroTypes.Nacion => Banks.Nacion.GetDescription(),
                EuroTypes.Galicia => Banks.Galicia.GetDescription(),
                EuroTypes.BBVA => Banks.BBVA.GetDescription(),
                EuroTypes.Hipotecario => Banks.Hipotecario.GetDescription(),
                EuroTypes.Chaco => Banks.Chaco.GetDescription(),
                EuroTypes.Pampa => Banks.Pampa.GetDescription(),
                _ => throw new ArgumentException($"Unable to get title from '{euroResponse.Type}'.")
            };
        }

        #endregion
    }
}
