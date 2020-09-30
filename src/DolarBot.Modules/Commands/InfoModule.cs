using Discord;
using Discord.Commands;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains information related commands.
    /// </summary>
    [HelpOrder(2)]
    [HelpTitle("Información")]
    public class InfoModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Color for the embed messages.
        /// </summary>
        private readonly Color infoEmbedColor = new Color(23, 99, 154);
        #endregion

        #region Constructor
        public InfoModule(IConfiguration configuration) : base(configuration) { }
        #endregion

        [Command("hora")]
        [Alias("date")]
        [Summary("Muestra la fecha y hora del bot y del servidor donde se aloja.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDateAsync()
        {
            Emoji timeEmoji = new Emoji("\u23F0");
            string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
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

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("sid")]
        [Summary("Muestra el ID del servidor de Discord actual.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetServerId()
        {
            string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
            string sid = Format.Bold(Context.Guild.Id.ToString());
            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("Server ID")
                                 .WithColor(infoEmbedColor)
                                 .WithThumbnailUrl(infoImageUrl)
                                 .WithDescription($"El ID del servidor es {sid}");

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("ping")]
        [Summary("Muestra la latencia del bot de Discord.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task Ping()
        {
            string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
            string commandPrefix = Configuration["commandPrefix"];

            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("Procesando...")
                                 .WithColor(infoEmbedColor);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var message = await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
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
            }).ConfigureAwait(false);
        }

        [Command("invitar")]
        [Alias("invite")]
        [Summary("Devuelve el link de invitación del bot en Discord.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetInviteLink()
        {
            string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
            string inviteLink = Configuration["inviteLink"];

            if (string.IsNullOrWhiteSpace(inviteLink))
            {
                inviteLink = Environment.GetEnvironmentVariable(GlobalConfiguration.GetInviteLinkEnvVarName());
            }

            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("DolarBot")
                                 .WithColor(infoEmbedColor)
                                 .WithThumbnailUrl(infoImageUrl)
                                 .WithDescription($"Invita al bot haciendo {Format.Url("click acá", inviteLink)}");

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("bot")]
        [Summary("Muestra información acerca del bot.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetAbout()
        {
            Emoji heartEmoji = new Emoji("\uD83D\uDC99");
            Emoji versionEmoji = new Emoji("\uD83D\uDCCD");
            Emoji supportServerEmoji = new Emoji("\uD83D\uDCAC");
            Emoji githubEmoji = new Emoji("\uD83D\uDCBB");
            Emoji coffeeEmoji = new Emoji("\u2615");
            Emoji checkEmoji = new Emoji("\u2705");

            string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
            string githubUrl = Configuration["githubUrl"];
            string donationUrl = Configuration["donationUrl"];
            string supportServerUrl = Configuration["supportServerUrl"];
            string version = Format.Bold(Assembly.GetEntryAssembly().GetName().Version.ToString());
            int serverCount = Context.Client.Guilds.Count;            

            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("DolarBot")
                                 .WithColor(infoEmbedColor)
                                 .WithThumbnailUrl(infoImageUrl)
                                 .WithDescription($"{versionEmoji} Versión: {version}".AppendLineBreak())
                                 .AddField("Status", $"{checkEmoji} {Format.Bold("Online")} en {Format.Bold(serverCount.ToString())} {(serverCount > 1 ? "servidores" : "servidor")}.".AppendLineBreak())
                                 .AddField("¿Dudas o sugerencias?", $"{supportServerEmoji} Unite al {Format.Url("servidor de soporte", supportServerUrl)}".AppendLineBreak())
                                 .AddField("GitHub", $"{githubEmoji} {Format.Url("guidospadavecchia/DolarBot", githubUrl)}".AppendLineBreak())
                                 .AddField("¿Te gusta DolarBot?", $"{coffeeEmoji} Invitame un {Format.Url("café", donationUrl)}".AppendLineBreak())
                                 .WithFooter($"Hecho con {heartEmoji} en .NET Core");

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }
    }
}