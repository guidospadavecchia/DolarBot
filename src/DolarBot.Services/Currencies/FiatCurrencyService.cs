using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.API.Services.DolarBotApi;
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

namespace DolarBot.Services.Currencies
{
    /// <summary>
    /// Contains several methods to process world currency related commands.
    /// </summary>
    public class FiatCurrencyService : BaseService
    {
        #region Constants
        /// <summary>
        /// How many currencies to fit into each embed page.
        /// </summary>
        private const int CURRENCIES_PER_PAGE = 25;
        /// <summary>
        /// How many days to fit into each embed page for historical commands.
        /// </summary>
        private const int HISTORICAL_DATES_PER_PAGE = 15;
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="FiatCurrencyService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public FiatCurrencyService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        #region API Calls

        /// <summary>
        /// Fetches all the available world currency codes.
        /// </summary>
        /// <returns>A collection of <see cref="WorldCurrencyCodeResponse"/> objects.</returns>
        public async Task<List<WorldCurrencyCodeResponse>> GetWorldCurrenciesList()
        {
            return await Api.DolarBot.GetWorldCurrenciesList();
        }

        /// <summary>
        /// Fetches a single currency rate.
        /// </summary>
        /// <param name="currencyCode">The currency 3-digit code.</param>
        /// <returns>A <see cref="WorldCurrencyResponse"/> object.</returns>
        public async Task<WorldCurrencyResponse> GetCurrencyValue(string currencyCode)
        {
            return await Api.DolarBot.GetWorldCurrencyValue(currencyCode);
        }

        /// <summary>
        /// Fetches a collection of historical currency values between two dates.
        /// </summary>
        /// <param name="currencyCode">The currency 3-digit code.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>A collection of <see cref="WorldCurrencyResponse"/> objects.</returns>
        public async Task<List<WorldCurrencyResponse>> GetHistoricalCurrencyValues(string currencyCode, DateTime? startDate = null, DateTime? endDate = null)
        {
            List<WorldCurrencyResponse> historicalCurrencyValues = await Api.DolarBot.GetWorldCurrencyHistoricalValues(currencyCode);
            if ((startDate != null && startDate.Value.Date <= DateTime.Now.Date) || (endDate != null && endDate.Value.Date <= DateTime.Now.Date))
            {
                historicalCurrencyValues = historicalCurrencyValues.Where(x => (!startDate.HasValue || x.Fecha.Date >= startDate.Value.Date) && (!endDate.HasValue || x.Fecha.Date <= endDate.Value.Date)).ToList();
            }
            return historicalCurrencyValues;
        }

        #endregion

        #region Embeds

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="WorldCurrencyResponse"/>.
        /// </summary>
        /// <param name="worldCurrencyResponse">The world currency response.</param>
        /// <param name="currencyName">The currency name.</param>
        /// <param name="amount">The amount to rate against.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public async Task<EmbedBuilder> CreateWorldCurrencyEmbedAsync(WorldCurrencyResponse worldCurrencyResponse, string currencyName, decimal amount = 1)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji currencyEmoji = Emoji.Parse(":flag_ar:");
            Emoji whatsappEmoji = new(emojis["whatsapp"]);
            Emoji amountEmoji = Emoji.Parse(":moneybag:");
            string currencyImageUrl = Configuration.GetSection("images").GetSection("coins")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];

            string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;
            string lastUpdated = worldCurrencyResponse.Fecha.ToString(worldCurrencyResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string amountField = Format.Bold($"{amountEmoji} {blankSpace} {amount} {worldCurrencyResponse.Code}");

            decimal? valuePrice = decimal.TryParse(worldCurrencyResponse?.Valor, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal v) ? v * amount : null;
            string value = valuePrice.HasValue ? valuePrice.Value.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

            string shareText = $"*{currencyName} ({worldCurrencyResponse.Code})*{Environment.NewLine}{Environment.NewLine}*{amount} {worldCurrencyResponse.Code}*{Environment.NewLine}Valor: \t$ *{value}*{Environment.NewLine}Hora: \t{lastUpdated} (UTC {utcOffset})";
            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Currency)
                                                   .WithTitle($"{currencyName} ({worldCurrencyResponse.Code})")
                                                   .WithDescription($"Cotización de {Format.Bold($"{currencyName} ({worldCurrencyResponse.Code})")} expresada en {Format.Bold("pesos argentinos")}.".AppendLineBreak())
                                                   .WithThumbnailUrl(currencyImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {lastUpdated} (UTC {utcOffset})",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField("Monto", amountField)
                                                   .AddInlineField($"Valor", $"{Format.Bold($"{currencyEmoji} ${blankSpace} {value.AppendLineBreak()}")}");

            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText);
            return embed.AddPlayStoreLink(Configuration);
        }

        /// <summary>
        /// Creates a collection of <see cref="EmbedBuilder"/> objects representing a list of currency codes.
        /// </summary>
        /// <param name="currenciesList">A collection of <see cref="WorldCurrencyCodeResponse"/> objects.</param>
        /// <param name="currencyCommand">The executing currency command.</param>
        /// <param name="username">The executing user name.</param>
        /// <param name="interactive">Indicates whether the embed message allows interactivity.</param>
        /// <returns>A list of embeds ready to be built.</returns>
        public List<EmbedBuilder> CreateWorldCurrencyListEmbedAsync(List<WorldCurrencyCodeResponse> currenciesList, string currencyCommand, string username, bool interactive = false)
        {
            int replyTimeout = Convert.ToInt32(Configuration["interactiveMessageReplyTimeout"]);
            Emoji coinEmoji = new(":coin:");
            string coinsImageUrl = Configuration.GetSection("images").GetSection("coins")["64"];
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();

            int pageCount = 0;
            int totalPages = (int)Math.Ceiling(Convert.ToDecimal(currenciesList.Count) / CURRENCIES_PER_PAGE);
            List<IEnumerable<WorldCurrencyCodeResponse>> currenciesListPages = currenciesList.ChunkBy(CURRENCIES_PER_PAGE);

            List<EmbedBuilder> embeds = new();
            foreach (IEnumerable<WorldCurrencyCodeResponse> currenciesPage in currenciesListPages)
            {
                string currencyList = string.Join(Environment.NewLine, currenciesPage.Select(x => $"{coinEmoji} {Format.Code(x.Code)}: {Format.Italics(x.Name)}."));
                EmbedBuilder embed = new EmbedBuilder()
                                     .WithColor(GlobalConfiguration.Colors.Currency)
                                     .WithThumbnailUrl(coinsImageUrl)
                                     .WithTitle("Monedas del mundo disponibles")
                                     .WithDescription($"Códigos de monedas disponibles para utilizar como parámetro del comando {Format.Code($"/{currencyCommand}")}.")
                                     .WithFooter($"Página {++pageCount} de {totalPages}")
                                     .AddField(GlobalConfiguration.Constants.BLANK_SPACE, currencyList);
                if (interactive)
                {
                    embed.AddField(GlobalConfiguration.Constants.BLANK_SPACE, $"{Format.Bold(username)}, para ver una cotización, respondé a este mensaje antes de las {Format.Bold(TimeZoneInfo.ConvertTime(DateTime.Now.AddSeconds(replyTimeout), localTimeZone).ToString("HH:mm:ss"))} con el {Format.Bold("código de 3 dígitos")} de la moneda.{Environment.NewLine}Por ejemplo: {Format.Code(currenciesList.First().Code)}.")
                         .AddField(GlobalConfiguration.Constants.BLANK_SPACE, $"{Format.Bold("Tip")}: {Format.Italics("Si ya sabés el código de la moneda, podés indicárselo al comando directamente, por ejemplo:")} {Format.Code($"/{currencyCommand} {currenciesList.First().Code}")}.");
                }
                else
                {
                    embed.AddField(GlobalConfiguration.Constants.BLANK_SPACE, $"{Format.Bold("Tip")}: {Format.Italics("Para ver una cotización, indica el código de la moneda. Por ejemplo:")} {Format.Code($"/{currencyCommand} {currenciesList.First().Code}")}.");
                }
                embeds.Add(embed);
            }

            return embeds;
        }

        /// <summary>
        /// Creates a collection of <see cref="EmbedBuilder"/> objects representing historical values.
        /// </summary>
        /// <param name="historicalCurrencyValues">A collection of <see cref="WorldCurrencyResponse"/> objects.</param>
        /// <param name="currencyName">The currency name.</param>
        /// <returns>A list of embeds ready to be built.</returns>
        public List<EmbedBuilder> CreateHistoricalValuesEmbedsAsync(List<WorldCurrencyResponse> historicalCurrencyValues, string currencyName, DateTime? startDate, DateTime? endDate)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji upEmoji = new(emojis["arrowUpRed"]);
            Emoji downEmoji = new(emojis["arrowDownGreen"]);
            Emoji neutralEmoji = new(emojis["neutral"]);
            Emoji calendarEmoji = new(":calendar_spiral:");
            string chartImageUrl = Configuration.GetSection("images").GetSection("chart")["64"];

            int pageCount = 0;
            int totalPages = (int)Math.Ceiling(Convert.ToDecimal(historicalCurrencyValues.Count) / HISTORICAL_DATES_PER_PAGE);
            List<IEnumerable<WorldCurrencyResponse>> historicalCurrencyValuesPages = historicalCurrencyValues.ChunkBy(HISTORICAL_DATES_PER_PAGE);

            if (!startDate.HasValue)
            {
                startDate = historicalCurrencyValues.First().Fecha.Date;
            }
            if (!endDate.HasValue || endDate.Value.Date > DateTime.Now.Date)
            {
                endDate = DateTime.Now.Date;
            }
            bool isSingleDate = startDate.Value.Date == endDate.Value.Date;

            List<EmbedBuilder> embeds = new();
            for (int i = 0; i < historicalCurrencyValuesPages.Count; i++)
            {
                IEnumerable<WorldCurrencyResponse> page = historicalCurrencyValuesPages.ElementAt(i);

                StringBuilder sbField = new();
                for (int j = 0; j < page.Count(); j++)
                {
                    WorldCurrencyResponse currencyValue = page.ElementAt(j);
                    bool rateIsNumeric = decimal.TryParse(currencyValue.Valor, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal currencyRate);

                    Emoji fieldEmoji = isSingleDate ? calendarEmoji : neutralEmoji;
                    if (i > 0 || j > 0)
                    {
                        WorldCurrencyResponse previousCurrencyValue = j > 0 ? page.ElementAt(j - 1) : historicalCurrencyValuesPages.ElementAt(i - 1).Last();
                        bool previousValueIsNumeric = decimal.TryParse(previousCurrencyValue.Valor, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal previousRate);
                        if (rateIsNumeric && previousValueIsNumeric)
                        {
                            if (currencyRate >= previousRate)
                            {
                                fieldEmoji = currencyRate > previousRate ? upEmoji : neutralEmoji;
                            }
                            else
                            {
                                fieldEmoji = downEmoji;
                            }
                        }
                    }
                    string fieldText = $"{fieldEmoji} {Format.Code(currencyValue.Fecha.ToString("dd/MM/yyyy"))}: {Format.Bold($"$ {currencyRate}")}";
                    sbField = sbField.AppendLine(fieldText);
                }

                string worldCurrencyCode = historicalCurrencyValues.First().Code;
                string embedTitle = isSingleDate ? "Cotización por fecha" : "Cotizaciones por Fecha";
                string embedDescription = $"{(isSingleDate ? "Cotización oficial" : "Cotizaciones oficiales")} de {Format.Bold($"{currencyName} ({worldCurrencyCode})")} {(isSingleDate ? $"para el día {Format.Code(startDate.Value.Date.ToString("dd/MM/yyyy"))}" : $"entre el {Format.Code(startDate.Value.Date.ToString("dd/MM/yyyy"))} y el {Format.Code(endDate.Value.Date.ToString("dd/MM/yyyy"))}")}, expresada en {Format.Bold("pesos argentinos")}.".AppendLineBreak();
                EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Currency)
                                                       .WithTitle($"{currencyName} ({worldCurrencyCode})")
                                                       .WithDescription(embedDescription)
                                                       .WithThumbnailUrl(chartImageUrl)
                                                       .WithFooter($"Página {++pageCount} de {totalPages}")
                                                       .AddField(embedTitle, sbField.AppendLineBreak().ToString());
                embeds.Add(embed);
            }

            return embeds;
        }

        #endregion

        #endregion
    }
}
