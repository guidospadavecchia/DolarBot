using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Services.Metals;
using Fergun.Interactive;
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
    [HelpTitle("Cotizaciones del metales")]
    public class MetalInteractiveModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about precious metals rates and values.
        /// </summary>
        private readonly MetalService MetalService;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        /// <param name="interactiveService">The interactive service.</param>
        public MetalInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api, InteractiveService interactiveService) : base(configuration, logger, interactiveService)
        {
            MetalService = new MetalService(configuration, api);
        }
        #endregion

        [SlashCommand("oro", "Muestra la cotización internacional del oro.", false, RunMode.Async)]
        public async Task GetGoldPriceAsync()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    MetalResponse result = await MetalService.GetGoldPrice();
                    if (result != null)
                    {
                        EmbedBuilder embed = await MetalService.CreateMetalEmbedAsync(result);
                        await FollowupAsync(embed: embed.Build());
                    }
                    else
                    {
                        await FollowUpWithApiErrorResponseAsync();
                    }
                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
                }
            });
        }

        [SlashCommand("plata", "Muestra la cotización internacional de la plata.", false, RunMode.Async)]
        public async Task GetSilverPriceAsync()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    MetalResponse result = await MetalService.GetSilverPrice();
                    if (result != null)
                    {
                        EmbedBuilder embed = await MetalService.CreateMetalEmbedAsync(result);
                        await FollowupAsync(embed: embed.Build());
                    }
                    else
                    {
                        await FollowUpWithApiErrorResponseAsync();
                    }
                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
                }
            });
        }

        [SlashCommand("cobre", "Muestra la cotización internacional del cobre.", false, RunMode.Async)]
        public async Task GetCopperPriceAsync()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    MetalResponse result = await MetalService.GetCopperPrice();
                    if (result != null)
                    {
                        EmbedBuilder embed = await MetalService.CreateMetalEmbedAsync(result);
                        await FollowupAsync(embed: embed.Build());
                    }
                    else
                    {
                        await FollowUpWithApiErrorResponseAsync();
                    }
                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
                }
            });
        }
    }
}