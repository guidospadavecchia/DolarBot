using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Euro;
using DolarBot.Util;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using EuroTypes = DolarBot.API.ApiCalls.DolarArgentinaApi.EuroTypes;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains the euro related commands.
    /// </summary>
    [HelpOrder(2)]
    [HelpTitle("Cotizaciones del Euro")]
    public class EuroModule : BaseInteractiveModule
    {
        #region Constants
        private const string REQUEST_ERROR_MESSAGE = "Error: No se pudo obtener la cotización. Intente nuevamente en más tarde.";
        #endregion

        #region Vars
        /// <summary>
        /// Provides access to the different APIs.
        /// </summary>
        protected readonly ApiCalls Api;

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
        public EuroModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration)
        {
            Logger = logger;
            Api = api;
        }
        #endregion

        [Command("euro", RunMode = RunMode.Async)]
        [Alias("e")]
        [Summary("Muestra todas las cotizaciones del Euro disponibles.")]
        [HelpUsageExample(false, "$euro", "$e")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetEuroPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    EuroService euroService = new EuroService(Configuration, Api);
                    EuroResponse[] responses = await Task.WhenAll(Api.DolarArgentina.GetEuroPrice(EuroTypes.Nacion),
                                                                  Api.DolarArgentina.GetEuroPrice(EuroTypes.Galicia),
                                                                  Api.DolarArgentina.GetEuroPrice(EuroTypes.BBVA),
                                                                  Api.DolarArgentina.GetEuroPrice(EuroTypes.Hipotecario),
                                                                  Api.DolarArgentina.GetEuroPrice(EuroTypes.Chaco),
                                                                  Api.DolarArgentina.GetEuroPrice(EuroTypes.Pampa)).ConfigureAwait(false);
                    if (responses.Any(r => r != null))
                    {
                        EuroResponse[] successfulResponses = responses.Where(r => r != null).ToArray();
                        EmbedBuilder embed = euroService.CreateEuroEmbed(successfulResponses);
                        if (responses.Length != successfulResponses.Length)
                        {
                            await ReplyAsync($"{Format.Bold("Atención")}: No se pudieron obtener algunas cotizaciones. Sólo se mostrarán aquellas que no presentan errores.").ConfigureAwait(false);
                        }
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

        [Command("euroahorro", RunMode = RunMode.Async)]
        [Alias("ea")]
        [Summary("Muestra todas las cotizaciones del Euro disponibles incluyendo impuesto P.A.I.S. y retención de ganancias.")]
        [HelpUsageExample(false, "$euroahorro", "$ea")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetEuroAhorroPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    EuroService euroService = new EuroService(Configuration, Api);
                    EuroResponse[] responses = await Task.WhenAll(Api.DolarArgentina.GetEuroPrice(EuroTypes.Nacion),
                                                                  Api.DolarArgentina.GetEuroPrice(EuroTypes.Galicia),
                                                                  Api.DolarArgentina.GetEuroPrice(EuroTypes.BBVA),
                                                                  Api.DolarArgentina.GetEuroPrice(EuroTypes.Hipotecario),
                                                                  Api.DolarArgentina.GetEuroPrice(EuroTypes.Chaco),
                                                                  Api.DolarArgentina.GetEuroPrice(EuroTypes.Pampa)).ConfigureAwait(false);
                    if (responses.Any(r => r != null))
                    {
                        EuroResponse[] successfulResponses = responses.Where(r => r != null).ToArray();
                        EmbedBuilder embed = euroService.CreateEuroAhorroEmbed(successfulResponses);
                        if (responses.Length != successfulResponses.Length)
                        {
                            await ReplyAsync($"{Format.Bold("Atención")}: No se pudieron obtener algunas cotizaciones. Sólo se mostrarán aquellas que no presentan errores.").ConfigureAwait(false);
                        }
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