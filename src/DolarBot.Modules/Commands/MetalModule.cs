using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Metals;
using DolarBot.Util;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains precious metal related commands.
    /// </summary>
    [HelpOrder(4)]
    [HelpTitle("Metales")]
    public class MetalModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about precious metals rates and values.
        /// </summary>
        private readonly MetalService MetalService;

        /// <summary>
        /// The log4net logger.
        /// </summary>
        private readonly ILog Logger;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public MetalModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration)
        {
            Logger = logger;
            MetalService = new MetalService(configuration, api);
        }
        #endregion

        [Command("oro", RunMode = RunMode.Async)]
        [Summary("Muestra la cotización internacional del oro.")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetGoldPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    MetalResponse result = await MetalService.GetGoldPrice().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = MetalService.CreateMetalEmbed(result);
                        await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        [Command("plata", RunMode = RunMode.Async)]
        [Summary("Muestra la cotización internacional de la plata.")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetSilverPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    MetalResponse result = await MetalService.GetSilverPrice().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = MetalService.CreateMetalEmbed(result);
                        await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        [Command("cobre", RunMode = RunMode.Async)]
        [Summary("Muestra la cotización internacional del cobre.")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetCopperPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    MetalResponse result = await MetalService.GetCopperPrice().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = MetalService.CreateMetalEmbed(result);
                        await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }
    }
}