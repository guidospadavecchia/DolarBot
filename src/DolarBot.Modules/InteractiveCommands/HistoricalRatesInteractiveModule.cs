using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Modules.InteractiveCommands.Choices;
using DolarBot.Services.HistoricalRates;
using Fergun.Interactive;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using static DolarBot.API.ApiCalls.DolarBotApi;

namespace DolarBot.Modules.InteractiveCommands
{
    /// <summary>
    /// Contains the euro related commands.
    /// </summary>
    [HelpOrder(11)]
    [HelpTitle("Evolución Anual")]
    public class HistoricalRatesInteractiveModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about Venezuela's currency.
        /// </summary>
        private readonly HistoricalRatesService HistoricalRatesService;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        /// <param name="interactiveService">The interactive service.</param>
        public HistoricalRatesInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api, InteractiveService interactiveService) : base(configuration, logger, interactiveService)
        {
            HistoricalRatesService = new HistoricalRatesService(configuration, api);
        }
        #endregion

        [SlashCommand("evolucion", "Muestra las distintas cotizaciones del dólar para Venezuela.", false, RunMode.Async)]
        public async Task GetEvolutionAsync(
            [Summary("cotización", "Indica el tipo de cotización a obtener")]
            HistoricalRatesChoices historicalRatesChoice
        )
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    HistoricalRatesParams historicalRatesParam = Enum.Parse<HistoricalRatesParams>(historicalRatesChoice.ToString());
                    HistoricalRatesResponse result = await HistoricalRatesService.GetHistoricalRates(historicalRatesParam);
                    if (result != null && result.Meses != null && result.Meses.Count > 0)
                    {
                        EmbedBuilder embed = HistoricalRatesService.CreateHistoricalRatesEmbed(result, historicalRatesParam);
                        await SendDeferredEmbedAsync(embed.Build());
                    }
                    else
                    {
                        await SendDeferredApiErrorResponseAsync();
                    }
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponseAsync(ex);
                }
            });
        }
    }
}