using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using SteamBuddy.Modules.Handlers;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SteamBuddy
{
    public class Program
    {
        #region Constants
        private const string PRIVATE_TOKEN = "NzUyMzEzMzg2MzUxNDYwMzcy.X1V0cA.ZhBCx0k6JXekfEc3putSM8MHebk";
        private const string LOG4NET_CONFIG_FILENAME = "log4net.config";
        #endregion

        #region Vars
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Startup

        public static void Main(string[] args) => new Program().RunAsync().GetAwaiter().GetResult();

        public async Task RunAsync()
        {
            ConfigureLogger();

            DiscordSocketClient client = new DiscordSocketClient();
            CommandService commands = new CommandService();
            IServiceProvider services = new ServiceCollection().AddSingleton(client).AddSingleton(commands).BuildServiceProvider();

            client.Log += LogClientEvent;

            await RegisterCommandsAsync(client, commands, services);

            await client.LoginAsync(TokenType.Bot, PRIVATE_TOKEN);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private Task LogClientEvent(LogMessage logMessage)
        {
            Console.WriteLine(logMessage);
            return Task.CompletedTask;
        }

        #endregion

        #region Methods

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
