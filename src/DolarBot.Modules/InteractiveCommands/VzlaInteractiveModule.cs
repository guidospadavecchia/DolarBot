using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Services.Venezuela;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DolarBot.Modules.InteractiveCommands
{
    /// <summary>
    /// Contains the euro related commands.
    /// </summary>
    [HelpOrder(5)]
    [HelpTitle("Cotizaciones del Venezuela")]
    public class VzlaInteractiveModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about Venezuela's currency.
        /// </summary>
        private readonly VzlaService VzlaService;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public VzlaInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger)
        {
            VzlaService = new VzlaService(configuration, api);
        }
        #endregion

        [SlashCommand("bolivar-dolar", "Muestra las distintas cotizaciones del dólar para Venezuela.", false, RunMode.Async)]
        public async Task GetDollarRatesAsync()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    VzlaResponse result = await VzlaService.GetDollarRates();
                    if (result != null)
                    {
                        EmbedBuilder embed = await VzlaService.CreateVzlaEmbedAsync(result);
                        await SendDeferredEmbed(embed.Build());
                    }
                    else
                    {
                        await SendDeferredApiErrorResponse();
                    }
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponse(ex);
                }
            });
        }

        [SlashCommand("bolivar-euro", "Muestra las distintas cotizaciones del Euro para Venezuela.", false, RunMode.Async)]
        public async Task GetEuroRatesAsync()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    VzlaResponse result = await VzlaService.GetEuroRates();
                    if (result != null)
                    {
                        EmbedBuilder embed = await VzlaService.CreateVzlaEmbedAsync(result);
                        await SendDeferredEmbed(embed.Build());
                    }
                    else
                    {
                        await SendDeferredApiErrorResponse();
                    }
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponse(ex);
                }
            });
        }
    }
}