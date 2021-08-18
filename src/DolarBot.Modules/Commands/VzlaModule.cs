using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Venezuela;
using DolarBot.Util;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains cryptocurrency related commands.
    /// </summary>
    [HelpOrder(9)]
    [HelpTitle("Cotizaciones de Venezuela")]
    public class VzlaModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about bolivar rates.
        /// </summary>
        private readonly VzlaService VzlaService;

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
        public VzlaModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration)
        {
            Logger = logger;
            VzlaService = new VzlaService(configuration, api);
        }
        #endregion

        [Command("bolivardolar", RunMode = RunMode.Async)]
        [Alias("bd")]
        [Summary("Muestra las distintas cotizaciones del dólar para Venezuela.")]
        [HelpUsageExample(false, "$bolivardolar", "$bd")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetDollarRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    VzlaResponse result = await VzlaService.GetDollarRates();
                    if (result != null)
                    {
                        EmbedBuilder embed = await VzlaService.CreateVzlaEmbedAsync(result);
                        await ReplyAsync(embed: embed.Build());
                    }
                    else
                    {
                        await ReplyAsync(REQUEST_ERROR_MESSAGE);
                    }
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        [Command("bolivareuro", RunMode = RunMode.Async)]
        [Alias("be")]
        [Summary("Muestra las distintas cotizaciones del Euro para Venezuela.")]
        [HelpUsageExample(false, "$bolivareuro", "$be")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetEuroRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    VzlaResponse result = await VzlaService.GetEuroRates();
                    if (result != null)
                    {
                        EmbedBuilder embed = await VzlaService.CreateVzlaEmbedAsync(result);
                        await ReplyAsync(embed: embed.Build());
                    }
                    else
                    {
                        await ReplyAsync(REQUEST_ERROR_MESSAGE);
                    }
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }
    }
}