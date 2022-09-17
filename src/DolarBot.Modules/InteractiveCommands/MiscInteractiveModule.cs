using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Services.Banking;
using DolarBot.Services.Banking.Interfaces;
using DolarBot.Services.Currencies;
using DolarBot.Services.Dolar;
using DolarBot.Services.Euro;
using DolarBot.Services.Quotes;
using DolarBot.Services.Real;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Fergun.Interactive;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.InteractiveCommands
{
    /// <summary>
    /// Contains misc commands.
    /// </summary>
    [HelpOrder(98)]
    [HelpTitle("Otros")]
    public class MiscInteractiveModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides several methods to interact with the different APIs.
        /// </summary>
        private readonly ApiCalls Api;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ILog"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="logger">Provides access to the different APIs.</param>
        /// <param name="interactiveService">The interactive service.</param>
        public MiscInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api, InteractiveService interactiveService) : base(configuration, logger, interactiveService)
        {
            Api = api;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Creates a new instance of the corresponding currency service according to <paramref name="currency"/>.
        /// </summary>
        /// <param name="currency">The currency type.</param>
        /// <returns>An instantiated service.</returns>
        private IBankCurrencyService GetCurrencyService(Currencies currency)
        {
            return currency switch
            {
                Currencies.Dolar => new DolarService(Configuration, Api),
                Currencies.Euro => new EuroService(Configuration, Api),
                Currencies.Real => new RealService(Configuration, Api),
                _ => throw new ArgumentException("Invalid currency.")
            };
        }

        #endregion

        [SlashCommand("bancos", "Muestra la lista de bancos disponibles para cada una de las monedas principales.", false, RunMode.Async)]
        public async Task GetBanks()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    Emoji bankEmoji = new(":bank:");
                    string bankImageUrl = Configuration.GetSection("images").GetSection("bank")["64"];

                    List<EmbedBuilder> embeds = new();
                    Currencies[] currencies = Enum.GetValues(typeof(Currencies)).Cast<Currencies>().ToArray();
                    foreach (Currencies currency in currencies)
                    {
                        IBankCurrencyService currencyService = GetCurrencyService(currency);
                        Banks[] banks = currencyService.GetValidBanks();
                        string bankList = string.Join(Environment.NewLine, banks.Select(x => $"{bankEmoji} {Format.Code(x.ToString().ToLower())}: {Format.Italics(x.GetDescription())}.")).AppendLineBreak();

                        EmbedBuilder embed = new EmbedBuilder().WithTitle($"{Format.Bold(currency.GetDescription())} ({Format.Code($"/{currency.ToString().ToLower()}")})")
                                                               .WithDescription($"Bancos disponibles para utilizar como parámetro en el comando {Format.Code($"/{currency.ToString().ToLower()}")}.")
                                                               .WithColor(GlobalConfiguration.Colors.Currency)
                                                               .WithThumbnailUrl(bankImageUrl)
                                                               .AddField(GlobalConfiguration.Constants.BLANK_SPACE, bankList);
                        embeds.Add(embed);
                    }

                    await FollowUpWithPaginatedEmbedAsync(embeds.Build());
                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
                }
            });
        }

        [SlashCommand("frase", "Muestra una frase célebre de la economía argentina.", false, RunMode.Async)]
        public async Task GetRandomQuote()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    Quote quote = QuoteService.GetRandomQuote();
                    string responseMessage;
                    if (quote != null && !string.IsNullOrWhiteSpace(quote.Text))
                    {
                        responseMessage = $"{Format.Italics($"\"{quote.Text}\"")} -{Format.Bold(quote.Author)}.";
                    }
                    else
                    {
                        responseMessage = $"{Format.Bold("Error")}. No se puede acceder a la información solicitada en este momento. Por favor, intentá nuevamente más tarde.";
                    }

                    await FollowupAsync(responseMessage);
                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
                }
            });
        }
    }
}