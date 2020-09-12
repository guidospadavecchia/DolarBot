﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
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

                await RegisterCommandsAsync(client, commands, services, configuration);

                string token = configuration["token"];
                if (token == null)
                {
                    throw new SystemException("Token is missing from configuration file");
                }

                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();
                await client.SetGameAsync(GlobalConfiguration.GetStatusText(), type: ActivityType.Listening);

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
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(GlobalConfiguration.GetAppSettingsFileName());
            return builder.Build();
        }

        public void ConfigureLogger()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(GlobalConfiguration.GetLogConfigFileName()));
        }

        public async Task RegisterCommandsAsync(DiscordSocketClient client, CommandService commands, IServiceProvider services, IConfiguration configuration)
        {
            CommandHandler commandHandler = new CommandHandler(client, commands, services, configuration, logger);
            client.MessageReceived += commandHandler.HandleCommandAsync;
            await commands.AddModulesAsync(Assembly.GetAssembly(typeof(CommandHandler)), services);
        }

        #endregion
    }
}
