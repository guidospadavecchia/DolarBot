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
        /// Provides methods to retrieve information about euro rates.
        /// </summary>
        private readonly EuroService EuroService;

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
            EuroService = new EuroService(configuration, api);
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
                    EuroResponse[] responses = await EuroService.GetAllEuroPrices().ConfigureAwait(false);
                    if (responses.Any(r => r != null))
                    {
                        EuroResponse[] successfulResponses = responses.Where(r => r != null).ToArray();

                        string euroImageUrl = Configuration.GetSection("images").GetSection("euro")["64"];
                        EmbedBuilder embed = EuroService.CreateEuroEmbed(successfulResponses, $"Cotizaciones disponibles del {Format.Bold("Euro")} expresadas en {Format.Bold("pesos argentinos")}.", euroImageUrl);
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
                    EuroResponse[] responses = await EuroService.GetAllEuroAhorroPrices().ConfigureAwait(false);

                    if (responses.Any(r => r != null))
                    {
                        EuroResponse[] successfulResponses = responses.Where(r => r != null).ToArray();

                        string euroImageUrl = Configuration.GetSection("images").GetSection("euro")["64"];
                        EmbedBuilder embed = EuroService.CreateEuroEmbed(successfulResponses, $"Cotizaciones disponibles del {Format.Bold("Euro")} incluyendo impuesto P.A.I.S. y retención de ganancias, expresadas en {Format.Bold("pesos argentinos")}.", euroImageUrl);
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