using Discord.Addons.Interactive;
using Discord.Commands;
using Microsoft.Extensions.Configuration;

namespace DolarBot.Modules.Commands.Base
{
    public class BaseInteractiveModule : InteractiveBase<SocketCommandContext>
    {
        #region Vars
        protected readonly IConfiguration Configuration;
        #endregion

        #region Constructor
        public BaseInteractiveModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        #endregion
    }
}
