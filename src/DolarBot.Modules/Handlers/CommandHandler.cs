using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.Handlers
{
    /// <summary>
    /// Contains methods and events to process the bot's commands.
    /// </summary>
    public class CommandHandler
    {
        #region Vars
        /// <summary>
        /// The current Discord client instance.
        /// </summary>
        private readonly DiscordSocketClient Client;
        /// <summary>
        /// Service which provides access to the available commands.
        /// </summary>
        private readonly CommandService Commands;
        /// <summary>
        /// Provides access to the different services instances.
        /// </summary>
        private readonly IServiceProvider Services;
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
        /// Command handler constructor.
        /// </summary>
        /// <param name="client">The current <see cref="DiscordSocketClient"/>.</param>
        /// <param name="commands">The Discord command service.</param>
        /// <param name="services">The service provider.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> object to access application settings.</param>
        /// <param name="logger">The log4net <see cref="ILog"/> instance.</param>
        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, IConfiguration configuration, ILog logger = null)
        {
            Client = client;
            Commands = commands;
            Services = services;
            Configuration = configuration;
            Logger = logger;
        }
        #endregion

        #region Events

        /// <summary>
        /// Processes user input and, if valid, executes the command.
        /// </summary>
        /// <param name="arg">The received <see cref="SocketMessage"/>.</param>
        /// <returns>A task that represents the asynchronous execution operation. The task result contains the result of the command execution.</returns>
        public Task HandleCommand(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message) || message.Author.IsBot)
            {
                return Task.CompletedTask;
            }

            _ = Task.Run(async () =>
            {
                SocketCommandContext context = new SocketCommandContext(Client, message);
                int argPos = default;
                if (!context.IsPrivate && message.HasStringPrefix(Configuration["commandPrefix"], ref argPos))
                {
                    IResult result = await Commands.ExecuteAsync(context, argPos, Services);
                    if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    {
                        switch (result.Error)
                        {
                            case CommandError.BadArgCount:
                                await ProcessBadArgCount(context, argPos);
                                break;
                            case CommandError.Exception:
                            case CommandError.Unsuccessful:
                            case CommandError.ParseFailed:
                                ProcessCommandError(result);
                                break;
                            default:
                                break;
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Retrieves the summary for a particular command.
        /// </summary>
        /// <param name="commandName">The command name or alias.</param>
        /// <returns>The command's summary if found, otherwise null.</returns>
        private string GetCommandSummary(string commandName)
        {
            var command = Commands.Commands.FirstOrDefault(c => c.Name.ToUpper().Trim().Equals(commandName.ToUpper().Trim()) || c.Aliases.Select(a => a.ToUpper().Trim()).Contains(commandName.ToUpper().Trim()));
            return command?.Summary;
        }

        /// <summary>
        /// Processes an invalid command and notifies the user.
        /// </summary>
        /// <param name="context">The current command context.</param>
        /// <param name="argPos">The position of which the command starts at.</param>
        /// <returns>A task that represents the asynchronous execution operation. The task result contains the result of the command execution.</returns>
        private async Task ProcessBadArgCount(SocketCommandContext context, int argPos)
        {
            string commandPrefix = Configuration["commandPrefix"];
            string commandName = Commands.Search(context, argPos).Text;
            string commandSummary = GetCommandSummary(commandName);
            if (!string.IsNullOrWhiteSpace(commandSummary))
            {
                await context.Channel.SendMessageAsync(Format.Italics(commandSummary));
            }
            else
            {
                await context.Channel.SendMessageAsync($"Error al ejecutar el comando {Format.Bold($"{commandPrefix}{commandName}")}. Verificá los parámetros con {Format.Bold($"{commandPrefix}ayuda")}.");
            }
        }

        /// <summary>
        /// Processes an error on a command invocation, logging the exception.
        /// </summary>
        /// <param name="result">The execution result.</param>
        private void ProcessCommandError(IResult result)
        {
            if (Logger != null)
            {
                if (result is ExecuteResult executeResult)
                {
                    Logger.Error($"Error executing command: {executeResult.Exception.Message}");
                    Logger.Error(executeResult.Exception.StackTrace);
                }
                else
                {
                    Logger.Error($"Error executing command: {result.ErrorReason}");
                }
            }
        }
        #endregion
    }
}
