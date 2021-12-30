using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Info;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains information related commands.
    /// </summary>
    [HelpOrder(99)]
    [HelpTitle("Información")]
    public class InfoModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about euro rates.
        /// </summary>
        private readonly InfoService InfoService;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public InfoModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger)
        {
            InfoService = new InfoService(configuration, api);
        }
        #endregion

        [Command("hora", RunMode = RunMode.Async)]
        [Alias("date")]
        [Summary("Muestra la fecha y hora del bot y del servidor donde se aloja.")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetDateAsync()
        {
            try
            {
                Emoji timeEmoji = new Emoji("\u23F0");
                string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
                TimeZoneInfo localTimeZoneInfo = GlobalConfiguration.GetLocalTimeZoneInfo();
                string localTimestamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZoneInfo).ToString("yyyy/MM/dd - HH:mm:ss");
                string serverTimestamp = DateTime.Now.ToString("yyyy/MM/dd - HH:mm:ss");

                EmbedBuilder embed = new EmbedBuilder()
                                     .WithTitle("Fecha y Hora")
                                     .WithColor(GlobalConfiguration.Colors.Info)
                                     .WithThumbnailUrl(infoImageUrl)
                                     .WithDescription(GlobalConfiguration.Constants.BLANK_SPACE)
                                     .AddField($"Fecha y hora del servidor", $"{timeEmoji} {serverTimestamp} ({Format.Italics(TimeZoneInfo.Local.StandardName)})".AppendLineBreak())
                                     .AddField($"Fecha y hora del bot", $"{timeEmoji} {localTimestamp} ({Format.Italics(localTimeZoneInfo.StandardName)})");

                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("sid", RunMode = RunMode.Async)]
        [Summary("Muestra el ID del servidor de Discord actual.")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetServerId()
        {
            try
            {
                string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
                string sid = Format.Bold(Context.Guild.Id.ToString());
                EmbedBuilder embed = new EmbedBuilder()
                                     .WithTitle("Server ID")
                                     .WithColor(GlobalConfiguration.Colors.Info)
                                     .WithThumbnailUrl(infoImageUrl)
                                     .WithDescription($"El ID del servidor es {sid}");

                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("ping", RunMode = RunMode.Async)]
        [Summary("Muestra la latencia del bot de Discord.")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task Ping()
        {
            try
            {
                string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
                string commandPrefix = Configuration["commandPrefix"];

                EmbedBuilder embed = new EmbedBuilder()
                                     .WithTitle("Procesando...")
                                     .WithColor(GlobalConfiguration.Colors.Info);

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
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("invitar", RunMode = RunMode.Async)]
        [Alias("invite")]
        [Summary("Devuelve el link de invitación del bot en Discord.")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetInviteLink()
        {
            try
            {
                string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
                string inviteLink = Configuration["inviteLink"];

                if (string.IsNullOrWhiteSpace(inviteLink))
                {
                    inviteLink = Environment.GetEnvironmentVariable(GlobalConfiguration.GetInviteLinkEnvVarName());
                }

                EmbedBuilder embed = new EmbedBuilder()
                                     .WithTitle("DolarBot")
                                     .WithColor(GlobalConfiguration.Colors.Info)
                                     .WithThumbnailUrl(infoImageUrl)
                                     .WithDescription($"Invita al bot haciendo {Format.Url("click acá", inviteLink)}");

                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("votar", RunMode = RunMode.Async)]
        [Alias("vote")]
        [Summary("Muestra el link para votar por DolarBot")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetVoteLink()
        {
            try
            {
                string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
                string voteLink = Configuration["voteUrl"];

                EmbedBuilder embed = new EmbedBuilder()
                                     .WithTitle("Votar")
                                     .WithColor(GlobalConfiguration.Colors.Info)
                                     .WithThumbnailUrl(infoImageUrl)
                                     .WithDescription($"Podes votar por {Format.Bold("DolarBot")} haciendo {Format.Url("click acá", voteLink)}. Gracias por tu apoyo!");

                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("bot", RunMode = RunMode.Async)]
        [Summary("Muestra información acerca del bot.")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetAbout()
        {
            try
            {
                var emojis = Configuration.GetSection("customEmojis");
                Emoji blueHeartEmoji = new Emoji("\uD83D\uDC99");
                Emoji versionEmoji = new Emoji(emojis["dolarbot"]);
                Emoji supportServerEmoji = new Emoji("\uD83D\uDCAC");
                Emoji checkEmoji = new Emoji("\u2705");
                Emoji voteEmoji = new Emoji(emojis["arrowUpRed"]);
                Emoji hourglassEmoji = new Emoji("\u23F3");

                Version version = Assembly.GetEntryAssembly().GetName().Version;
                string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
                string dolarBotImageUrl = Configuration.GetSection("images")?.GetSection("dolarbot")?["32"];
                string websiteUrl = Configuration["websiteUrl"];
                string githubUrl = Configuration["githubUrl"];
                string playStoreUrl = Configuration["playStoreLink"];
                string supportServerUrl = Configuration["supportServerUrl"];
                string voteUrl = Configuration["voteUrl"];
                string versionDescription = Format.Bold(version.ToString(3));
                int serverCount = Context.Client.Guilds.Count;
                TimeSpan uptime = GlobalConfiguration.GetUptime();

                EmbedBuilder embed = new EmbedBuilder()
                                     .WithTitle("DolarBot")                                     
                                     .WithColor(GlobalConfiguration.Colors.Info)
                                     .WithThumbnailUrl(infoImageUrl)
                                     .WithDescription($"{versionEmoji} Versión {versionDescription}".AppendLineBreak())
                                     .AddField("Status", $"{checkEmoji} {Format.Bold("Online")} en {Format.Bold(serverCount.ToString())} {(serverCount > 1 ? "servidores" : "servidor")}".AppendLineBreak())
                                     .AddField("Uptime", $"{hourglassEmoji} {Format.Bold(uptime.Days.ToString())}d {Format.Bold(uptime.Hours.ToString())}h {Format.Bold(uptime.Minutes.ToString())}m {Format.Bold(uptime.Seconds.ToString())}s".AppendLineBreak())
                                     .AddField("¿Dudas o sugerencias?", $"{supportServerEmoji} Unite al {Format.Url("servidor oficial de DolarBot", supportServerUrl)}".AppendLineBreak())
                                     .AddField("Web", InfoService.GetWebsiteEmbedDescription().AppendLineBreak())
                                     .AddField("¿Te gusta DolarBot?", new StringBuilder().AppendLine($"{voteEmoji} {Format.Url("Votalo en top.gg", voteUrl)}").AppendLineBreak())
                                     .WithFooter($"Hecho con {blueHeartEmoji} en {RuntimeInformation.FrameworkDescription}");

                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("status", RunMode = RunMode.Async)]
        [Summary("Obtiene el estado actual del bot.")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetApiStatus()
        {
            try
            {
                string statusText = await InfoService.GetApiStatus();
                EmbedBuilder embed = InfoService.CreateStatusEmbed(statusText, Context.Client.Latency);
                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }
    }
}