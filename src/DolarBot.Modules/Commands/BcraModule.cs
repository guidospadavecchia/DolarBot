using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Bcra;
using DolarBot.Util;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains the BCRA (Argentine Republic Central Bank) related commands.
    /// </summary>
    [HelpOrder(4)]
    [HelpTitle("Indicadores BCRA")]
    public class BcraModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about BCRA rates and values.
        /// </summary>
        private readonly BcraService BcraService;

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
        public BcraModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration)
        {
            Logger = logger;
            BcraService = new BcraService(configuration, api);
        }
        #endregion

        [Command("riesgopais", RunMode = RunMode.Async)]
        [Alias("rp")]
        [Summary("Muestra el valor del riesgo país.")]
        [HelpUsageExample(false, "$riesgopais", "$rp")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetRiesgoPaisValueAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    RiesgoPaisResponse result = await BcraService.GetCountryRisk().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = BcraService.CreateCountryRiskEmbed(result);
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

        [Command("reservas", RunMode = RunMode.Async)]
        [Alias("rs")]
        [Summary("Muestra las reservas de dólares del Banco Central a la fecha.")]
        [HelpUsageExample(false, "$reservas", "$rs")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetReservasAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    BcraResponse result = await BcraService.GetReserves().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = BcraService.CreateReservesEmbed(result);
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

        [Command("circulante", RunMode = RunMode.Async)]
        [Alias("c")]
        [Summary("Muestra la cantidad total de pesos en circulación a la fecha.")]
        [HelpUsageExample(false, "$circulante", "$c")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetCirculanteAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    BcraResponse result = await BcraService.GetCirculatingMoney().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = BcraService.CreateCirculatingMoneyEmbed(result);
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