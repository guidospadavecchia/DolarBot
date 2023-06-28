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
        protected Task FollowUpWithPaginatedEmbedAsync(IEnumerable<EmbedBuilder> embedBuilders) => FollowUpWithPaginatedEmbedAsync(embedBuilders.Select(x => x.Build()).ToArray());

        /// <summary>
        /// Follows up an interaction with a deferred paginated message.
        /// </summary>
        /// <param name="embeds">The embed pages.</param>
        protected async Task FollowUpWithPaginatedEmbedAsync(Embed[] embeds)
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
            await InteractiveService.SendPaginatorAsync(paginator, Context.Interaction, responseType: InteractionResponseType.DeferredChannelMessageWithSource, resetTimeoutOnInput: true, timeout: TimeSpan.FromSeconds(paginatorTimeout));
        }

        /// <summary>
        /// Follows up to the original deferred response by sending a message indicating an error has ocurred with the API.
        /// </summary>
        /// <returns></returns>
        protected async Task FollowUpWithApiErrorResponseAsync()
        {
            await Context.Interaction.FollowupAsync(RequestErrorMessage);
        }

        /// <summary>
        /// Sends a reply indicating an error has occurred, by following up the original deferred response.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        protected async Task FollowUpWithErrorResponseAsync(Exception ex)
        {
            string errorMessage = GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]);
            await Context.Interaction.FollowupAsync(errorMessage);
            Logger.Error("Error executing command", ex);
            Console.WriteLine($"Error executing command: {ex}");
        }

        #endregion
    }
}
