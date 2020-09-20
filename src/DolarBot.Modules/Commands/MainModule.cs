using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DollarType = DolarBot.API.ApiCalls.DolarArgentinaApi.DollarType;

namespace DolarBot.Modules.Commands
{
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
        private const string REQUEST_ERROR_MESSAGE = "Error: No se pudo obtener la cotización. Intente nuevamente en más tarde.";
        #endregion

        #region Vars
        private readonly Color mainEmbedColor = new Color(67, 181, 129);
        protected readonly ApiCalls Api;
        #endregion

        #region Constructor
        public MainModule(IConfiguration configuration, ApiCalls api) : base(configuration)
        {
            Api = api;
        }
        #endregion

        [Command("dolar")]
        [Alias("d")]
        [Summary("Muestra todas las cotizaciones del dólar disponibles.")]
        [RateLimit(1, 5, Measure.Seconds, RatelimitFlags.ApplyPerGuild)]
        public async Task GetDolarPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse[] responses = await Task.WhenAll(Api.DolarArgentina.GetDollarPrice(DollarType.Oficial),
                                                               Api.DolarArgentina.GetDollarPrice(DollarType.Ahorro),
                                                               Api.DolarArgentina.GetDollarPrice(DollarType.Blue),
                                                               Api.DolarArgentina.GetDollarPrice(DollarType.Bolsa),
                                                               Api.DolarArgentina.GetDollarPrice(DollarType.Promedio),
                                                               Api.DolarArgentina.GetDollarPrice(DollarType.ContadoConLiqui));
                if (responses.All(r => r != null))
                {
                    EmbedBuilder embed = CreateDollarEmbed(responses);
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE);
                }
            }
        }

        [Command("dolaroficial")]
        [Alias("do")]
        [Summary("Muestra la cotización del dólar oficial (Banco Nación).")]
        [RateLimit(1, 5, Measure.Seconds, RatelimitFlags.ApplyPerGuild)]
        public async Task GetDolarOficialPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Oficial);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar oficial")} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE);
                }
            }
        }

        [Command("dolarahorro")]
        [Alias("da")]
        [Summary("Muestra la cotización del dólar oficial más impuesto P.A.I.S. y retención de ganancias.")]
        [RateLimit(1, 5, Measure.Seconds, RatelimitFlags.ApplyPerGuild)]
        public async Task GetDolarAhorroPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Ahorro);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar ahorro")} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE);
                }
            }
        }

        [Command("dolarblue")]
        [Alias("db")]
        [Summary("Muestra la cotización del dólar blue.")]
        [RateLimit(1, 5, Measure.Seconds, RatelimitFlags.ApplyPerGuild)]
        public async Task GetDolarBluePriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Blue);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar blue")} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE);
                }
            }
        }

        [Command("dolarpromedio")]
        [Alias("dp")]
        [Summary("Muestra el promedio de las cotizaciones bancarias del dólar oficial.")]
        [RateLimit(1, 5, Measure.Seconds, RatelimitFlags.ApplyPerGuild)]
        public async Task GetDolarPromedioPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Promedio);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, $"Cotización {Format.Bold("promedio de los bancos del dólar oficial")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE);
                }
            }
        }

        [Command("dolarbolsa")]
        [Alias("dbo")]
        [Summary("Muestra la cotización del dólar bolsa (MEP).")]
        [RateLimit(1, 5, Measure.Seconds, RatelimitFlags.ApplyPerGuild)]
        public async Task GetDolarBolsaPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Bolsa);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar bolsa (MEP)")} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE);
                }
            }
        }

        [Command("contadoconliqui")]
        [Alias("ccl")]
        [Summary("Muestra la cotización del dólar contado con liquidación.")]
        [RateLimit(1, 5, Measure.Seconds, RatelimitFlags.ApplyPerGuild)]
        public async Task GetDolarContadoConLiquiPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.ContadoConLiqui);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar contado con liquidación")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE);
                }
            }
        }

        [Command("riesgopais")]
        [Alias("rp")]
        [Summary("Muestra el valor del riesgo país.")]
        [RateLimit(1, 5, Measure.Seconds, RatelimitFlags.ApplyPerGuild)]
        public async Task GetRiesgoPaisValueAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                RiesgoPaisResponse result = await Api.DolarArgentina.GetRiesgoPais();
                if (result != null)
                {
                    EmbedBuilder embed = CreateRiesgoPaisEmbed(result);
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE);
                }
            }
        }

        #region Methods

        private EmbedBuilder CreateDollarEmbed(DolarResponse[] dollarResponses)
        {
            Emoji dollarEmoji = new Emoji("\uD83D\uDCB5");
            Emoji clockEmoji = new Emoji("\u23F0");
            string dollarImageUrl = Configuration.GetSection("images")?.GetSection("dollar")?["64"];
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();

            EmbedBuilder embed = new EmbedBuilder().WithColor(mainEmbedColor)
                                                   .WithTitle("Cotizaciones del Dólar")
                                                   .WithDescription($"Cotizaciones disponibles expresadas en {Format.Bold("pesos argentinos")}.".AppendLineBreak())
                                                   .WithThumbnailUrl(dollarImageUrl)
                                                   .WithFooter($"{clockEmoji} = Última actualización ({localTimeZone.StandardName})");

            foreach (DolarResponse response in dollarResponses)
            {
                string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
                string title = GetTitle(response);
                string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(response.Fecha, localTimeZone).ToString("dd/MM - HH:mm");
                string buyPrice = decimal.TryParse(response?.Compra, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal compra) ? $"${compra:F}" : "?";
                string sellPrice = decimal.TryParse(response?.Venta, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal venta) ? $"${venta:F}" : "?";

                StringBuilder sbField = new StringBuilder().Append($"{dollarEmoji} {blankSpace} Compra: {Format.Bold(buyPrice)} {blankSpace}")
                                                           .AppendLine($"{dollarEmoji} {blankSpace} Venta: {Format.Bold(sellPrice)} {blankSpace}")
                                                           .AppendLine($"{clockEmoji} {blankSpace} {lastUpdated} {blankSpace}");
                embed.AddInlineField(title, sbField.ToString().AppendLineBreak());
            }

            return embed;
        }

        public EmbedBuilder CreateDollarEmbed(DolarResponse dollarResponse, string description)
        {
            Emoji dollarEmoji = new Emoji("\uD83D\uDCB5");
            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            string dollarImageUrl = Configuration.GetSection("images")?.GetSection("dollar")?["64"];
            string footerImageUrl = Configuration.GetSection("images")?.GetSection("clock")?["32"];
            string title = GetTitle(dollarResponse);
            string lastUpdated = TimeZoneInfo.ConvertTimeFromUtc(dollarResponse.Fecha, localTimeZone).ToString(dollarResponse.Fecha.Date == DateTime.UtcNow.Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string buyPrice = decimal.TryParse(dollarResponse?.Compra, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal compra) ? $"${compra:F}" : "?";
            string sellPrice = decimal.TryParse(dollarResponse?.Venta, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal venta) ? $"${venta:F}" : "?";

            EmbedBuilder embed = new EmbedBuilder().WithColor(mainEmbedColor)
                                                   .WithTitle(title)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(dollarImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} ({localTimeZone.StandardName})", footerImageUrl)
                                                   .AddInlineField("Compra", Format.Bold($"{dollarEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {buyPrice}"))
                                                   .AddInlineField("Venta", Format.Bold($"{dollarEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {sellPrice}".AppendLineBreak()));
            return embed;
        }

        public string GetTitle(DolarResponse dollarResponse)
        {
            switch (dollarResponse.Type)
            {
                case DollarType.Oficial:
                    return DOLAR_OFICIAL_TITLE;
                case DollarType.Ahorro:
                    return DOLAR_AHORRO_TITLE;
                case DollarType.Blue:
                    return DOLAR_BLUE_TITLE;
                case DollarType.Bolsa:
                    return DOLAR_BOLSA_TITLE;
                case DollarType.Promedio:
                    return DOLAR_PROMEDIO_TITLE;
                case DollarType.ContadoConLiqui:
                    return DOLAR_CCL_TITLE;
                default:
                    return string.Empty;
            }
        }

        public EmbedBuilder CreateRiesgoPaisEmbed(RiesgoPaisResponse riesgoPaisResponse)
        {
            Emoji chartEmoji = new Emoji("\uD83D\uDCC8");
            string chartImageUrl = Configuration.GetSection("images")?.GetSection("chart")?["64"];
            string footerImageUrl = Configuration.GetSection("images")?.GetSection("clock")?["32"];
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