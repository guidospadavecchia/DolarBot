using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using DollarType = DolarBot.API.ApiCalls.DolarArgentinaApi.DollarType;

namespace DolarBot.Modules.Commands
{
    [HelpOrder(1)]
    [HelpTitle("Cotizaciones")]
    public class MainModule : InteractiveBase<SocketCommandContext>
    {
        private const string ERROR_MESSAGE = "Error: No se pudo obtener la cotización. Intente nuevamente en más tarde.";
        private readonly Color mainEmbedColor = new Color(67, 181, 129);
        private readonly IConfiguration configuration;
        private readonly ApiCalls api;

        public MainModule(IConfiguration configuration, ApiCalls api)
        {
            this.configuration = configuration;
            this.api = api;
        }

        [Command("dolaroficial")]
        [Alias("do")]
        [Summary("Muestra la cotización del dólar oficial.")]
        public async Task GetDolarOficialPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await api.DolarArgentina.GetDollarPrice(DollarType.Oficial);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, Format.Bold("Dólar Oficial"), $"Cotización del {Format.Bold("dólar oficial")} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(ERROR_MESSAGE);
                }
            }
        }

        [Command("dolarturista")]
        [Alias("dt")]
        [Summary("Muestra la cotización del dólar oficial más el impuesto PAÍS.")]
        public async Task GetDolarTuristaPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await api.DolarArgentina.GetDollarPrice(DollarType.Oficial);
                if (result != null)
                {
                    decimal taxPercent = (decimal.Parse(configuration["dollarTaxPercent"]) / 100) + 1;
                    result.Venta *= taxPercent;

                    EmbedBuilder embed = CreateDollarEmbed(result, Format.Bold("Dólar Turista"), $"Cotización del {Format.Bold("dólar turista")} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(ERROR_MESSAGE);
                }
            }
        }

        [Command("dolarblue")]
        [Alias("db")]
        [Summary("Muestra la cotización del dólar blue.")]
        public async Task GetDolarBluePriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await api.DolarArgentina.GetDollarPrice(DollarType.Blue);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, Format.Bold("Dólar Blue"), $"Cotización del {Format.Bold("dólar blue")} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(ERROR_MESSAGE);
                }
            }
        }

        [Command("dolarpromedio")]
        [Alias("dp")]
        [Summary("Muestra el promedio de las cotizaciones del dólar oficial.")]
        public async Task GetDolarPromedioPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await api.DolarArgentina.GetDollarPrice(DollarType.Promedio);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, Format.Bold("Dólar Promedio"), $"Cotización {Format.Bold("promedio de los bancos del dólar oficial")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(ERROR_MESSAGE);
                }
            }
        }

        [Command("dolarbolsa")]
        [Alias("dbo")]
        [Summary("Muestra la cotización del dólar bolsa.")]
        public async Task GetDolarBolsaPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await api.DolarArgentina.GetDollarPrice(DollarType.Bolsa);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, Format.Bold("Dólar Bolsa"), $"Cotización del {Format.Bold("dólar bolsa (MEP)")} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(ERROR_MESSAGE);
                }
            }
        }

        [Command("contadoconliqui")]
        [Alias("ccl")]
        [Summary("Muestra la cotización del dólar contado con liquidación.")]
        public async Task GetDolarContadoConLiquiPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarResponse result = await api.DolarArgentina.GetDollarPrice(DollarType.ContadoConLiqui);
                if (result != null)
                {
                    EmbedBuilder embed = CreateDollarEmbed(result, Format.Bold("Dólar Contado con Liqui"), $"Cotización del {Format.Bold("dólar contado con liquidación")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.");
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(ERROR_MESSAGE);
                }
            }
        }

        [Command("riesgopais")]
        [Alias("rp")]
        [Summary("Muestra el valor del riesgo país.")]
        public async Task GetRiesgoPaisValueAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                RiesgoPaisResponse result = await api.DolarArgentina.GetRiesgoPais();
                if (result != null)
                {
                    EmbedBuilder embed = CreateRiesgoPaisEmbed(result);
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync(ERROR_MESSAGE);
                }
            }
        }

        #region Methods

        public EmbedBuilder CreateDollarEmbed(DolarResponse dollarResponse, string title, string description)
        {
            Emoji dollarEmoji = new Emoji("\uD83D\uDCB5");
            string dollarImageUrl = configuration.GetSection("images")?.GetSection("dollar")?["64"];
            string footerImageUrl = configuration.GetSection("images")?.GetSection("clock")?["32"];

            EmbedBuilder embed = new EmbedBuilder().WithColor(mainEmbedColor)
                                                   .WithTitle(title)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(dollarImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {TimeZoneInfo.ConvertTimeFromUtc(dollarResponse.Fecha, GlobalConfiguration.GetLocalTimeZoneInfo()):dd/MM/yyyy HH:mm}",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"{dollarEmoji} Compra", Format.Bold($"${dollarResponse.Compra:F}"))
                                                   .AddInlineField($"{dollarEmoji} Venta", Format.Bold($"${dollarResponse.Venta:F}".AppendLineBreak()));
            return embed;
        }

        public EmbedBuilder CreateRiesgoPaisEmbed(RiesgoPaisResponse riesgoPaisResponse)
        {
            Emoji chartEmoji = new Emoji("\uD83D\uDCC8");
            string chartImageUrl = configuration.GetSection("images")?.GetSection("chart")?["64"];
            string footerImageUrl = configuration.GetSection("images")?.GetSection("clock")?["32"];
            int value = (int)(Math.Round(riesgoPaisResponse.Valor * 1000, MidpointRounding.AwayFromZero));

            EmbedBuilder embed = new EmbedBuilder().WithColor(mainEmbedColor)
                                                   .WithTitle("Riesgo País")
                                                   .WithDescription($"Valor del {Format.Bold("riesgo país")} argentino.".AppendLineBreak())
                                                   .WithThumbnailUrl(chartImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {TimeZoneInfo.ConvertTimeFromUtc(riesgoPaisResponse.Fecha, GlobalConfiguration.GetLocalTimeZoneInfo()):dd/MM/yyyy HH:mm}",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"{chartEmoji} Valor", Format.Bold(value.ToString()).AppendLineBreak());
            return embed;
        }
        #endregion
    }
}