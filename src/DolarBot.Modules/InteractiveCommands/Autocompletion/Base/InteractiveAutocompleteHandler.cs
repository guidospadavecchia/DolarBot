using Discord.Interactions;
using DolarBot.API;
using Microsoft.Extensions.Configuration;

namespace DolarBot.Modules.InteractiveCommands.Autocompletion.Base
{
    public abstract class InteractiveAutocompleteHandler : AutocompleteHandler
    {
        /// <summary>
        /// The maximum amount of autocomplete results that can be shown.
        /// </summary>
        protected const int MAX_AUTOCOMPLETE_RESULTS = 20;

        /// <summary>
        /// Provides access to application settings.
        /// </summary>
        protected IConfiguration Configuration { get; set; }

        /// <summary>
        /// Provides access to the different APIs.
        /// </summary>
        protected ApiCalls ApiCalls { get; set; }

        public InteractiveAutocompleteHandler(IConfiguration configuration, ApiCalls apiCalls)
        {
            Configuration = configuration;
            ApiCalls = apiCalls;
        }
    }
}
