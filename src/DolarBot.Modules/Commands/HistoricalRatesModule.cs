using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.HistoricalRates;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using HistoricalRatesParams = DolarBot.API.ApiCalls.DolarArgentinaApi.HistoricalRatesParams;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains the historical rates related commands.
    /// </summary>
    [HelpOrder(5)]
    [HelpTitle("Evolución Anual")]
    public class HistoricalRatesModule : BaseInteractiveModule
    {
        #region Constants
        private const string REQUEST_ERROR_MESSAGE = "Error: No se pudo obtener la cotización. Intente nuevamente en más tarde.";
        #endregion

        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about historical rates.
        /// </summary>
        private readonly HistoricalRatesService HistoricalRatesService;

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
        public HistoricalRatesModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration)
        {
            Logger = logger;
            HistoricalRatesService = new HistoricalRatesService(configuration, api);
        }
        #endregion

        [Command("evolucion", RunMode = RunMode.Async)]
        [Alias("ev")]
        [Summary("Muestra la evolución mensual anualizada de las distintas cotizaciones disponibles.")]
        [HelpUsageExample(false, "$evolucion DolarOficial", "$evolucion DolarBlue", "$ev Euro", "$ev Real")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetEvolucionAsync([Summary("Indica el tipo de cotización a obtener")] string cotizacion = null)
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    if(cotizacion == null)
                    {
                        await ReplyAsync(GetEvolucionInvalidParamsMessage()).ConfigureAwait(false);
                    }
                    else
                    {
                        string userInput = Format.Sanitize(cotizacion).RemoveFormat(true);
                        if (Enum.TryParse(userInput, true, out HistoricalRatesParams historicalRateParam))
                        {
                            HistoricalRatesResponse result = await HistoricalRatesService.GetHistoricalRates(historicalRateParam).ConfigureAwait(false);
                            if (result != null && result.Meses != null && result.Meses.Count > 0)
                            {
                                EmbedBuilder embed = HistoricalRatesService.CreateHistoricalRatesEmbed(result, historicalRateParam);
                                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                            }
                            else
                            {
                                await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await ReplyAsync(GetEvolucionInvalidParamsMessage()).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        /// <summary>
        /// Retrieves the error message to show when the command is called with invalid parameters.
        /// </summary>
        /// <returns>An error message.</returns>
        private string GetEvolucionInvalidParamsMessage()
        {
            string commandPrefix = Configuration["commandPrefix"];
            string parameters = string.Join(", ", Enum.GetNames(typeof(HistoricalRatesParams)).Select(x => Format.Code(x.ToLower())));
            
            return $"Debe especificar un parámetro válido para este comando. Las opciones disponibles son: {parameters}. Para más información, ejecute {Format.Code($"{commandPrefix}ayuda evolucion")}.";
        }
    }
}