using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.InteractiveCommands.Autocompletion.Base;
using DolarBot.Services.Currencies;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.InteractiveCommands.Autocompletion.FiatCurrency
{
    public class FiatCurrencyCodeAutocompleteHandler : InteractiveAutocompleteHandler
    {
        /// <summary>
        /// The fiat currency service.
        /// </summary>
        private FiatCurrencyService FiatCurrencyService { get; set; }

        /// <summary>
        /// Creates a new <see cref="FiatCurrencyCodeAutocompleteHandler"/>.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="apiCalls">Provides access to the different APIs.</param>
        public FiatCurrencyCodeAutocompleteHandler(IConfiguration configuration, ApiCalls apiCalls) : base(configuration, apiCalls)
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
                List<WorldCurrencyCodeResponse> filteredCurrencyCodes = currencyCodes.Where(x => x.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) || 
                                                                                                 x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                                                                                     .Take(MAX_AUTOCOMPLETE_RESULTS)
                                                                                     .ToList();
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
