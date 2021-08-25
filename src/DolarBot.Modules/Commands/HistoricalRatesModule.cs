using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.HistoricalRates;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using HistoricalRatesParams = DolarBot.API.ApiCalls.DolarBotApi.HistoricalRatesParams;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains the historical rates related commands.
    /// </summary>
    [HelpOrder(11)]
    [HelpTitle("Evolución Anual")]
    public class HistoricalRatesModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about historical rates.
        /// </summary>
        private readonly HistoricalRatesService HistoricalRatesService;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public HistoricalRatesModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger)
        {
            HistoricalRatesService = new HistoricalRatesService(configuration, api);
        }
        #endregion

        [Command("evolucion", RunMode = RunMode.Async)]
        [Alias("ev")]
        [Summary("Muestra la evolución mensual anualizada de las distintas cotizaciones disponibles.")]
        [HelpUsageExample(false, "$evolucion DolarOficial", "$evolucion DolarBlue", "$evolucion DolarAhorro", "$ev Euro", "$ev EuroAhorro", "$ev Real", "$ev RealAhorro")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetEvolucionAsync([Summary("Indica el tipo de cotización a obtener")] string cotizacion = null)
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    if (cotizacion == null)
                    {
                        await ReplyAsync(GetEvolucionInvalidParamsMessage());
                    }
                    else
                    {
                        string userInput = Format.Sanitize(cotizacion).RemoveFormat(true);
                        if (Enum.TryParse(userInput, true, out HistoricalRatesParams historicalRateParam))
                        {
                            HistoricalRatesResponse result = await HistoricalRatesService.GetHistoricalRates(historicalRateParam);
                            if (result != null && result.Meses != null && result.Meses.Count > 0)
                            {
                                EmbedBuilder embed = HistoricalRatesService.CreateHistoricalRatesEmbed(result, historicalRateParam);
                                await ReplyAsync(embed: embed.Build());
                            }
                            else
                            {
                                await ReplyAsync(REQUEST_ERROR_MESSAGE);
                            }
                        }
                        else
                        {
                            await ReplyAsync(GetEvolucionInvalidParamsMessage());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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