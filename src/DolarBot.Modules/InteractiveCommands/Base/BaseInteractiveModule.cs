using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.InteractiveCommands.Base
{
    public class BaseInteractiveModule : InteractionModuleBase
    {
        #region Vars
        /// <summary>
        /// The message to show when there is an error with the API.
        /// </summary>
        private readonly string RequestErrorMessage = $"{Format.Bold("Error")}: No se pudo obtener la cotización ya que el servicio se encuentra inaccesible. Intente nuevamente en más tarde.";
        /// <summary>
        /// Provides access to application settings.
        /// </summary>
        protected readonly IConfiguration Configuration;
        /// <summary>
        /// The log4net logger.
        /// </summary>
        protected readonly ILog Logger;

        /// <summary>
        /// The service to interact with messages.
        /// </summary>
        private readonly InteractiveService InteractiveService;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the object using the <see cref="IConfiguration"/> object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="logger">The log4net logger.</param>
        /// <param name="interactiveService">The interactive service.</param>
        public BaseInteractiveModule(IConfiguration configuration, ILog logger, InteractiveService interactiveService)
        {
            Configuration = configuration;
            Logger = logger;
            InteractiveService = interactiveService;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Sends a deferred paginated message from an <see cref="IEnumerable{T}"/> of <see cref="EmbedBuilder"/> objects.
        /// </summary>
        /// <param name="embedBuilders">The embed builders.</param>
        protected Task SendDeferredPaginatedEmbedAsync(IEnumerable<EmbedBuilder> embedBuilders)
        {
            return SendDeferredPaginatedEmbedAsync(embedBuilders.Select(x => x.Build()).ToArray());
        }

        /// <summary>
        /// Sends a deferred paginated message.
        /// </summary>
        /// <param name="embeds">The embed pages.</param>
        protected async Task SendDeferredPaginatedEmbedAsync(Embed[] embeds)
        {
            List<PageBuilder> pages = new();
            foreach (Embed embed in embeds)
            {
                PageBuilder pageBuilder = new PageBuilder().WithTitle(embed.Title)
                                                           .WithDescription(embed.Description);
                foreach (EmbedField field in embed.Fields)
                {
                    pageBuilder.AddField(new EmbedFieldBuilder()
                    {
                        Name = field.Name,
                        Value = field.Value,
                        IsInline = field.Inline,
                    });
                }
                if (embed.Color.HasValue)
                {
                    pageBuilder.WithColor(embed.Color.Value);
                }
                if (embed.Thumbnail.HasValue)
                {
                    pageBuilder.WithThumbnailUrl(embed.Thumbnail.Value.Url);
                }
                if (embed.Footer.HasValue)
                {
                    pageBuilder.WithFooter(new EmbedFooterBuilder()
                    {
                        Text = embed.Footer.Value.Text,
                        IconUrl = embed.Footer.HasValue ? embed.Footer.Value.IconUrl : null,
                    });
                }
                pages.Add(pageBuilder);
            }

            int paginatorTimeout = Convert.ToInt32(Configuration["paginatorTimeout"]);
            StaticPaginator paginator = new StaticPaginatorBuilder()
                                            .WithPages(pages)
                                            .WithDefaultButtons(Configuration)
                                            .WithJumpInputPrompt("Navegación de páginas")
                                            .WithJumpInputTextLabel($"Navegar a la página (1-{pages.Count}):")
                                            .WithExpiredJumpInputMessage("La interacción expiró. Por favor, ejecutá el comando nuevamente para volver a interactuar.")
                                            .WithInvalidJumpInputMessage($"La página a ingresar debe ser un número entre {Format.Bold("1")} y {Format.Bold(pages.Count.ToString())}.")
                                            .WithJumpInputInUseMessage("La función para saltar de página se encuentra actualmente en uso por otro usuario")
                                            .Build();
            await InteractiveService.SendPaginatorAsync(paginator, Context.Interaction as SocketInteraction, responseType: InteractionResponseType.DeferredChannelMessageWithSource, resetTimeoutOnInput: true, timeout: TimeSpan.FromSeconds(paginatorTimeout));
        }

        /// <summary>
        /// Modifies the original deferred response by sending an embed message.
        /// </summary>
        /// <param name="embed">The embed to be sent.</param>
        /// <param name="components">Optional components.</param>
        protected async Task SendDeferredEmbedAsync(Embed embed, Optional<MessageComponent> components = default)
        {
            await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) =>
            {
                messageProperties.Embed = embed;
                messageProperties.Components = components;
            });
        }

        /// <summary>
        /// Modifies the original deferred response by sending multiple embed messages.
        /// </summary>
        /// <param name="embeds">The embed messages to be sent.</param>
        protected async Task SendDeferredEmbedAsync(Embed[] embeds)
        {
            await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embeds = embeds);
        }

        /// <summary>
        /// Modifies the original deferred response by sending a message.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        protected async Task SendDeferredMessageAsync(string message, Optional<MessageComponent> components = default)
        {
            await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) =>
            {
                messageProperties.Content = message;
                messageProperties.Components = components;
            });
        }

        /// <summary>
        /// Modifies the original deferred response by sending a message indicating an error has ocurred with the API.
        /// </summary>
        /// <returns></returns>
        protected async Task SendDeferredApiErrorResponseAsync()
        {
            await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Content = RequestErrorMessage);
        }

        /// <summary>
        /// Sends a reply indicating an error has occurred, by modifying the original deferred response.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        protected async Task SendDeferredErrorResponseAsync(Exception ex)
        {
            string errorMessage = GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]);
            await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Content = errorMessage);
            Logger.Error("Error al ejecutar comando.", ex);
        }

        #endregion
    }
}
