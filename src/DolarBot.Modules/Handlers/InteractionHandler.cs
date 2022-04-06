using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DolarBot.Modules.Handlers
{
    /// <summary>
    /// Handles Discord interaction related events, such as slash and context commands.
    /// </summary>
    /// <see cref="https://github.com/discord-net/Discord.Net/blob/dev/samples/04_interactions_framework/CommandHandler.cs"/>
    public class InteractionHandler
    {
        #region Vars
        /// <summary>
        /// The current Discord client instance.
        /// </summary>
        private readonly DiscordSocketClient Client;
        /// <summary>
        /// Service which provides access to the available commands.
        /// </summary>
        private readonly InteractionService InteractionCommands;
        /// <summary>
        /// The interactive service to handle pagination and selections.
        /// </summary>
        private readonly InteractiveService InteractiveService;
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
        /// Interaction handler constructor.
        /// </summary>
        /// <param name="client">The current <see cref="DiscordSocketClient"/>.</param>
        /// <param name="interactionCommands">The Discord interaction command service.</param>
        /// <param name="InteractiveService">The interactive service to handle pagination and selections.</param>
        /// <param name="services">The service provider.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> object to access application settings.</param>
        /// <param name="logger">The log4net <see cref="ILog"/> instance.</param>
        public InteractionHandler(DiscordSocketClient client, InteractionService interactionCommands, InteractiveService interactiveService, IServiceProvider services, IConfiguration configuration, ILog logger = null)
        {
            Client = client;
            InteractionCommands = interactionCommands;
            InteractiveService = interactiveService;
            Services = services;
            Configuration = configuration;
            Logger = logger;
        }
        #endregion

        #region Events

        /// <summary>
        /// Retrieves the Component Command result and acts accordingly in case of error.
        /// </summary>
        /// <param name="_">The received <see cref="ComponentCommandInfo"/>.</param>
        /// <param name="arg2">The interaction context.</param>
        /// <param name="arg3">The interaction result.</param>
        /// <returns>A task that represents the asynchronous execution operation. The task contains the result of the command execution.</returns>
        public async Task HandleComponentCommandAsync(ComponentCommandInfo _, IInteractionContext arg2, IResult arg3) => await HandleCommandAsync(arg2, arg3);

        /// <summary>
        /// Retrieves the Context Command result and acts accordingly in case of error.
        /// </summary>
        /// <param name="_">The received <see cref="ContextCommandInfo"/>.</param>
        /// <param name="arg2">The interaction context.</param>
        /// <param name="arg3">The interaction result.</param>
        /// <returns>A task that represents the asynchronous execution operation. The task contains the result of the command execution.</returns>
        public async Task HandleContextCommandAsync(ContextCommandInfo _, IInteractionContext arg2, IResult arg3) => await HandleCommandAsync(arg2, arg3);

        /// <summary>
        /// Retrieves the Slash Command result and acts accordingly in case of error.
        /// </summary>
        /// <param name="_">The received <see cref="SlashCommandInfo"/>.</param>
        /// <param name="arg2">The interaction context.</param>
        /// <param name="arg3">The interaction result.</param>
        /// <returns>A task that represents the asynchronous execution operation. The task contains the result of the command execution.</returns>
        public async Task HandleSlashCommandAsync(SlashCommandInfo _, IInteractionContext arg2, IResult arg3) => await HandleCommandAsync(arg2, arg3);

        /// <summary>
        /// Processes and executes the interaction command.
        /// </summary>
        /// <param name="socketInteraction">The <see cref="SocketInteraction"/> object.</param>
        /// <returns>A task that represents the asynchronous execution operation. The task contains the result of the command execution.</returns>
        public async Task HandleInteractionAsync(SocketInteraction socketInteraction)
        {
            try
            {
                bool isInteractiveServiceCallback = socketInteraction is SocketMessageComponent messageComponent && InteractiveService.Callbacks.ContainsKey(messageComponent.Message.Id);
                if (!isInteractiveServiceCallback)
                {
                    SocketInteractionContext context = new(Client, socketInteraction);
                    await InteractionCommands.ExecuteCommandAsync(context, Services);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Logger.Error(ex);

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist.
                // It is a good idea to delete the original response, or at least let the user know that something went wrong during the command execution.
                if (socketInteraction.Type == InteractionType.ApplicationCommand)
                {
                    await socketInteraction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves the command result and acts accordingly in case of error.
        /// </summary>
        /// <param name="context">The interaction context.</param>
        /// <param name="result">The interaction result.</param>
        /// <returns>A task that represents the asynchronous execution operation. The task contains the result of the command execution.</returns>
        private async Task HandleCommandAsync(IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.BadArgs:
                        await ProcessBadArgs(context);
                        break;
                    case InteractionCommandError.Exception:
                    case InteractionCommandError.Unsuccessful:
                    case InteractionCommandError.UnknownCommand:
                    case InteractionCommandError.UnmetPrecondition:
                        ProcessCommandError(result);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Processes an invalid command and notifies the user.
        /// </summary>
        /// <param name="context">The current command context.</param>
        /// <returns>A task that represents the asynchronous execution operation. The task result contains the result of the command execution.</returns>
        private async Task ProcessBadArgs(IInteractionContext context)
        {
            string commandPrefix = Configuration["commandPrefix"];
            await context.Interaction.RespondAsync($"Error al ejecutar el comando. Verificá los parámetros con {Format.Bold($"{commandPrefix}ayuda")}.");
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
