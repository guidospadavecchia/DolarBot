using Discord.WebSocket;
using DiscordBotsList.Api;
using DiscordBotsList.Api.Objects;
using DolarBot.Util;
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
        /// Allows access to application settings.
        /// </summary>
        private readonly IConfiguration Configuration;
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private readonly ILog Logger;
        #endregion

        #region Constructors

        /// <summary>
        /// Client handler constructor.
        /// </summary>
        /// <param name="client">The current <see cref="DiscordSocketClient"/>.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> object to access application settings.</param>
        /// <param name="logger">The log4net <see cref="ILog"/> instance.</param>
        public ClientHandler(DiscordSocketClient client, IConfiguration configuration, ILog logger = null)
        {
            Client = client;
            Configuration = configuration;
            Logger = logger;
        }
        #endregion

        #region Events

        /// <summary>
        /// Executed when the bot enters ready state.
        /// </summary>
        /// <returns></returns>
        public async Task OnReady()
        {
            try
            {
                await UpdateServerLogAsync(false).ConfigureAwait(false);

                await UpdateStatsDbl();
            }
            catch (Exception ex)
            {
                Logger.Error("Error updating server log", ex);
            }
        }

        /// <summary>
        /// Executed when the bot joins or leaves a guild.
        /// </summary>
        /// <param name="socketGuild">The <see cref="SocketGuild"/> the bot joined or left.</param>
        /// <returns>A completed task</returns>
        public async Task OnGuildCountChanged(SocketGuild _)
        {
            List<Task> tasks = new List<Task>();
            bool shouldUpdateDbl = bool.TryParse(Configuration["useDbl"], out bool useDbl) && useDbl;
            if (shouldUpdateDbl)
            {
                Task updateStatsTask = UpdateStatsDbl();
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
        private async Task UpdateServerLogAsync(bool overwrite = true)
        {
            try
            {
                string serverListFile = Configuration["serverListLog"];
                if (!string.IsNullOrEmpty(serverListFile))
                {
                    await Task.Run(() =>
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
                            File.WriteAllLines(serverListFile, servers);
                        }
                    });
                }
                else
                {
                    Logger.Error("Cannot write to Server list log: Not configured.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error updating server log", ex);
            }
        }

        /// <summary>
        /// Sends a request to DBL (top.gg) to update the bot stats.
        /// </summary>
        /// <returns>A completed task.</returns>
        private async Task UpdateStatsDbl()
        {
            try
            {
                string dblToken = GlobalConfiguration.GetDblToken(Configuration);
                bool isConfigured = ulong.TryParse(Configuration["botDiscordId"], out ulong botDiscordId);
                if (isConfigured)
                {
                    AuthDiscordBotListApi dblApi = new AuthDiscordBotListApi(botDiscordId, dblToken);
                    IDblSelfBot self = await dblApi.GetMeAsync();

                    await self.UpdateStatsAsync(Client.Guilds.Count);
                }
                else
                {
                    Logger.Error("Cannot update DBL stats: Not configured.");
                }
            }
            catch (SystemException ex)
            {
                Logger.Error("Error updating DBL stats.", ex);
            }
        }

        #endregion
    }
}
