using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DollarType = DolarBot.API.ApiCalls.DolarArgentinaApi.DollarType;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains the main theme related commands.
    /// </summary>
    [HelpOrder(1)]
    [HelpTitle("Cotizaciones")]
    public class MainModule : BaseInteractiveModule
    {
        #region Constants
        private const string DOLAR_OFICIAL_TITLE = "Dólar Oficial";
        private const string DOLAR_AHORRO_TITLE = "Dólar Ahorro";
        private const string DOLAR_BLUE_TITLE = "Dólar Blue";
        private const string DOLAR_BOLSA_TITLE = "Dólar Bolsa (MEP)";
        private const string DOLAR_PROMEDIO_TITLE = "Dólar Promedio";
        private const string DOLAR_CCL_TITLE = "Contado con Liqui";
        private const string DOLAR_BANCO_NACION = "Banco Nación";
        private const string DOLAR_BBVA_TITLE = "Banco BBVA";
        private const string DOLAR_PIANO_TITLE = "Banco Piano";
        private const string DOLAR_HIPOTECARIO_TITLE = "Banco Hipotecario";
        private const string DOLAR_GALICIA_TITLE = "Banco Galicia";
        private const string DOLAR_SANTANDER_TITLE = "Banco Santander";
        private const string DOLAR_CIUDAD_TITLE = "Banco Ciudad";
        private const string DOLAR_SUPERVIELLE_TITLE = "Banco Supervielle";
        private const string DOLAR_PATAGONIA_TITLE = "Banco Patagonia";
        private const string DOLAR_COMAFI_TITLE = "Banco Comafi";
        private const string REQUEST_ERROR_MESSAGE = "Error: No se pudo obtener la cotización. Intente nuevamente en más tarde.";
        #endregion

        #region Vars
        /// <summary>
        /// Represents the available bank parameters for dollar command.
        /// </summary>
        private enum BankCommandType
        {
            [Description("Todos los bancos")]
            Bancos,
            [Description("Banco Nación")]
            Nacion,
            [Description("Banco BBVA")]
            BBVA,
            [Description("Banco Piano")]
            Piano,
            [Description("Banco Hipotecario")]
            Hipotecario,
            [Description("Banco Galicia")]
            Galicia,
            [Description("Banco Santander")]
            Santander,
            [Description("Banco Ciudad")]
            Ciudad,
            [Description("Banco Supervielle")]
            Supervielle,
            [Description("Banco Patagonia")]
            Patagonia,
            [Description("Banco Comafi")]
            Comafi
        }

        /// <summary>
        /// Color for the embed messages.
        /// </summary>
        private readonly Color mainEmbedColor = new Color(67, 181, 129);

        /// <summary>
        /// Provides access to the different APIs.
        /// </summary>
        protected readonly ApiCalls Api;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public MainModule(IConfiguration configuration, ApiCalls api) : base(configuration)
        {
            Api = api;
        }
        #endregion

        [Command("bancos")]
        [Alias("b")]
        [Summary("Muestra la lista de bancos disponibles para obtener las cotizaciones.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetBanks()
        {
            string commandPrefix = Configuration["commandPrefix"];
            string banks = string.Join(", ", Enum.GetNames(typeof(BankCommandType)).Select(b => Format.Bold(b)));
            await ReplyAsync($"Parámetros disponibles del comando {Format.Code($"{commandPrefix}dolar <banco>")}: {banks}.").ConfigureAwait(false);
        }

        [Command("dolar", RunMode = RunMode.Async)]
        [Alias("d")]
        [Summary("Muestra todas las cotizaciones del dólar disponibles o por banco.")]
        [HelpUsageExample(false, "$dolar", "$d", "$dolar bancos", "$dolar santander", "$d galicia")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarPriceAsync(
            [Summary("Indica la cotización del banco a mostrar. Los valores posibles son aquellos devueltos por el comando `$bancos`.")]
            string banco = null)
        {
            using (Context.Channel.EnterTypingState())
            {
                if (banco != null)
                {
                    if (Enum.TryParse(banco, true, out BankCommandType bankCommandType))
                    {
                        if (bankCommandType == BankCommandType.Bancos)
                        {
                            //Show all private banks prices
                            List<BankCommandType> banks = Enum.GetValues(typeof(BankCommandType)).Cast<BankCommandType>().Where(b => b != BankCommandType.Bancos && b != BankCommandType.Nacion).ToList();
                            Task<DolarResponse>[] tasks = new Task<DolarResponse>[banks.Count];
                            for (int i = 0; i < banks.Count; i++)
                            {
                                DollarType dollarType = GetBankInformation(banks[i], out string _);
                                tasks[i] = Api.DolarArgentina.GetDollarPrice(dollarType);
                            }

                            DolarResponse[] responses = await Task.WhenAll(tasks).ConfigureAwait(false);
                            if (responses.Any(r => r != null))
                            {
                                string thumbnailUrl = Configuration.GetSection("images").GetSection("bank")["64"];
                                DolarResponse[] successfulResponses = responses.Where(r => r != null).ToArray();
                                EmbedBuilder embed = CreateDollarEmbed(successfulResponses, $"Cotizaciones de {Format.Bold("bancos privados")} expresados en {Format.Bold("pesos argentinos")}.", thumbnailUrl);
                                if (responses.Length != successfulResponses.Length)
                                {
                                    await ReplyAsync($"{Format.Bold("Atención")}: No se pudieron obtener algunas cotizaciones. Sólo se mostrarán aquellas que no presentan errores.").ConfigureAwait(false);
                                }
                                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                            }
                            else
                            {
                                await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                            }
                        }
                        else
                        {   //Show individual bank price
                            DollarType dollarType = GetBankInformation(bankCommandType, out string thumbnailUrl);
                            DolarResponse result = await Api.DolarArgentina.GetDollarPrice(dollarType).ConfigureAwait(false);
                            if (result != null)
                            {
                                EmbedBuilder embed = CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar oficial")} del {Format.Bold($"Banco {dollarType}")} expresada en {Format.Bold("pesos argentinos")}.", bankCommandType == BankCommandType.Nacion ? DOLAR_BANCO_NACION : null, thumbnailUrl);
                                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                            }
                            else
                            {
                                await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                            }
                        }
                    }
                    else
                    {   //Unknown parameter
                        string commandPrefix = Configuration["commandPrefix"];
                        string bankCommand = GetType().GetMethod("GetBanks").GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
                        await ReplyAsync($"Banco '{Format.Bold(banco)}' inexistente. Verifique los bancos disponibles con {Format.Code($"{commandPrefix}{bankCommand}")}.").ConfigureAwait(false);
                    }
                }
                else
                {   //Show all dollar types (not banks)
                    DolarResponse[] responses = await Task.WhenAll(Api.DolarArgentina.GetDollarPrice(DollarType.Oficial),
                                                                   Api.DolarArgentina.GetDollarPrice(DollarType.Ahorro),
                                                                   Api.DolarArgentina.GetDollarPrice(DollarType.Blue),
                                                                   Api.DolarArgentina.GetDollarPrice(DollarType.Bolsa),
                                                                   Api.DolarArgentina.GetDollarPrice(DollarType.Promedio),
                                                                   Api.DolarArgentina.GetDollarPrice(DollarType.ContadoConLiqui)).ConfigureAwait(false);
                    if (responses.Any(r => r != null))
                    {
                        DolarResponse[] successfulResponses = responses.Where(r => r != null).ToArray();
                        EmbedBuilder embed = CreateDollarEmbed(successfulResponses);
                        if (responses.Length != successfulResponses.Length)
                        {
                            await ReplyAsync($"{Format.Bold("Atención")}: No se pudieron obtener algunas cotizaciones. Sólo se mostrarán aquellas que no presentan errores.").ConfigureAwait(false);
                        }
                        await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                    }
                }
            }
        }

        [Command("dolaroficial", RunMode = RunMode.Async)]
        [Alias("do")]
        [Summary("Muestra la cotización del dólar oficial (Banco Nación).")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarOficialPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Oficial).ConfigureAwait(false);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar oficial")} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                }
            }
        }

        [Command("dolarahorro", RunMode = RunMode.Async)]
        [Alias("da")]
        [Summary("Muestra la cotización del dólar oficial más impuesto P.A.I.S. y ganancias.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarAhorroPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Ahorro).ConfigureAwait(false);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar ahorro")} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                }
            }
        }

        [Command("dolarblue", RunMode = RunMode.Async)]
        [Alias("db")]
        [Summary("Muestra la cotización del dólar blue.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarBluePriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Blue).ConfigureAwait(false);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar blue")} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                }
            }
        }

        [Command("dolarpromedio", RunMode = RunMode.Async)]
        [Alias("dp")]
        [Summary("Muestra el promedio de las cotizaciones bancarias del dólar oficial.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarPromedioPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Promedio).ConfigureAwait(false);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, $"Cotización {Format.Bold("promedio de los bancos del dólar oficial")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                }
            }
        }

        [Command("dolarbolsa", RunMode = RunMode.Async)]
        [Alias("dbo")]
        [Summary("Muestra la cotización del dólar bolsa (MEP).")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarBolsaPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Bolsa).ConfigureAwait(false);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar bolsa (MEP)")} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                }
            }
        }

        [Command("contadoconliqui", RunMode = RunMode.Async)]
        [Alias("ccl")]
        [Summary("Muestra la cotización del dólar contado con liquidación.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarContadoConLiquiPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.ContadoConLiqui).ConfigureAwait(false);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar contado con liquidación")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                }
            }
        }

        [Command("riesgopais", RunMode = RunMode.Async)]
        [Alias("rp")]
        [Summary("Muestra el valor del riesgo país.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetRiesgoPaisValueAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                RiesgoPaisResponse result = await Api.DolarArgentina.GetRiesgoPais().ConfigureAwait(false);
                if (result != null)
                {
                    EmbedBuilder embed = CreateRiesgoPaisEmbed(result);
                    await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                }
            }
        }

        #region Methods

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple dollar responses.
        /// </summary>
        /// <param name="dollarResponses">The dollar responses to show.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        private EmbedBuilder CreateDollarEmbed(DolarResponse[] dollarResponses)
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
        private EmbedBuilder CreateDollarEmbed(DolarResponse[] dollarResponses, string description, string thumbnailUrl)
        {
            Emoji dollarEmoji = new Emoji("\uD83D\uDCB5");
            Emoji clockEmoji = new Emoji("\u23F0");

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();

            EmbedBuilder embed = new EmbedBuilder().WithColor(mainEmbedColor)
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
        private EmbedBuilder CreateDollarEmbed(DolarResponse dollarResponse, string description, string title = null, string thumbnailUrl = null)
        {
            Emoji dollarEmoji = new Emoji("\uD83D\uDCB5");
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            string dollarImageUrl = thumbnailUrl ?? Configuration.GetSection("images").GetSection("dollar")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = title ?? GetTitle(dollarResponse);
            string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(dollarResponse.Fecha, localTimeZone).ToString(dollarResponse.Fecha.Date == DateTime.UtcNow.Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string buyPrice = decimal.TryParse(dollarResponse?.Compra, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal compra) ? $"${compra:F}" : null;
            string sellPrice = decimal.TryParse(dollarResponse?.Venta, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal venta) ? $"${venta:F}" : null;

            EmbedBuilder embed = new EmbedBuilder().WithColor(mainEmbedColor)
                                                   .WithTitle(embedTitle)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(dollarImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} ({localTimeZone.StandardName})", footerImageUrl)
                                                   .AddInlineField("Compra", Format.Bold($"{dollarEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {buyPrice}"))
                                                   .AddInlineField("Venta", Format.Bold($"{dollarEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {sellPrice}".AppendLineBreak()));
            return embed;
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
                DollarType.BBVA => DOLAR_BBVA_TITLE,
                DollarType.Piano => DOLAR_PIANO_TITLE,
                DollarType.Hipotecario => DOLAR_HIPOTECARIO_TITLE,
                DollarType.Galicia => DOLAR_GALICIA_TITLE,
                DollarType.Santander => DOLAR_SANTANDER_TITLE,
                DollarType.Ciudad => DOLAR_CIUDAD_TITLE,
                DollarType.Supervielle => DOLAR_SUPERVIELLE_TITLE,
                DollarType.Patagonia => DOLAR_PATAGONIA_TITLE,
                DollarType.Comafi => DOLAR_COMAFI_TITLE,
                _ => string.Empty,
            };
        }

        /// <summary>
        /// Converts a <see cref="BankCommandType"/> object to its <see cref="DollarType"/> equivalent and returns its thumbnail URL.
        /// </summary>
        /// <param name="bankCommandType">The value to convert.</param>
        /// <param name="thumbnailUrl">The thumbnail URL corresponding to the bank.</param>
        /// <returns>The converted value as <see cref="DollarType"/>.</returns>
        private DollarType GetBankInformation(BankCommandType bankCommandType, out string thumbnailUrl)
        {
            switch (bankCommandType)
            {
                case BankCommandType.Nacion:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["nacion"];
                    return DollarType.Oficial;
                case BankCommandType.BBVA:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["bbva"];
                    return DollarType.BBVA;
                case BankCommandType.Piano:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["piano"];
                    return DollarType.Piano;
                case BankCommandType.Hipotecario:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["hipotecario"];
                    return DollarType.Hipotecario;
                case BankCommandType.Galicia:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["galicia"];
                    return DollarType.Galicia;
                case BankCommandType.Santander:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["santander"];
                    return DollarType.Santander;
                case BankCommandType.Ciudad:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["ciudad"];
                    return DollarType.Ciudad;
                case BankCommandType.Supervielle:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["supervielle"];
                    return DollarType.Supervielle;
                case BankCommandType.Patagonia:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["patagonia"];
                    return DollarType.Patagonia;
                case BankCommandType.Comafi:
                    thumbnailUrl = Configuration.GetSection("images").GetSection("banks")["comafi"];
                    return DollarType.Comafi;
                default:
                    thumbnailUrl = string.Empty;
                    return DollarType.Oficial;
            }
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="RiesgoPaisResponse"/>.
        /// </summary>
        /// <param name="riesgoPaisResponse">The Riesgo Pais response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateRiesgoPaisEmbed(RiesgoPaisResponse riesgoPaisResponse)
        {
            Emoji chartEmoji = new Emoji("\uD83D\uDCC8");
            string chartImageUrl = Configuration.GetSection("images").GetSection("chart")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string value = decimal.TryParse(riesgoPaisResponse?.Valor, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal valor) ? ((int)Math.Round(valor * 1000, MidpointRounding.AwayFromZero)).ToString() : "No informado";

            EmbedBuilder embed = new EmbedBuilder().WithColor(mainEmbedColor)
                                                   .WithTitle("Riesgo País")
                                                   .WithDescription($"Valor del {Format.Bold("riesgo país")} argentino.".AppendLineBreak())
                                                   .WithThumbnailUrl(chartImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {TimeZoneInfo.ConvertTimeFromUtc(riesgoPaisResponse.Fecha, GlobalConfiguration.GetLocalTimeZoneInfo()):dd/MM/yyyy - HH:mm}",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"Valor", $"{Format.Bold($"{chartEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {value}")} puntos".AppendLineBreak());
            return embed;
        }
        #endregion
    }
}