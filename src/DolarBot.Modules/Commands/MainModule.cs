using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

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
                DolarResponse result = await api.DolarArgentina.GetDolarOficial();
                if (result != null)
                {
                    Emoji dollarEmoji = new Emoji("\uD83D\uDCB5");
                    string dollarImageUrl = configuration.GetSection("images")?.GetSection("dollar")?["64"];
                    string footerImageUrl = configuration.GetSection("images")?.GetSection("clock")?["32"];

                    EmbedBuilder embed = new EmbedBuilder().WithColor(mainEmbedColor)
                                                           .WithTitle(Format.Bold("Dolar oficial"))
                                                           .WithDescription($"Cotización del dólar oficial en {Format.Bold("pesos argentinos")}.")
                                                           .WithThumbnailUrl(dollarImageUrl)
                                                           .WithFooter(new EmbedFooterBuilder()
                                                           {
                                                               Text = $"Ultima actualización: {result.Fecha:dd/MM/yyyy HH:mm}",
                                                               IconUrl = footerImageUrl
                                                           })
                                                           .AddInlineField(Format.Bold($"{dollarEmoji} Compra"), Format.Bold($"${result.Compra}"))
                                                           .AddInlineField(Format.Bold($"{dollarEmoji} Venta"), Format.Bold($"${result.Venta}"))
                                                           .AddEmptyLine();
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
                
            }
        }
    }
}