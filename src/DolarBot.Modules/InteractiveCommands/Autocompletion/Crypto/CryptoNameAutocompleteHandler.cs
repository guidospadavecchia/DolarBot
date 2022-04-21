﻿using Discord;
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

namespace DolarBot.Modules.InteractiveCommands.Autocompletion.Crypto
{
    public class CryptoNameAutocompleteHandler : InteractiveAutocompleteHandler
    {
        /// <summary>
        /// The crypto service.
        /// </summary>
        private CryptoService CryptoService { get; set; }

        /// <summary>
        /// Creates a new <see cref="CryptoNameAutocompleteHandler"/>.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="apiCalls">Provides access to the different APIs.</param>
        public CryptoNameAutocompleteHandler(IConfiguration configuration, ApiCalls apiCalls) : base(configuration, apiCalls)
        {
            Configuration = configuration;
            ApiCalls = apiCalls;
            CryptoService = new(Configuration, ApiCalls);
        }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            string filter = autocompleteInteraction.Data.Current.Value.ToString();
            List<CryptoCodeResponse> currencyCodes = await CryptoService.GetCryptoCodeList();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                currencyCodes = currencyCodes.Where(x => x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)).Take(MAX_AUTOCOMPLETE_RESULTS).ToList();
            }
            else
            {
                currencyCodes = currencyCodes.Take(MAX_AUTOCOMPLETE_RESULTS).ToList();
            }

            IEnumerable<AutocompleteResult> autocompletionCollection = currencyCodes.Select(x => new AutocompleteResult($"{x.Name} ({x.Symbol.ToUpper()})", x.Code)).OrderBy(x => x.Name);
            return AutocompletionResult.FromSuccess(autocompletionCollection);
        }
    }
}
