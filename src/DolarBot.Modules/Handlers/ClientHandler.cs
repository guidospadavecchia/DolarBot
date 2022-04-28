using Discord.Interactions;
using Discord.WebSocket;
using DolarBot.API;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.Handlers
{
    /// <summary>
    /// Handles Discord client related events.
    /// </summary>
    public class ClientHandler
    {
        #region Vars
        /// <summary>
        /// The current Discord client instance.
        /// </summary>
        private readonly DiscordSocketClient Client;
        /// <summary>
        /// The current Discord client instance.
        /// </summary>
        private readonly ApiCalls Api;
        /// <summary>
        /// Service which provides access to the available commands.
        /// </summary>
        private readonly InteractionService InteractionService;
        /// <summary>
        /// Allows access to application settings.
        /// </summary>
        private readonly IConfiguration Configuration;
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private readonly ILog Logger;
        /// <summary>
        /// Indicates wether the current instance is running in a debug state.
        /// </summary>
        private readonly bool IsDebug;
        #endregion

        #region Constructors

        /// <summary>
        /// Client handler constructor.
        /// </summary>
        /// <param name="client">The current <see cref="DiscordSocketClient"/>.</param>
        /// <param name="api">The API interface instance.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> object to access application settings.</param>
        /// <param name="logger">The log4net <see cref="ILog"/> instance.</param>
        public ClientHandler(DiscordSocketClient client, ApiCalls api, InteractionService interactionService, IConfiguration configuration, ILog logger = null)
        {
            Client = client;
            Api = api;
            InteractionService = interactionService;
            Configuration = configuration;
            Logger = logger;
            IsDebug = Debugger.IsAttached;
        }
        #endregion

        #region Events

        /// <summary>
        /// Executed when the bot enters ready state.
        /// </summary>
        public async Task OnReady()
        {
            try
            {
                bool shouldUpdateDbl = bool.TryParse(Configuration["useDbl"], out bool useDbl) && useDbl && !IsDebug;
                Task updateStatsDbl = shouldUpdateDbl ? UpdateStatsDblAsync() : Task.CompletedTask;
                Task updateServerLog = UpdateServerLogAsync(false);

                bool testGuildConfigured = ulong.TryParse(Configuration["testServerId"], out ulong testServerId);
                Task registerCommands = IsDebug && testGuildConfigured ? InteractionService.RegisterCommandsToGuildAsync(testServerId) : InteractionService.RegisterCommandsGloballyAsync();

                await Task.WhenAll(updateStatsDbl, updateServerLog, registerCommands);
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing", ex);
            }
        }

        /// <summary>
        /// Executed when the bot joins or leaves a guild.
        /// </summary>
        /// <param name="_">The <see cref="SocketGuild"/> the bot joined or left.</param>
        /// <returns>A completed task</returns>
        public async Task OnGuildCountChanged(SocketGuild _)
        {
            List<Task> tasks = new();
            bool shouldUpdateDbl = bool.TryParse(Configuration["useDbl"], out bool useDbl) && useDbl;
            if (shouldUpdateDbl)
            {
                Task updateStatsTask = UpdateStatsDblAsync();
                tasks.Add(updateStatsTask);
            }

            Task updateServerLogTask = UpdateServerLogAsync();
            tasks.Add(updateServerLogTask);

            await Task.WhenAll(tasks);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the server log file with the current server list.
        /// </summary>
        /// <param name="overwrite">Indicates whether to overwrite the file if exists, or do nothing.</param>
        private Task UpdateServerLogAsync(bool overwrite = true)
        {
            try
            {
                string serverListFile = Configuration["serverListLog"];
                if (!string.IsNullOrEmpty(serverListFile))
                {
                    return Task.Run(() =>
                    {
                        bool fileExists = File.Exists(serverListFile);
                        if (!fileExists || (fileExists && overwrite))
                        {

                            string directory = Path.GetDirectoryName(serverListFile);
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }
                            var servers = Client.Guilds.Select(x => $"[{x.Id}] {x.Name}");
                            File.WriteAllLinesAsync(serverListFile, servers);
                        }
                    });
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error updating server log", ex);
                return Task.FromException(ex);
            }
        }

        /// <summary>
        /// Sends a request to DBL (top.gg) to update the bot stats.
        /// </summary>
        /// <returns>A completed task.</returns>
        private Task UpdateStatsDblAsync()
        {
            try
            {
                return Task.Run(async () =>
                {
                    bool posted = await Api.Topgg.PostServerCountAsync(Client.Guilds.Count);
                    if (!posted)
                    {
                        Logger.Warn("Could not update DBL stats. Check log for errors.");
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Error updating DBL stats.", ex);
                return Task.FromException(ex);
            }
        }

        #endregion
    }
}
