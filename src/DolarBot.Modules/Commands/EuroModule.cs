using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Banking;
using DolarBot.Services.Currencies;
using DolarBot.Services.Euro;
using DolarBot.Util;
using DolarBot.Util.Extensions;
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
        [Summary("Muestra todas las cotizaciones del Euro oficial disponibles.")]
        [HelpUsageExample(false, "$euro", "$e", "$euro Nacion", "$e BBVA")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetEuroPriceAsync(
            [Summary("Opcional. Indica el banco a mostrar. Los valores posibles son aquellos devueltos por el comando `$bancos euro`. Si no se especifica, mostrará todas las cotizaciones.")]
            string banco = null)
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    if (banco != null)
                    {
                        string userInput = Format.Sanitize(banco).RemoveFormat(true);
                        if (Enum.TryParse(userInput, true, out Banks bank))
                        {
                            if (EuroService.GetValidBanks().Contains(bank))
                            {
                                //Show individual bank rate
                                string thumbnailUrl = EuroService.GetBankThumbnailUrl(bank);
                                EuroResponse result = await EuroService.GetEuroByBank(bank).ConfigureAwait(false);
                                if (result != null)
                                {
                                    EmbedBuilder embed = EuroService.CreateEuroEmbed(result, $"Cotización del {Format.Bold("Euro oficial")} del {Format.Bold(bank.GetDescription())} expresada en {Format.Bold("pesos argentinos")}.", bank.GetDescription(), thumbnailUrl);
                                    await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                                }
                                else
                                {
                                    await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                //Invalid bank for currency
                                string commandPrefix = Configuration["commandPrefix"];
                                if (bank == Banks.Bancos)
                                {
                                    await ReplyAsync($"Esta opción no está disponible para esta moneda. Si desea ver todas las cotizaciones, ejecute {Format.Code($"{commandPrefix}{Currencies.Euro.GetDescription().ToLower()}")}.").ConfigureAwait(false);
                                }
                                else
                                {
                                    string bankCommand = typeof(MiscModule).GetMethod("GetBanks").GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
                                    await ReplyAsync($"La cotización del {Format.Bold(bank.GetDescription())} no está disponible para esta moneda. Verifique los bancos disponibles con {Format.Code($"{commandPrefix}{bankCommand} {Currencies.Euro.GetDescription().ToLower()}")}.").ConfigureAwait(false);
                                }
                            }
                        }
                        else
                        {
                            //Unknown parameter
                            string commandPrefix = Configuration["commandPrefix"];
                            string bankCommand = typeof(MiscModule).GetMethod("GetBanks").GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
                            await ReplyAsync($"Banco '{Format.Bold(userInput)}' inexistente. Verifique los bancos disponibles con {Format.Code($"{commandPrefix}{bankCommand} {Currencies.Euro.GetDescription().ToLower()}")}.").ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        //Show all bank rates
                        EuroResponse[] responses = await EuroService.GetAllEuroRates().ConfigureAwait(false);
                        if (responses.Any(r => r != null))
                        {
                            EuroResponse[] successfulResponses = responses.Where(r => r != null).ToArray();

                            string euroImageUrl = Configuration.GetSection("images").GetSection("euro")["64"];
                            EmbedBuilder embed = EuroService.CreateEuroEmbed(successfulResponses, $"Cotizaciones disponibles del {Format.Bold("Euro oficial")} expresadas en {Format.Bold("pesos argentinos")}.", euroImageUrl);
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
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }
    }
}