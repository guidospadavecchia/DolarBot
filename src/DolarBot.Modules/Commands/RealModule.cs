using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Real;
using DolarBot.Util;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains the real (Brazil) related commands.
    /// </summary>
    [HelpOrder(3)]
    [HelpTitle("Cotizaciones del Real")]
    public class RealModule : BaseInteractiveModule
    {
        #region Constants
        private const string REQUEST_ERROR_MESSAGE = "Error: No se pudo obtener la cotización. Intente nuevamente en más tarde.";
        #endregion

        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about real rates.
        /// </summary>
        private readonly RealService RealService;

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
        public RealModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration)
        {
            Logger = logger;
            RealService = new RealService(configuration, api);
        }
        #endregion

        [Command("real", RunMode = RunMode.Async)]
        [Alias("r")]
        [Summary("Muestra todas las cotizaciones del Real disponibles.")]
        [HelpUsageExample(false, "$real", "$r")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetRealPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    RealResponse[] responses = await RealService.GetAllRealPrices().ConfigureAwait(false);
                    if (responses.Any(r => r != null))
                    {
                        RealResponse[] successfulResponses = responses.Where(r => r != null).ToArray();

                        string realImageUrl = Configuration.GetSection("images").GetSection("real")["64"];
                        EmbedBuilder embed = RealService.CreateRealEmbed(successfulResponses, $"Cotizaciones disponibles del {Format.Bold("Real")} expresadas en {Format.Bold("pesos argentinos")}.", realImageUrl);
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

        [Command("realahorro", RunMode = RunMode.Async)]
        [Alias("ra")]
        [Summary("Muestra todas las cotizaciones del Real disponibles incluyendo impuesto P.A.I.S. y retención de ganancias.")]
        [HelpUsageExample(false, "$realahorro", "$ra")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetRealAhorroPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    RealResponse[] responses = await RealService.GetAllRealAhorroPrices().ConfigureAwait(false);
                    if (responses.Any(r => r != null))
                    {
                        RealResponse[] successfulResponses = responses.Where(r => r != null).ToArray();

                        string realImageUrl = Configuration.GetSection("images").GetSection("real")["64"];
                        EmbedBuilder embed = RealService.CreateRealEmbed(successfulResponses, $"Cotizaciones disponibles del {Format.Bold("Real")} incluyendo impuesto P.A.I.S. y retención de ganancias, expresadas en {Format.Bold("pesos argentinos")}.", realImageUrl);
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