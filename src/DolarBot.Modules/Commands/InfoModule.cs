using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using DolarBot.Modules.Attributes;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    [HelpOrder(2)]
    [HelpTitle("Información")]
    public class InfoModule : InteractiveBase<SocketCommandContext>
    {
        private readonly Color infoEmbedColor = new Color(23, 99, 154);
        private readonly IConfiguration configuration;

        public InfoModule(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [Command("hora")]
        [Alias("date")]
        [Summary("Muestra la fecha y hora del bot y del servidor donde se aloja.")]
        public async Task GetDateAsync()
        {
            Emoji timeEmoji = new Emoji("\u23F0");
            string infoImageUrl = configuration.GetSection("images")?.GetSection("info")?["64"];
            TimeZoneInfo localTimeZoneInfo = GlobalConfiguration.GetLocalTimeZoneInfo();
            string localTimestamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZoneInfo).ToString("yyyy/MM/dd - HH:mm:ss");
            string serverTimestamp = DateTime.Now.ToString("yyyy/MM/dd - HH:mm:ss");

            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("Fecha y Hora")
                                 .WithColor(infoEmbedColor)
                                 .WithThumbnailUrl(infoImageUrl)
                                 .WithDescription(GlobalConfiguration.Constants.BLANK_SPACE)
                                 .AddField($"Fecha y hora del servidor", $"{timeEmoji} {serverTimestamp} ({Format.Italics(TimeZoneInfo.Local.StandardName)})".AppendLineBreak())
                                 .AddField($"Fecha y hora del bot", $"{timeEmoji} {localTimestamp} ({Format.Italics(localTimeZoneInfo.StandardName)})");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("sid")]
        [Summary("Muestra el ID del servidor de Discord actual.")]
        public async Task GetServerId()
        {
            string infoImageUrl = configuration.GetSection("images")?.GetSection("info")?["64"];
            string sid = Format.Bold(Context.Guild.Id.ToString());
            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("Server ID")
                                 .WithColor(infoEmbedColor)
                                 .WithThumbnailUrl(infoImageUrl)
                                 .WithDescription($"El ID del servidor es {sid}");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("ping")]
        [Summary("Muestra la latencia del bot de Discord.")]
        public async Task Ping()
        {
            string infoImageUrl = configuration.GetSection("images")?.GetSection("info")?["64"];
            string commandPrefix = configuration["commandPrefix"];

            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("Procesando...")
                                 .WithColor(infoEmbedColor);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var message = await ReplyAsync(embed: embed.Build());
            sw.Stop();

            string responseTime = $"{Context.Client.Latency} ms";
            string latency = $"{sw.ElapsedMilliseconds} ms";
            await message.ModifyAsync(x =>
            {
                Emoji pingEmoji = new Emoji("\uD83D\uDCE1");
                Emoji gatewayEmoji = new Emoji("\uD83D\uDEAA");

                embed.WithTitle($"Resultado del {Format.Code($"{commandPrefix}ping")}")
                     .WithThumbnailUrl(infoImageUrl)
                     .AddInlineField("Tiempo de respuesta", $"{pingEmoji} {responseTime}")
                     .AddInlineField()
                     .AddInlineField("Latencia del gateway", $"{gatewayEmoji} {latency}");

                x.Embed = embed.Build();
            });
        }

        [Command("invite")]
        [Alias("invitar")]
        [Summary("Devuelve el link de invitación del bot en Discord.")]
        public async Task GetInviteLink()
        {
            string infoImageUrl = configuration.GetSection("images")?.GetSection("info")?["64"];
            string inviteLink = configuration["inviteLink"];

            if (string.IsNullOrWhiteSpace(inviteLink))
            {
                inviteLink = Environment.GetEnvironmentVariable(GlobalConfiguration.GetInviteLinkEnvVarName());
            }

            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("DolarBot")
                                 .WithColor(infoEmbedColor)
                                 .WithThumbnailUrl(infoImageUrl)
                                 .WithDescription($"Invita al bot haciendo {Format.Url("click acá", inviteLink)}");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("bot")]
        [Summary("Muestra información acerca del bot.")]
        public async Task GetAbout()
        {
            Emoji heartEmoji = new Emoji("\u2764");
            Emoji versionEmoji = new Emoji("\uD83D\uDCCD");
            string infoImageUrl = configuration.GetSection("images")?.GetSection("info")?["64"];
            string version = Format.Bold(Assembly.GetEntryAssembly().GetName().Version.ToString());

            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("DolarBot")
                                 .WithColor(infoEmbedColor)
                                 .WithThumbnailUrl(infoImageUrl)
                                 .WithDescription($"{versionEmoji} Versión: {version}".AppendLineBreak())
                                 .AddField("Autor", "Guido Spadavecchia")
                                 .AddField("Contacto", "guido.spadavecchia@gmail.com")
                                 .AddField("¿Te gusta DolarBot?", $"Invitame un {Format.Url("café", "https://www.mercadopago.com.ar/checkout/v1/redirect?pref_id=644604751-7a01236a-d22c-49f9-9194-f77c58485af1")}".AppendLineBreak())
                                 .WithFooter($"Hecho con {heartEmoji} en .NET Core");

            await ReplyAsync(embed: embed.Build());
        }
    }
}