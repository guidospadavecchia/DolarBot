using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DolarBot.Modules.Handlers;
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
        #region Constants
        private const string CONFIG_FILENAME = "appsettings.json";
        private const string LOG4NET_CONFIG_FILENAME = "log4net.config";
        private const string GAME_STATUS = "$ayuda";
        #endregion

        #region Vars
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Startup

        public static void Main() => new Program().RunAsync().GetAwaiter().GetResult();

        public async Task RunAsync()
        {
            try
            {
                IConfiguration configuration = ConfigureAppSettings();
                ConfigureLogger();

                DiscordSocketClient client = new DiscordSocketClient();
                CommandService commands = new CommandService();
                IServiceProvider services = new ServiceCollection().AddSingleton(client).AddSingleton(commands).AddSingleton(configuration).BuildServiceProvider();

                client.Log += LogClientEvent;

                await RegisterCommandsAsync(client, commands, services);

                string token = configuration["token"];
                if (token == null)
                {
                    throw new SystemException("Token is missing from configuration file");
                }

                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();
                await client.SetGameAsync(GAME_STATUS, type: ActivityType.Listening);

                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                logger.Error("Application error", ex);
            }
        }

        private Task LogClientEvent(LogMessage logMessage)
        {
            Console.WriteLine(logMessage);
            return Task.CompletedTask;
        }

        #endregion

        #region Methods

        public IConfiguration ConfigureAppSettings()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(CONFIG_FILENAME);
            return builder.Build();
        }

        public void ConfigureLogger()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(LOG4NET_CONFIG_FILENAME));
        }

        public async Task RegisterCommandsAsync(DiscordSocketClient client, CommandService commands, IServiceProvider services)
        {
            CommandHandler commandHandler = new CommandHandler(client, commands, services, logger);
            client.MessageReceived += commandHandler.HandleCommandAsync;
            await commands.AddModulesAsync(Assembly.GetAssembly(typeof(CommandHandler)), services);
        }

        #endregion
    }
}
