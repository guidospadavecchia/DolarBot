using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Banking;
using DolarBot.Services.Currencies;
using DolarBot.Services.Real;
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
    /// Contains the real (Brazil) related commands.
    /// </summary>
    [HelpOrder(3)]
    [HelpTitle("Cotizaciones del Real")]
    public class RealModule : BaseInteractiveModule
    {
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
        [Summary("Muestra todas las cotizaciones del Real oficial disponibles.")]
        [HelpUsageExample(false, "$real", "$r", "$real Nacion", "$r BBVA")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetRealPriceAsync(
            [Summary("Opcional. Indica el banco a mostrar. Los valores posibles son aquellos devueltos por el comando `$bancos real`. Si no se especifica, mostrará todas las cotizaciones.")]
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
                            if (RealService.GetValidBanks().Contains(bank))
                            {
                                //Show individual bank rate
                                string thumbnailUrl = RealService.GetBankThumbnailUrl(bank);
                                RealResponse result = await RealService.GetRealByBank(bank).ConfigureAwait(false);
                                if (result != null)
                                {
                                    EmbedBuilder embed = await RealService.CreateRealEmbedAsync(result, $"Cotización del {Format.Bold("Real oficial")} del {Format.Bold(bank.GetDescription())} expresada en {Format.Bold("pesos argentinos")}.", bank.GetDescription(), thumbnailUrl);
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
                                    await ReplyAsync($"Esta opción no está disponible para esta moneda. Si desea ver todas las cotizaciones, ejecute {Format.Code($"{commandPrefix}{Currencies.Real.GetDescription().ToLower()}")}.").ConfigureAwait(false);
                                }
                                else
                                {
                                    string bankCommand = typeof(MiscModule).GetMethod("GetBanks").GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
                                    await ReplyAsync($"La cotización del {Format.Bold(bank.GetDescription())} no está disponible para esta moneda. Verifique los bancos disponibles con {Format.Code($"{commandPrefix}{bankCommand} {Currencies.Real.GetDescription().ToLower()}")}.").ConfigureAwait(false);
                                }
                            }
                        }
                        else
                        {
                            //Unknown parameter
                            string commandPrefix = Configuration["commandPrefix"];
                            string bankCommand = typeof(MiscModule).GetMethod("GetBanks").GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
                            await ReplyAsync($"Banco '{Format.Bold(userInput)}' inexistente. Verifique los bancos disponibles con {Format.Code($"{commandPrefix}{bankCommand} {Currencies.Real.GetDescription().ToLower()}")}.").ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        //Show all bank rates
                        RealResponse[] responses = await RealService.GetAllRealRates().ConfigureAwait(false);
                        if (responses.Any(r => r != null))
                        {
                            RealResponse[] successfulResponses = responses.Where(r => r != null).ToArray();

                            string realImageUrl = Configuration.GetSection("images").GetSection("real")["64"];
                            EmbedBuilder embed = RealService.CreateRealEmbed(successfulResponses, $"Cotizaciones disponibles del {Format.Bold("Real oficial")} expresadas en {Format.Bold("pesos argentinos")}.", realImageUrl);
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