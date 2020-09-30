using Discord.Addons.Interactive;
using Discord.Commands;
using Microsoft.Extensions.Configuration;

namespace DolarBot.Modules.Commands.Base
{
    /// <summary>
    /// Base class for interactive modules.
    /// </summary>
    public class BaseInteractiveModule : InteractiveBase<SocketCommandContext>
    {
        #region Vars
        /// <summary>
        /// Provides access to application settings.
        /// </summary>
        protected readonly IConfiguration Configuration;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the object using the <see cref="IConfiguration"/> object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        public BaseInteractiveModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        #endregion
    }
}
