﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DolarBot.API;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Base;
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

namespace DolarBot.Modules.InteractiveCommands
{
    /// <summary>
    /// Contains information related commands.
    /// </summary>
    [HelpOrder(99)]
    [HelpTitle("Información")]
    public class InfoInteractiveModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve general information about the bot and the server.
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
        public InfoInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger)
        {
            InfoService = new InfoService(configuration, api);
        }
        #endregion

        [SlashCommand("hora", "Muestra la fecha y hora del bot y del servidor donde se aloja.", false, RunMode.Async)]
        public async Task GetDateAsync()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    Emoji timeEmoji = new("\u23F0");
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

                    await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embed = embed.Build());
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponse(Context.Interaction, ex);
                }
            });
        }

        [SlashCommand("sid", "Muestra el ID del servidor de Discord actual.", false, RunMode.Async)]
        public async Task GetServerId()
        {
            await DeferAsync().ContinueWith(async (task) =>
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

                    await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embed = embed.Build());
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponse(Context.Interaction, ex);
                }
            });
        }

        [SlashCommand("ping", "Muestra la latencia del bot de Discord.", false, RunMode.Async)]
        public async Task Ping()
        {
            try
            {
                string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
                string commandPrefix = Configuration["commandPrefix"];

                EmbedBuilder embed = new EmbedBuilder()
                                     .WithTitle("Procesando...")
                                     .WithColor(GlobalConfiguration.Colors.Info);

                Stopwatch sw = new();
                sw.Start();
                await RespondAsync(embed: embed.Build());
                sw.Stop();

                string responseTime = Context.Client is DiscordSocketClient socketClient ? $"{socketClient.Latency} ms" : null;
                string latency = $"{sw.ElapsedMilliseconds} ms";
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    Emoji pingEmoji = new("\uD83D\uDCE1");
                    Emoji gatewayEmoji = new("\uD83D\uDEAA");

                    embed.WithTitle($"Resultado del {Format.Code($"{commandPrefix}ping")}").WithThumbnailUrl(infoImageUrl);
                    if (responseTime != null)
                    {
                        embed.AddInlineField("Tiempo de respuesta", $"{pingEmoji} {responseTime}").AddInlineField();
                    }
                    embed.AddInlineField("Latencia del gateway", $"{gatewayEmoji} {latency}");

                    x.Embed = embed.Build();
                });
            }
            catch (Exception ex)
            {
                await SendDeferredErrorResponse(Context.Interaction, ex);
            }
        }

        [SlashCommand("invitar", "Devuelve el link de invitación del bot en Discord.", false, RunMode.Async)]
        public async Task GetInviteLink()
        {
            await DeferAsync().ContinueWith(async (task) =>
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
                                         .WithDescription($"Invita al bot haciendo {Format.Url("click aquí", inviteLink)}");

                    await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embed = embed.Build());
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponse(Context.Interaction, ex);
                }
            });
        }

        [SlashCommand("votar", "Muestra el link para votar por DolarBot", false, RunMode.Async)]
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

                await RespondAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                await SendDeferredErrorResponse(Context.Interaction, ex);
            }
        }

        [SlashCommand("bot", "Muestra información acerca del bot.", false, RunMode.Async)]
        public async Task GetAbout()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    var emojis = Configuration.GetSection("customEmojis");
                    Emoji blueHeartEmoji = new("\uD83D\uDC99");
                    Emoji versionEmoji = new(emojis["dolarbot"]);
                    Emoji supportServerEmoji = new("\uD83D\uDCAC");
                    Emoji checkEmoji = new("\u2705");
                    Emoji voteEmoji = new(emojis["arrowUpRed"]);
                    Emoji hourglassEmoji = new("\u23F3");

                    Version version = Assembly.GetEntryAssembly().GetName().Version;
                    string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
                    string dolarBotImageUrl = Configuration.GetSection("images")?.GetSection("dolarbot")?["32"];
                    string websiteUrl = Configuration["websiteUrl"];
                    string githubUrl = Configuration["githubUrl"];
                    string playStoreUrl = Configuration["playStoreLink"];
                    string supportServerUrl = Configuration["supportServerUrl"];
                    string voteUrl = Configuration["voteUrl"];
                    string versionDescription = Format.Bold(version.ToString(3));
                    int serverCount = Context.Client is DiscordSocketClient socketClient ? socketClient.Guilds.Count : -1;
                    TimeSpan uptime = GlobalConfiguration.GetUptime();

                    EmbedBuilder embed = new EmbedBuilder()
                                         .WithTitle("DolarBot")
                                         .WithColor(GlobalConfiguration.Colors.Info)
                                         .WithThumbnailUrl(infoImageUrl)
                                         .WithDescription($"{versionEmoji} Versión {versionDescription}".AppendLineBreak());
                    if (serverCount > -1)
                    {
                        embed.AddField("Status", $"{checkEmoji} {Format.Bold("Online")} en {Format.Bold(serverCount.ToString())} {(serverCount > 1 ? "servidores" : "servidor")}".AppendLineBreak());
                    }

                    embed.AddField("Uptime (Desde último reinicio)", $"{hourglassEmoji} {Format.Bold(uptime.Days.ToString())}d {Format.Bold(uptime.Hours.ToString())}h {Format.Bold(uptime.Minutes.ToString())}m {Format.Bold(uptime.Seconds.ToString())}s".AppendLineBreak())
                         .AddField("¿Dudas o sugerencias?", $"{supportServerEmoji} Unite al {Format.Url("servidor oficial de DolarBot", supportServerUrl)}".AppendLineBreak())
                         .AddField("Web", InfoService.GetWebsiteEmbedDescription().AppendLineBreak())
                         .AddField("¿Te gusta DolarBot?", new StringBuilder().AppendLine($"{voteEmoji} {Format.Url("Votalo en top.gg", voteUrl)}").AppendLineBreak())
                         .WithFooter($"Hecho con {blueHeartEmoji} en {RuntimeInformation.FrameworkDescription}");

                    await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embed = embed.Build());
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponse(Context.Interaction, ex);
                }
            });
        }

        [SlashCommand("status", "Obtiene el estado actual del bot.", false, RunMode.Async)]
        public async Task GetApiStatus()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    string statusText = await InfoService.GetApiStatus();
                    int? latency = Context.Client is DiscordSocketClient socketClient ? socketClient.Latency : null;
                    EmbedBuilder embed = InfoService.CreateStatusEmbed(statusText, latency);

                    await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embed = embed.Build());
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponse(Context.Interaction, ex);
                }
            });
        }
    }
}