using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.Modules.InteractiveCommands.Autocompletion.Base;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.InteractiveCommands.Autocompletion.Help
{
    public class SlashCommandAutocompleteHandler : InteractiveAutocompleteHandler
    {
        private const string HELP_COMMAND = "ayuda";

        /// <summary>
        /// Creates a new <see cref="SlashCommandAutocompleteHandler"/>.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="apiCalls">Provides access to the different APIs.</param>
        public SlashCommandAutocompleteHandler(IConfiguration configuration, ApiCalls apiCalls) : base(configuration, apiCalls)
        {
            Configuration = configuration;
            ApiCalls = apiCalls;
        }

        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            string filter = autocompleteInteraction.Data.Current.Value.ToString();
            List<SlashCommandInfo> slashCommands = InteractionService.SlashCommands.Where(x => !x.Name.Equals(HELP_COMMAND, StringComparison.OrdinalIgnoreCase)).OrderBy(x => x.Name).ToList();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                List<SlashCommandInfo> filteredSlashCommands = slashCommands.Where(x => x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)).Take(MAX_AUTOCOMPLETE_RESULTS).ToList();
                IEnumerable<AutocompleteResult> autocompletionCollection = filteredSlashCommands.Select(x => new AutocompleteResult(x.Name, x.Name));
                return Task.FromResult(AutocompletionResult.FromSuccess(autocompletionCollection));
            }
            else
            {
                List<SlashCommandInfo> topSlashCommands = slashCommands.Take(MAX_AUTOCOMPLETE_RESULTS).OrderBy(x => x.Name).ToList();
                IEnumerable<AutocompleteResult> autocompletionCollection = topSlashCommands.Select(x => new AutocompleteResult(x.Name, x.Name));
                return Task.FromResult(AutocompletionResult.FromSuccess(autocompletionCollection));
            }
        }
    }
}
