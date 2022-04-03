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
        /// Modifies the original deferred response by sending an embed message.
        /// </summary>
        /// <param name="embed">The embed to be sent.</param>
        protected async Task SendDeferredEmbed(Embed embed)
        {
            await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embed = embed);
        }

        /// <summary>
        /// Modifies the original deferred response by sending multiple embed messages.
        /// </summary>
        /// <param name="embeds">The embed messages to be sent.</param>
        protected async Task SendDeferredEmbed(Embed[] embeds)
        {
            await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embeds = embeds);
        }

        /// <summary>
        /// Modifies the original deferred response by sending a message.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        protected async Task SendDeferredMessage(string message)
        {
            await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Content = message);
        }

        /// <summary>
        /// Modifies the original deferred response by sending a message indicating an error has ocurred with the API.
        /// </summary>
        /// <returns></returns>
        protected async Task SendDeferredApiErrorResponse()
        {
            await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Content = RequestErrorMessage);
        }

        /// <summary>
        /// Sends a reply indicating an error has occurred, by modifying the original deferred response.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        protected async Task SendDeferredErrorResponse(Exception ex)
        {
            string errorMessage = GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]);
            await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Content = errorMessage);
            Logger.Error("Error al ejecutar comando.", ex);
        }
        #endregion
    }
}
