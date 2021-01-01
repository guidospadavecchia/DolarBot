using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Banking;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DollarTypes = DolarBot.API.ApiCalls.DolarArgentinaApi.DollarTypes;

namespace DolarBot.Services.Dolar
{
    /// <summary>
    /// Contains several methods to process dollar commands.
    /// </summary>
    public class DolarService : BaseCurrencyService
    {
        #region Constants
        private const string DOLAR_OFICIAL_TITLE = "Dólar Oficial";
        private const string DOLAR_AHORRO_TITLE = "Dólar Ahorro";
        private const string DOLAR_BLUE_TITLE = "Dólar Blue";
        private const string DOLAR_BOLSA_TITLE = "Dólar Bolsa (MEP)";
        private const string DOLAR_PROMEDIO_TITLE = "Dólar Promedio";
        private const string DOLAR_CCL_TITLE = "Contado con Liqui";
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="DolarService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public DolarService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override Banks[] GetValidBanks()
        {
            return Enum.GetValues(typeof(Banks)).Cast<Banks>().ToArray();
        }

        #region API Calls

        /// <summary>
        /// Fetches all oficial dollar rates from <see cref="Banks"/>.
        /// </summary>
        /// <returns>An array of <see cref="DolarResponse"/> objects.</returns>
        public async Task<DolarResponse[]> GetAllBankRates()
        {
            List<Banks> banks = Enum.GetValues(typeof(Banks)).Cast<Banks>().Where(b => b != Banks.Bancos).ToList();
            Task<DolarResponse>[] tasks = new Task<DolarResponse>[banks.Count];
            for (int i = 0; i < banks.Count; i++)
            {
                DollarTypes dollarType = ConvertToDollarType(banks[i]);
                tasks[i] = Api.DolarArgentina.GetDollarPrice(dollarType);
            }

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches all Ahorro dollar rates from <see cref="Banks"/>.
        /// </summary>
        /// <returns>An array of <see cref="DolarResponse"/> objects.</returns>
        public async Task<DolarResponse[]> GetAllAhorroBankRates()
        {
            List<Banks> banks = Enum.GetValues(typeof(Banks)).Cast<Banks>().Where(b => b != Banks.Bancos).ToList();
            Task<DolarResponse>[] tasks = new Task<DolarResponse>[banks.Count];
            for (int i = 0; i < banks.Count; i++)
            {
                DollarTypes dollarType = ConvertToDollarType(banks[i]);
                tasks[i] = Api.DolarArgentina.GetDollarPrice(dollarType);
            }

            DolarResponse[] dolarResponses = await Task.WhenAll(tasks).ConfigureAwait(false);
            return dolarResponses.Select(x => (DolarResponse)ApplyTaxes(x)).ToArray();
        }

        /// <summary>
        /// Fetches all available dollar prices.
        /// </summary>
        /// <returns>An array of <see cref="DolarResponse"/> objects.</returns>
        public async Task<DolarResponse[]> GetAllDollarRates()
        {
            return await Task.WhenAll(GetDollarOficial(),
                                      GetDollarAhorro(),
                                      GetDollarBlue(),
                                      GetDollarBolsa(),
                                      GetDollarPromedio(),
                                      GetDollarContadoConLiqui()).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the official dollar rate for the specified bank.
        /// </summary>
        /// <param name="bank">The bank who's rate is to be retrieved.</param>
        /// <returns>A single <see cref="DolarResponse"/>.</returns>
        public async Task<DolarResponse> GetByBank(Banks bank)
        {
            DollarTypes dollarType = ConvertToDollarType(bank);
            return await Api.DolarArgentina.GetDollarPrice(dollarType).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the dollar Ahorro rate for the specified bank.
        /// </summary>
        /// <param name="bank">The bank who's rate is to be retrieved.</param>
        /// <returns>A single <see cref="DolarResponse"/>.</returns>
        public async Task<DolarResponse> GetDollarAhorroByBank(Banks bank)
        {
            DollarTypes dollarType = ConvertToDollarType(bank);
            DolarResponse dolarResponse = await Api.DolarArgentina.GetDollarPrice(dollarType).ConfigureAwait(false);
            dolarResponse = (DolarResponse)ApplyTaxes(dolarResponse);
            if (dolarResponse != null)
            {
                dolarResponse.Type = DollarTypes.Ahorro;
            }
            return dolarResponse;
        }

        /// <summary>
        /// Fetches the price for dollar Oficial.
        /// </summary>
        /// <returns>A single <see cref="DolarResponse"/>.</returns>
        public async Task<DolarResponse> GetDollarOficial()
        {
            return await Api.DolarArgentina.GetDollarPrice(DollarTypes.Oficial).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for dollar Ahorro.
        /// </summary>
        /// <returns>A single <see cref="DolarResponse"/>.</returns>
        public async Task<DolarResponse> GetDollarAhorro()
        {
            DolarResponse dolarResponse = await Api.DolarArgentina.GetDollarPrice(DollarTypes.Oficial).ConfigureAwait(false);
            dolarResponse = (DolarResponse)ApplyTaxes(dolarResponse);
            if (dolarResponse != null)
            {
                dolarResponse.Type = DollarTypes.Ahorro;
            }
            return dolarResponse;
        }

        /// <summary>
        /// Fetches the price for dollar Blue.
        /// </summary>
        /// <returns>A single <see cref="DolarResponse"/>.</returns>
        public async Task<DolarResponse> GetDollarBlue()
        {
            return await Api.DolarArgentina.GetDollarPrice(DollarTypes.Blue).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for dollar Promedio.
        /// </summary>
        /// <returns>A single <see cref="DolarResponse"/>.</returns>
        public async Task<DolarResponse> GetDollarPromedio()
        {
            return await Api.DolarArgentina.GetDollarPrice(DollarTypes.Promedio).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for dollar Bolsa.
        /// </summary>
        /// <returns>A single <see cref="DolarResponse"/>.</returns>
        public async Task<DolarResponse> GetDollarBolsa()
        {
            return await Api.DolarArgentina.GetDollarPrice(DollarTypes.Bolsa).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for dollar Contado con Liquidación.
        /// </summary>
        /// <returns>A single <see cref="DolarResponse"/>.</returns>
        public async Task<DolarResponse> GetDollarContadoConLiqui()
        {
            return await Api.DolarArgentina.GetDollarPrice(DollarTypes.ContadoConLiqui).ConfigureAwait(false);
        }

        #endregion

        #region Embeds

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple dollar responses.
        /// </summary>
        /// <param name="dollarResponses">The dollar responses to show.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateDollarEmbed(DolarResponse[] dollarResponses)
        {
            string dollarImageUrl = Configuration.GetSection("images").GetSection("dollar")["64"];
            return CreateDollarEmbed(dollarResponses, $"Cotizaciones disponibles del {Format.Bold("dólar")} expresadas en {Format.Bold("pesos argentinos")}.", dollarImageUrl);
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
            var emojis = Configuration.GetSection("customEmojis");
            Emoji dollarEmoji = new Emoji("\uD83D\uDCB5");
            Emoji clockEmoji = new Emoji("\u23F0");
            Emoji buyEmoji = new Emoji(emojis["buyGreen"]);
            Emoji sellEmoji = new Emoji(emojis["sellGreen"]);

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle("Cotizaciones del Dólar")
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($" C = Compra | V = Venta | {clockEmoji} = Última actualización ({localTimeZone.StandardName})");

            for (int i = 0; i < dollarResponses.Length; i++)
            {
                DolarResponse response = dollarResponses[i];
                string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
                string title = GetTitle(response.Type);
                string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(response.Fecha, localTimeZone).ToString("dd/MM - HH:mm");
                string buyPrice = decimal.TryParse(response?.Compra, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal compra) ? compra.ToString("F2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellPrice = decimal.TryParse(response?.Venta, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal venta) ? venta.ToString("F2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

                if (buyPrice != "?" || sellPrice != "?")
                {
                    StringBuilder sbField = new StringBuilder()
                                            .AppendLine($"{dollarEmoji} {blankSpace} {buyEmoji} {Format.Bold($"$ {buyPrice}")}")
                                            .AppendLine($"{dollarEmoji} {blankSpace} {sellEmoji} {Format.Bold($"$ {sellPrice}")}")
                                            .AppendLine($"{clockEmoji} {blankSpace} {lastUpdated}");
                    embed.AddInlineField(title, sbField.AppendLineBreak().ToString());
                }
            }

            return embed;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a single dollar response specifying a custom description, title and thumbnail URL.
        /// </summary>
        /// <param name="dollarResponse">The dollar response to show.</param>
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
            string embedTitle = title ?? GetTitle(dollarResponse.Type);
            string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(dollarResponse.Fecha, localTimeZone).ToString(dollarResponse.Fecha.Date == DateTime.UtcNow.Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string buyPrice = decimal.TryParse(dollarResponse?.Compra, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal compra) ? compra.ToString("F2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string sellPrice = decimal.TryParse(dollarResponse?.Venta, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal venta) ? venta.ToString("F2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle(embedTitle)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(dollarImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} ({localTimeZone.StandardName})", footerImageUrl)
                                                   .AddInlineField("Compra", Format.Bold($"{dollarEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {buyPrice}"))
                                                   .AddInlineField("Venta", Format.Bold($"{dollarEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {sellPrice}".AppendLineBreak()));
            return embed;
        }

        /// <summary>
        /// Returns the title depending on the dollar type.
        /// </summary>
        /// <param name="dollarType">The dollar type.</param>
        /// <returns>The corresponding title.</returns>
        private string GetTitle(DollarTypes dollarType)
        {
            return dollarType switch
            {
                DollarTypes.Oficial => DOLAR_OFICIAL_TITLE,
                DollarTypes.Ahorro => DOLAR_AHORRO_TITLE,
                DollarTypes.Blue => DOLAR_BLUE_TITLE,
                DollarTypes.Bolsa => DOLAR_BOLSA_TITLE,
                DollarTypes.Promedio => DOLAR_PROMEDIO_TITLE,
                DollarTypes.ContadoConLiqui => DOLAR_CCL_TITLE,
                DollarTypes.Nacion => Banks.Nacion.GetDescription(),
                DollarTypes.BBVA => Banks.BBVA.GetDescription(),
                DollarTypes.Piano => Banks.Piano.GetDescription(),
                DollarTypes.Hipotecario => Banks.Hipotecario.GetDescription(),
                DollarTypes.Galicia => Banks.Galicia.GetDescription(),
                DollarTypes.Santander => Banks.Santander.GetDescription(),
                DollarTypes.Ciudad => Banks.Ciudad.GetDescription(),
                DollarTypes.Supervielle => Banks.Supervielle.GetDescription(),
                DollarTypes.Patagonia => Banks.Patagonia.GetDescription(),
                DollarTypes.Comafi => Banks.Comafi.GetDescription(),
                DollarTypes.BIND => Banks.BIND.GetDescription(),
                DollarTypes.Bancor => Banks.Bancor.GetDescription(),
                DollarTypes.Chaco => Banks.Chaco.GetDescription(),
                DollarTypes.Pampa => Banks.Pampa.GetDescription(),
                _ => throw new ArgumentException($"Unable to get title from '{dollarType}'.")
            };
        }

        #endregion

        /// <summary>
        /// Converts a <see cref="Banks"/> object to its <see cref="DollarTypes"/> equivalent.
        /// </summary>
        /// <param name="bank">The value to convert.</param>
        /// <returns>The converted value as <see cref="DollarTypes"/>.</returns>
        public DollarTypes ConvertToDollarType(Banks bank)
        {
            return bank switch
            {
                Banks.Nacion => DollarTypes.Nacion,
                Banks.BBVA => DollarTypes.BBVA,
                Banks.Piano => DollarTypes.Piano,
                Banks.Hipotecario => DollarTypes.Hipotecario,
                Banks.Galicia => DollarTypes.Galicia,
                Banks.Santander => DollarTypes.Santander,
                Banks.Ciudad => DollarTypes.Ciudad,
                Banks.Supervielle => DollarTypes.Supervielle,
                Banks.Patagonia => DollarTypes.Patagonia,
                Banks.Comafi => DollarTypes.Comafi,
                Banks.BIND => DollarTypes.BIND,
                Banks.Bancor => DollarTypes.Bancor,
                Banks.Chaco => DollarTypes.Chaco,
                Banks.Pampa => DollarTypes.Pampa,
                _ => DollarTypes.Oficial,
            };
        }

        #endregion
    }
}
