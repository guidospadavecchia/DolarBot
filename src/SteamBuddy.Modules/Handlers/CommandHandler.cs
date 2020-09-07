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

        private readonly ILog logger;
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services;

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
                if (!result.IsSuccess)
                {
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                    if (logger != null)
                    {
                        logger.Error(result.ErrorReason); 
                    }
                }
            }
        }
    }
}
