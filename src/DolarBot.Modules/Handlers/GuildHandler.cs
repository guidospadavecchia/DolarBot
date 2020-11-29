using Discord.WebSocket;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.Handlers
{
    /// <summary>
    /// Handles guild-related events.
    /// </summary>
    public class GuildHandler
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
        /// Guild handler constructor.
        /// </summary>
        /// <param name="client">The current <see cref="DiscordSocketClient"/>.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> object to access application settings.</param>
        /// <param name="logger">The log4net <see cref="ILog"/> instance.</param>
        public GuildHandler(DiscordSocketClient client, IConfiguration configuration, ILog logger = null)
        {
            Client = client;
            Configuration = configuration;
            Logger = logger;
        }
        #endregion

        #region Events

        /// <summary>
        /// Updates the log file with the current servers.
        /// </summary>
        /// <param name="socketGuild">A <see cref="SocketGuild"/> object.</param>
        /// <returns>A completed task</returns>
        public Task UpdateServerLog(SocketGuild _)
        {
            try
            {
                string serverListFile = Configuration["serverListLog"];
                if (!string.IsNullOrEmpty(serverListFile))
                {
                    string directory = Path.GetDirectoryName(serverListFile);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    var servers = Client.Guilds.Select(x => $"[{x.Id}] {x.Name}");
                    File.WriteAllLines(serverListFile, servers);
                }
                else
                {
                    Logger.Error("Cannot write to Server list log: Not configured.");
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.Error("Error updating server log", ex);
                return Task.CompletedTask;
            }
        }

        #endregion

        #region Methods

        #endregion
    }
}
