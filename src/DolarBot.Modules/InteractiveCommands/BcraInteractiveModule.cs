using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Services.Bcra;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DolarBot.Modules.InteractiveCommands
{
    /// <summary>
    /// Contains the BCRA (Argentine Republic Central Bank) related commands.
    /// </summary>
    [HelpOrder(9)]
    [HelpTitle("Indicadores BCRA")]
    public class BcraInteractiveModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about BCRA rates and values.
        /// </summary>
        private readonly BcraService BcraService;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public BcraInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger)
        {
            BcraService = new BcraService(configuration, api);
        }
        #endregion

        [SlashCommand("riesgo-pais", "Muestra el valor del riesgo país de la República Argentina.", false, RunMode.Async)]
        public async Task GetRiesgoPaisValueAsync()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    CountryRiskResponse result = await BcraService.GetCountryRisk();
                    if (result != null)
                    {
                        EmbedBuilder embed = await BcraService.CreateCountryRiskEmbedAsync(result);
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

        [SlashCommand("reservas", "Muestra las reservas aproximadas de dólares a la fecha del Banco Central de la República Argentina.", false, RunMode.Async)]
        public async Task GetReservasAsync()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    BcraResponse result = await BcraService.GetReserves();
                    if (result != null)
                    {
                        EmbedBuilder embed = await BcraService.CreateReservesEmbedAsync(result);
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

        [SlashCommand("circulante", "Muestra la cantidad total aproximada de pesos argentinos en circulación a la fecha.", false, RunMode.Async)]
        public async Task GetCirculanteAsync()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    BcraResponse result = await BcraService.GetCirculatingMoney();
                    if (result != null)
                    {
                        EmbedBuilder embed = await BcraService.CreateCirculatingMoneyEmbedAsync(result);
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
