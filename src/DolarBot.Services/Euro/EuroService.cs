using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Banking;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EuroTypes = DolarBot.API.ApiCalls.DolarArgentinaApi.EuroTypes;

namespace DolarBot.Services.Euro
{
    /// <summary>
    /// Contains several methods to process Euro commands.
    /// </summary>
    public class EuroService : BaseCurrencyService
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="EuroService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public EuroService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        #region API Calls

        /// <summary>
        /// Fetches all available euro prices.
        /// </summary>
        /// <returns>An array of <see cref="EuroResponse"/> objects.</returns>
        public async Task<EuroResponse[]> GetAllEuroPrices()
        {
            return await Task.WhenAll(GetEuroNacion(),
                                      GetEuroGalicia(),
                                      GetEuroBBVA(),
                                      GetEuroHipotecario(),
                                      GetEuroChaco(),
                                      GetEuroPampa()).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches all available euro Ahorro prices.
        /// </summary>
        /// <returns>An array of <see cref="EuroResponse"/> objects.</returns>
        public async Task<EuroResponse[]> GetAllEuroAhorroPrices()
        {
            EuroResponse[] euroResponses = await GetAllEuroPrices().ConfigureAwait(false);
            return euroResponses.Select(x => (EuroResponse)ApplyTaxes(x)).ToArray();
        }

        /// <summary>
        /// Fetches the price for euro from bank Nacion.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroNacion()
        {
            return await Api.DolarArgentina.GetEuroPrice(EuroTypes.Nacion).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for euro from bank Galicia.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroGalicia()
        {
            return await Api.DolarArgentina.GetEuroPrice(EuroTypes.Galicia).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for euro from bank BBVA.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroBBVA()
        {
            return await Api.DolarArgentina.GetEuroPrice(EuroTypes.BBVA).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for euro from bank Hipotecario.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroHipotecario()
        {
            return await Api.DolarArgentina.GetEuroPrice(EuroTypes.Hipotecario).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for euro from bank Nuevo Banco del Chaco.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroChaco()
        {
            return await Api.DolarArgentina.GetEuroPrice(EuroTypes.Chaco).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for euro from bank Banco de La Pampa.
        /// </summary>
        /// <returns>A single <see cref="EuroResponse"/>.</returns>
        public async Task<EuroResponse> GetEuroPampa()
        {
            return await Api.DolarArgentina.GetEuroPrice(EuroTypes.Pampa).ConfigureAwait(false);
        }

        #endregion

        #region Embed

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
            Emoji buyEmoji = new Emoji(":regional_indicator_c:");
            Emoji sellEmoji = new Emoji(":regional_indicator_v:");

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Euro)
                                                   .WithTitle("Cotizaciones del Euro")
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($" C = Compra | V = Venta | {clockEmoji} = Última actualización ({localTimeZone.StandardName})");

            for (int i = 0; i < euroResponses.Length; i++)
            {
                EuroResponse response = euroResponses[i];
                string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
                string title = GetTitle(response);
                string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(response.Fecha, localTimeZone).ToString("dd/MM - HH:mm");
                string buyPrice = decimal.TryParse(response?.Compra, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal compra) ? compra.ToString("F2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellPrice = decimal.TryParse(response?.Venta, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal venta) ? venta.ToString("F2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

                if (buyPrice != "?" || sellPrice != "?")
                {
                    StringBuilder sbField = new StringBuilder()
                                            .AppendLine($"{euroEmoji} {blankSpace} {buyEmoji} {Format.Bold($"$ {buyPrice}")}")
                                            .AppendLine($"{euroEmoji} {blankSpace} {sellEmoji} {Format.Bold($"$ {sellPrice}")}")
                                            .AppendLine($"{clockEmoji} {blankSpace} {lastUpdated}");
                    embed.AddInlineField(title, sbField.AppendLineBreak().ToString());
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
            string buyPrice = decimal.TryParse(euroResponse?.Compra, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal compra) ? compra.ToString("F2", GlobalConfiguration.GetLocalCultureInfo()) : null;
            string sellPrice = decimal.TryParse(euroResponse?.Venta, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal venta) ? venta.ToString("F2", GlobalConfiguration.GetLocalCultureInfo()) : null;

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

        #endregion
    }
}
