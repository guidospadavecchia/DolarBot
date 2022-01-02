using Discord;
using Discord.Interactions;
using DolarBot.Util;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DolarBot.Modules.InteractiveCommands.Base
{
    public class BaseInteractiveModule : InteractionModuleBase
    {
        #region Constants
        protected const string REQUEST_ERROR_MESSAGE = "**Error**: No se pudo obtener la cotización ya que el servicio se encuentra inaccesible. Intente nuevamente en más tarde.";
        #endregion

        #region Vars
        /// <summary>
        /// Provides access to application settings.
        /// </summary>
        protected readonly IConfiguration Configuration;
        /// <summary>
        /// The log4net logger.
        /// </summary>
        protected readonly ILog Logger;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the object using the <see cref="IConfiguration"/> object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        public BaseInteractiveModule(IConfiguration configuration, ILog logger)
        {
            Configuration = configuration;
            Logger = logger;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sends a reply indicating an error has occurred.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        protected async Task SendErrorResponse(Exception ex)
        {
            await RespondAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
            Logger.Error("Error al ejecutar comando.", ex);
        }

        /// <summary>
        /// Sends a reply indicating an error has occurred, by modifying the original deferred response.
        /// </summary>
        /// <param name="interaction">The interaction context.</param>
        /// <param name="ex">The exception to log.</param>
        protected async Task SendDeferredErrorResponse(IDiscordInteraction interaction, Exception ex)
        {
            string errorMessage = GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]);
            await interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Content = errorMessage);
            Logger.Error("Error al ejecutar comando.", ex);
        }
        #endregion
    }
}
