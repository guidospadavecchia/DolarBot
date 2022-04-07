using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Currencies;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.InteractiveCommands.Autocompletion.FiatCurrency
{
    public class FiatCurrencyCodeAutocompleteHandler : AutocompleteHandler
    {
        /// <summary>
        /// Provides access to application settings.
        /// </summary>
        private IConfiguration Configuration { get; set; }

        /// <summary>
        /// Provides access to the different APIs.
        /// </summary>
        private ApiCalls ApiCalls { get; set; }

        private FiatCurrencyService FiatCurrencyService { get; set; }

        public FiatCurrencyCodeAutocompleteHandler(IConfiguration configuration, ApiCalls apiCalls)
        {
            Configuration = configuration;
            ApiCalls = apiCalls;
            FiatCurrencyService = new(Configuration, ApiCalls);
        }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            string filter = autocompleteInteraction.Data.Current.Value.ToString();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                List<WorldCurrencyCodeResponse> currencyCodes = await FiatCurrencyService.GetWorldCurrenciesList();
                List<WorldCurrencyCodeResponse> filteredCurrencyCodes = currencyCodes.Where(x => x.Code.StartsWith(filter, StringComparison.OrdinalIgnoreCase) || x.Name.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).ToList();
                IEnumerable<AutocompleteResult> autocompletionCollection = filteredCurrencyCodes.Select(x => new AutocompleteResult($"{x.Code} ({x.Name})", x.Code));
                return AutocompletionResult.FromSuccess(autocompletionCollection);
            }
            else
            {
                return AutocompletionResult.FromSuccess();
            }
        }
    }
}
