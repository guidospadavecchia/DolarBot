using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.InteractiveCommands.Autocompletion.Base;
using DolarBot.Services.Crypto;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace DolarBot.Modules.InteractiveCommands.Autocompletion.Crypto
{
    public class CryptoAutocompleteHandler : InteractiveAutocompleteHandler
    {
        /// <summary>
        /// The crypto service.
        /// </summary>
        private CryptoService CryptoService { get; set; }

        /// <summary>
        /// Creates a new <see cref="CryptoAutocompleteHandler"/>.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="apiCalls">Provides access to the different APIs.</param>
        public CryptoAutocompleteHandler(IConfiguration configuration, ApiCalls apiCalls) : base(configuration, apiCalls)
        {
            Configuration = configuration;
            ApiCalls = apiCalls;
            CryptoService = new(Configuration, ApiCalls);
        }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            try
            {
                string? filter = autocompleteInteraction.Data.Current.Value.ToString();
                List<CryptoCodeResponse>? currencyCodes = await CryptoService.GetCryptoCodeList();
                if (currencyCodes?.Any() ?? false)
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        List<CryptoCodeResponse> currencyCodesBySymbol = currencyCodes.Where(x => x.Symbol?.Equals(filter, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
                        if (!currencyCodesBySymbol.Any())
                        {
                            currencyCodesBySymbol = currencyCodes.Where(x => x.Symbol?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
                        }
                        List<CryptoCodeResponse> currencyCodesByName = currencyCodes.Where(x => x.Name?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
                        currencyCodes = currencyCodesBySymbol.Union(currencyCodesByName).Take(MAX_AUTOCOMPLETE_RESULTS).ToList();
                    }
                    else
                    {
                        currencyCodes = currencyCodes.Take(MAX_AUTOCOMPLETE_RESULTS).ToList();
                    }

                    IEnumerable<AutocompleteResult> autocompletionCollection = currencyCodes.Select(x => new AutocompleteResult($"[{x.Symbol?.ToUpper()}] {x.Name}", x.Code)).OrderBy(x => x.Name);
                    return AutocompletionResult.FromSuccess(autocompletionCollection);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generation suggestions: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine(ex.InnerException.StackTrace);
                }
            }

            return AutocompletionResult.FromSuccess();
        }
    }
}
