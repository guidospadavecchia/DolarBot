using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DolarBot.API;
using DolarBot.Modules.Handlers;
using DolarBot.Util;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            try
            {
                IConfiguration configuration = ConfigureAppSettings();
                ConfigureLogger();

                ApiCalls api = new ApiCalls(configuration, logger);
                DiscordSocketClient client = new DiscordSocketClient();
                CommandService commands = new CommandService();

                IServiceProvider services = new ServiceCollection().AddSingleton(client)
                                                                   .AddSingleton(commands)
                                                                   .AddSingleton(configuration)
                                                                   .AddSingleton<InteractiveService>()
                                                                   .AddSingleton(api)
                                                                   .BuildServiceProvider();
                string token = GetToken(configuration);

                client.Log += LogClientEvent;
                
                PrintCurrentVersion();
                await RegisterCommandsAsync(client, commands, services, configuration).ConfigureAwait(false);
                await client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
                await client.StartAsync().ConfigureAwait(false);
                await client.SetGameAsync(GlobalConfiguration.GetStatusText(), type: ActivityType.Listening).ConfigureAwait(false);

                await Task.Delay(-1).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error("Application error", ex);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Redirects log messages to console.
        /// </summary>
        /// <param name="logMessage">Incoming log message.</param>
        /// <returns>A completed task.</returns>
        private Task LogClientEvent(LogMessage logMessage)
        {
            Console.WriteLine(logMessage);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Prints the current assembly version to console.
        /// </summary>
        private void PrintCurrentVersion()
        {
            string assemblyVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
            Console.WriteLine($"DolarBot v{assemblyVersion}");
            Console.WriteLine();
        }

        /// <summary>
        /// Creates an <see cref="IConfiguration"/> object to access application settings.
        /// </summary>
        /// <returns></returns>
        public IConfiguration ConfigureAppSettings()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(GlobalConfiguration.GetAppSettingsFileName());
            return builder.Build();
        }

        /// <summary>
        /// Configures the file logger.
        /// </summary>
        public void ConfigureLogger()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(GlobalConfiguration.GetLogConfigFileName()));
        }

        /// <summary>
        /// Handles the commands events and suscribes modules to the <see cref="CommandService"/>.
        /// </summary>
        /// <param name="client">The Discord client.</param>
        /// <param name="commands">The <see cref="CommandService"/> object.</param>
        /// <param name="services">A collection of services to use throughout the application.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> object to access application settings.</param>
        /// <returns>A task with the result of the asynchronous operation.</returns>
        public async Task RegisterCommandsAsync(DiscordSocketClient client, CommandService commands, IServiceProvider services, IConfiguration configuration)
        {
            CommandHandler commandHandler = new CommandHandler(client, commands, services, configuration, logger);
            client.MessageReceived += commandHandler.HandleCommandAsync;
            await commands.AddModulesAsync(Assembly.GetAssembly(typeof(CommandHandler)), services);
        }

        /// <summary>
        /// Retrieves the bot's token from the application settings or operating system's enviromental variable.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> object to access application settings.</param>
        /// <returns>The retrieved token.</returns>
        public string GetToken(IConfiguration configuration)
        {
            string token = configuration["token"];
            if (string.IsNullOrWhiteSpace(token))
            {
                token = Environment.GetEnvironmentVariable(GlobalConfiguration.GetTokenEnvVarName());
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new SystemException("Missing token");
                }
            }

            return token;
        }

        #endregion
    }
}
