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
    public class CommandHandler
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services;
        private readonly IConfiguration configuration;
        private readonly ILog logger;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, IConfiguration configuration, ILog logger = null)
        {
            this.client = client;
            this.commands = commands;
            this.services = services;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task HandleCommandAsync(SocketMessage arg)
        {
            SocketUserMessage message = arg as SocketUserMessage;

            if (message == null || message.Author.IsBot)
            {
                return;
            }

            SocketCommandContext context = new SocketCommandContext(client, message);
            int argPos = default;
            if (message.HasStringPrefix(configuration["commandPrefix"], ref argPos))
            {
                IResult result = await commands.ExecuteAsync(context, argPos, services);
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
        }

        private string GetCommandSummary(string commandName)
        {
            var command = commands.Commands.FirstOrDefault(c => c.Name.ToUpper().Trim().Equals(commandName.ToUpper().Trim()) || c.Aliases.Select(a => a.ToUpper().Trim()).Contains(commandName.ToUpper().Trim()));
            return command?.Summary;
        }

        private async Task ProcessBadArgCount(SocketCommandContext context, int argPos)
        {
            string commandPrefix = configuration["commandPrefix"];
            string commandName = commands.Search(context, argPos).Text;
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

        private void ProcessCommandError(IResult result)
        {
            if (logger != null)
            {
                if (result is ExecuteResult executeResult)
                {
                    logger.Error($"Error executing command: {executeResult.Exception.Message}");
                    logger.Error(executeResult.Exception.StackTrace);
                }
                else
                {
                    logger.Error($"Error executing command: {result.ErrorReason}");
                }
            }
        }
    }
}
