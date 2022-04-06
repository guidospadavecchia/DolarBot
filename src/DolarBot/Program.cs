using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Interactions;
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
using System.Threading;
using System.Threading.Tasks;

namespace DolarBot
{
    using FergunInteractiveService = Fergun.Interactive.InteractiveService;

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
                /* TODO: Remove GuildMessages after it becomes privileged intent and all commands are replaced by slash commands */
                GatewayIntents = GatewayIntents.GuildMessages | GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessageTyping | GatewayIntents.DirectMessages | GatewayIntents.DirectMessageReactions | GatewayIntents.DirectMessageTyping,
            });
            CommandService commandsService = new();
            InteractionService interactionService = new(client);
            FergunInteractiveService fergunInteractiveService = new(client);

            using ServiceProvider services = ConfigureServices(client, commandsService, interactionService, fergunInteractiveService, api);
            
            string commandPrefix = Configuration["commandPrefix"];
            string token = GlobalConfiguration.GetToken(Configuration);

            PrintCurrentVersion();
            await RegisterEventsAsync(client, commandsService, interactionService, fergunInteractiveService, services);
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            await client.SetGameAsync(GlobalConfiguration.GetStatusText(), type: ActivityType.Listening);

            await Task.Delay(Timeout.Infinite);
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
        private static void PrintCurrentVersion()
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
        private static void ConfigureLogger()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(GlobalConfiguration.GetLogConfigFileName()));
        }

        /// <summary>
        /// Configures all the required services and returns a built service provider.
        /// </summary>
        /// <param name="discordClient">The <see cref="DiscordSocketClient"/> instance.</param>
        /// <param name="commandsService">The <see cref="CommandService"/> instance.</param>
        /// <param name="interactionService">The <see cref="InteractionService"/> instance.</param>
        /// <param name="fergunInteractiveService">The interactive service to handle pagination and selections.</param>
        /// <param name="api">The <see cref="ApiCalls"/> instance.</param>
        /// <returns>A built service provider.</returns>
        private ServiceProvider ConfigureServices(DiscordSocketClient discordClient, CommandService commandsService, InteractionService interactionService, FergunInteractiveService fergunInteractiveService, ApiCalls api)
        {
            return new ServiceCollection().AddSingleton(discordClient)
                                          .AddSingleton(commandsService)
                                          .AddSingleton(interactionService)
                                          .AddSingleton(fergunInteractiveService)
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
        /// <param name="commandsService">The <see cref="CommandService"/> object.</param>
        /// <param name="interactionService">The <see cref="InteractionService"/> object.</param>
        /// <param name="fergunInteractiveService">The interactive service to handle pagination and selections.</param>
        /// <param name="services">A collection of services to use throughout the application.</param>
        /// <returns>A task with the result of the asynchronous operation.</returns>
        private async Task RegisterEventsAsync(DiscordSocketClient client, CommandService commandsService, InteractionService interactionService, FergunInteractiveService fergunInteractiveService, IServiceProvider services)
        {

            ClientHandler clientHandler = new(client, interactionService, Configuration, logger: logger, isDebug: Debugger.IsAttached);
            CommandHandler commandHandler = new(client, commandsService, services, Configuration, logger);
            InteractionHandler interactionHandler = new(client, interactionService, fergunInteractiveService, services, Configuration, logger);

            client.Log += LogClientEvent;
            client.Ready += clientHandler.OnReady;
            client.JoinedGuild += clientHandler.OnGuildCountChanged;
            client.LeftGuild += clientHandler.OnGuildCountChanged;
            client.MessageReceived += commandHandler.HandleCommandAsync;
            client.InteractionCreated += interactionHandler.HandleInteractionAsync;
            interactionService.SlashCommandExecuted += interactionHandler.HandleSlashCommandAsync;
            interactionService.ContextCommandExecuted += interactionHandler.HandleContextCommandAsync;
            interactionService.ComponentCommandExecuted += interactionHandler.HandleComponentCommandAsync;

            Task addCommandModules = commandsService.AddModulesAsync(Assembly.GetAssembly(typeof(CommandHandler)), services);
            Task addInteractionCommmandModules = interactionService.AddModulesAsync(Assembly.GetAssembly(typeof(InteractionHandler)), services);

            await Task.WhenAll(addCommandModules, addInteractionCommmandModules);
        }

        #endregion
    }
}
