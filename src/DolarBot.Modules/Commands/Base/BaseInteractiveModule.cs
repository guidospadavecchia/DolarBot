using Discord.Addons.Interactive;
using Discord.Commands;
using DolarBot.Util;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands.Base
{
    /// <summary>
    /// Base class for interactive modules.
    /// </summary>
    public class BaseInteractiveModule : InteractiveBase<SocketCommandContext>
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
        protected async Task SendErrorReply(Exception ex)
        {
            await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
            Logger.Error("Error al ejecutar comando.", ex);
        }

        #endregion
    }
}
