using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using System;
using System.Threading.Tasks;

namespace SteamBuddy.Modules.Handlers
{
    public class CommandHandler
    {
        private const string COMMAND_PREFIX = "-";

        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services;
        private readonly ILog logger;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, ILog logger = null)
        {
            this.client = client;
            this.commands = commands;
            this.services = services;
            this.logger = logger;
        }

        public async Task HandleCommandAsync(SocketMessage arg)
        {
            SocketUserMessage message = arg as SocketUserMessage;
            SocketCommandContext context = new SocketCommandContext(client, message);

            if (message.Author.IsBot)
            {
                return;
            }

            int argPos = default;
            if (message.HasStringPrefix(COMMAND_PREFIX, ref argPos))
            {
                IResult result = await commands.ExecuteAsync(context, argPos, services);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    string commandName = Format.Bold(commands.Search(context, argPos).Text);
                    switch (result.Error)
                    {
                        case CommandError.BadArgCount:
                            await context.Channel.SendMessageAsync($"Syntax error for {commandName} command. Check parameters within {Format.Bold("-help")}.");
                            break;
                        case CommandError.Exception:
                        case CommandError.Unsuccessful:
                        case CommandError.ParseFailed:
                            if (logger != null)
                            {
                                logger.Error($"Error executing command: {result.ErrorReason}");
                            }
                            break;
                        default:
                            break;
                    }

                }
            }
        }
    }
}
