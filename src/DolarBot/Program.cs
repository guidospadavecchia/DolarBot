using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DolarBot.API;
using DolarBot.Modules.Handlers;
using DolarBot.Services.Quotes;
using DolarBot.Util;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DolarBot
{
    public class Program
    {
        #region Vars
        /// <summary>
        /// Log4Net logger.
        /// </summary>
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Provides access to application settings.
        /// </summary>
        protected IConfiguration Configuration;
        #endregion

        #region Startup

        /// <summary>
        /// Program starting point. Creates an asynchronous context.
        /// </summary>
        public static void Main() => new Program().RunAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Program's asynchronous startup.
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync()
        {
            GlobalConfiguration.Initialize();
            ConfigureAppSettings();
            ConfigureLogger();
            QuoteService.TryLoadQuotes();
            
            ApiCalls api = new(Configuration, logger);
            DiscordSocketClient client = new(new DiscordSocketConfig()
            {
                ExclusiveBulkDelete = true,
            });
            CommandService commands = new();

            IServiceProvider services = ConfigureServices(client, commands, api);
            
            string commandPrefix = Configuration["commandPrefix"];
            string token = GlobalConfiguration.GetToken(Configuration);

            PrintCurrentVersion();
            await RegisterEventsAsync(client, commands, services).ConfigureAwait(false);
            await client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
            await client.StartAsync().ConfigureAwait(false);
            await client.SetGameAsync(GlobalConfiguration.GetStatusText(commandPrefix), type: ActivityType.Listening).ConfigureAwait(false);

            await Task.Delay(-1).ConfigureAwait(false);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Redirects log messages to console and errors to log.
        /// </summary>
        /// <param name="logMessage">Incoming log message.</param>
        /// <returns>A completed task.</returns>
        private Task LogClientEvent(LogMessage logMessage)
        {
            if (logMessage.Exception != null)
            {
                logger.Error("Application error", logMessage.Exception);
            }
            Console.WriteLine(logMessage);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Prints the current assembly version to console.
        /// </summary>
        private void PrintCurrentVersion()
        {
            string assemblyVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
            Console.WriteLine(Debugger.IsAttached ? $"DolarBot v{assemblyVersion} (DEV)" : $"DolarBot v{assemblyVersion}");
            Console.WriteLine();
        }

        /// <summary>
        /// Creates an <see cref="IConfiguration"/> object to access application settings.
        /// </summary>
        private void ConfigureAppSettings()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(GlobalConfiguration.GetAppSettingsFileName());
            Configuration = builder.Build();
        }

        /// <summary>
        /// Configures the file logger.
        /// </summary>
        private void ConfigureLogger()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(GlobalConfiguration.GetLogConfigFileName()));
        }

        /// <summary>
        /// Configures all the required services and returns a built service provider.
        /// </summary>
        /// <param name="discordClient">The <see cref="DiscordSocketClient"/> instance.</param>
        /// <param name="commands">The <see cref="CommandService"/> instance.</param>
        /// <param name="api">The <see cref="ApiCalls0"/> instance.</param>
        /// <returns>A built service provider.</returns>
        private IServiceProvider ConfigureServices(DiscordSocketClient discordClient, CommandService commands, ApiCalls api)
        {
            return new ServiceCollection().AddSingleton(discordClient)
                                          .AddSingleton(commands)
                                          .AddSingleton(Configuration)
                                          .AddSingleton<InteractiveService>()
                                          .AddSingleton(api)
                                          .AddSingleton(logger)
                                          .BuildServiceProvider();
        }

        /// <summary>
        /// Handles the commands events and suscribes modules to the <see cref="CommandService"/>.
        /// </summary>
        /// <param name="client">The Discord client.</param>
        /// <param name="commands">The <see cref="CommandService"/> object.</param>
        /// <param name="services">A collection of services to use throughout the application.</param>
        /// <returns>A task with the result of the asynchronous operation.</returns>
        private async Task RegisterEventsAsync(DiscordSocketClient client, CommandService commands, IServiceProvider services)
        {
            client.Log += LogClientEvent;

            ClientHandler clientHandler = new(client, Configuration, logger);
            client.Ready += clientHandler.OnReady;
            client.JoinedGuild += clientHandler.OnGuildCountChanged;
            client.LeftGuild += clientHandler.OnGuildCountChanged;

            CommandHandler commandHandler = new(client, commands, services, Configuration, logger);
            client.MessageReceived += commandHandler.HandleCommandAsync;
            await commands.AddModulesAsync(Assembly.GetAssembly(typeof(CommandHandler)), services);
        }

        #endregion
    }
}
