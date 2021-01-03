using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Banking;
using DolarBot.Services.Currencies;
using DolarBot.Services.Dolar;
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
    /// Contains the dollar related commands.
    /// </summary>
    [HelpOrder(1)]
    [HelpTitle("Cotizaciones del Dólar")]
    public class DolarModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about dollar rates.
        /// </summary>
        private readonly DolarService DolarService;

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
        public DolarModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration)
        {
            Logger = logger;
            DolarService = new DolarService(configuration, api);
        }
        #endregion

        [Command("dolar", RunMode = RunMode.Async)]
        [Alias("d")]
        [Summary("Muestra todas las cotizaciones del dólar disponibles o por banco.")]
        [HelpUsageExample(false, "$dolar", "$d", "$dolar bancos", "$dolar santander", "$d galicia")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarPriceAsync(
            [Summary("Opcional. Indica el banco a mostrar. Los valores posibles son aquellos devueltos por el comando `$bancos dolar`. Si no se especifica, mostrará todas las cotizaciones no bancarias.")]
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
                            if (bank == Banks.Bancos)
                            {
                                DollarResponse[] responses = await DolarService.GetAllBankRates().ConfigureAwait(false);
                                if (responses.Any(r => r != null))
                                {
                                    string thumbnailUrl = Configuration.GetSection("images").GetSection("bank")["64"];
                                    DollarResponse[] successfulResponses = responses.Where(r => r != null).ToArray();
                                    EmbedBuilder embed = DolarService.CreateDollarEmbed(successfulResponses, $"Cotizaciones de {Format.Bold("bancos")} del {Format.Bold("dólar oficial")} expresadas en {Format.Bold("pesos argentinos")}.", thumbnailUrl);
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
                            else
                            {   //Show individual bank rate
                                string thumbnailUrl = DolarService.GetBankThumbnailUrl(bank);
                                DollarResponse result = await DolarService.GetByBank(bank).ConfigureAwait(false);
                                if (result != null)
                                {
                                    EmbedBuilder embed = DolarService.CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar oficial")} del {Format.Bold(bank.GetDescription())} expresada en {Format.Bold("pesos argentinos")}.", null, thumbnailUrl);
                                    await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                                }
                                else
                                {
                                    await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                                }
                            }
                        }
                        else
                        {   //Unknown parameter
                            string commandPrefix = Configuration["commandPrefix"];
                            string bankCommand = typeof(MiscModule).GetMethod("GetBanks").GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
                            await ReplyAsync($"Banco '{Format.Bold(userInput)}' inexistente. Verifique los bancos disponibles con {Format.Code($"{commandPrefix}{bankCommand} {Currencies.Dolar.GetDescription().ToLower()}")}.").ConfigureAwait(false);
                        }
                    }
                    else
                    {   //Show all dollar types (not banks)
                        DollarResponse[] responses = await DolarService.GetAllDollarRates().ConfigureAwait(false);
                        if (responses.Any(r => r != null))
                        {
                            DollarResponse[] successfulResponses = responses.Where(r => r != null).ToArray();
                            EmbedBuilder embed = DolarService.CreateDollarEmbed(successfulResponses);
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

        [Command("dolaroficial", RunMode = RunMode.Async)]
        [Alias("do")]
        [Summary("Muestra la cotización del dólar oficial.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarOficialPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    DollarResponse result = await DolarService.GetDollarOficial().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = DolarService.CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar oficial")} expresada en {Format.Bold("pesos argentinos")}.");
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

        [Command("dolarahorro", RunMode = RunMode.Async)]
        [Alias("da")]
        [Summary("Muestra la cotización del dólar oficial más impuesto P.A.I.S. y ganancias.")]
        [RateLimit(1, 5, Measure.Seconds)]
        [HelpUsageExample(false, "$dolarahorro", "$da", "$dolar bancos", "$dolar santander", "$d galicia")]
        public async Task GetDolarAhorroPriceAsync(
            [Summary("Opcional. Indica el banco a mostrar. Los valores posibles son aquellos devueltos por el comando `$bancos dolar`. Si no se especifica, mostrará la cotización estándar.")]
            string banco = null)
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    if (banco != null)
                    {
                        //Show bank rate
                        string userInput = Format.Sanitize(banco).RemoveFormat(true);
                        if (Enum.TryParse(userInput, true, out Banks bank))
                        {
                            if (bank == Banks.Bancos)
                            {
                                DollarResponse[] responses = await DolarService.GetAllAhorroBankRates().ConfigureAwait(false);
                                if (responses.Any(r => r != null))
                                {
                                    string thumbnailUrl = Configuration.GetSection("images").GetSection("bank")["64"];
                                    DollarResponse[] successfulResponses = responses.Where(r => r != null).ToArray();
                                    EmbedBuilder embed = DolarService.CreateDollarEmbed(successfulResponses, $"Cotizaciones de {Format.Bold("bancos")} del {Format.Bold("dólar ahorro")} expresadas en {Format.Bold("pesos argentinos")}.", thumbnailUrl);
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
                            else
                            {
                                //Show individual bank rate
                                string thumbnailUrl = DolarService.GetBankThumbnailUrl(bank);
                                DollarResponse result = await DolarService.GetDollarAhorroByBank(bank).ConfigureAwait(false);
                                if (result != null)
                                {
                                    EmbedBuilder embed = DolarService.CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar ahorro")} del {Format.Bold(bank.GetDescription())} expresada en {Format.Bold("pesos argentinos")}.", bank.GetDescription(), thumbnailUrl);
                                    await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                                }
                                else
                                {
                                    await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                                }
                            }
                        }
                        else
                        {
                            //Unknown parameter
                            string commandPrefix = Configuration["commandPrefix"];
                            string bankCommand = typeof(MiscModule).GetMethod("GetBanks").GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
                            await ReplyAsync($"Banco '{Format.Bold(userInput)}' inexistente. Verifique los bancos disponibles con {Format.Code($"{commandPrefix}{bankCommand} {Currencies.Dolar.GetDescription().ToLower()}")}.").ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        DollarResponse result = await DolarService.GetDollarAhorro().ConfigureAwait(false);
                        if (result != null)
                        {
                            EmbedBuilder embed = DolarService.CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar ahorro")} expresada en {Format.Bold("pesos argentinos")}.");
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

        [Command("dolarblue", RunMode = RunMode.Async)]
        [Alias("db")]
        [Summary("Muestra la cotización del dólar blue.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarBluePriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    DollarResponse result = await DolarService.GetDollarBlue().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = DolarService.CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar blue")} expresada en {Format.Bold("pesos argentinos")}.");
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

        [Command("dolarpromedio", RunMode = RunMode.Async)]
        [Alias("dp")]
        [Summary("Muestra el promedio de las cotizaciones bancarias del dólar oficial.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarPromedioPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    DollarResponse result = await DolarService.GetDollarPromedio().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = DolarService.CreateDollarEmbed(result, $"Cotización {Format.Bold("promedio de los bancos del dólar oficial")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.");
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

        [Command("dolarbolsa", RunMode = RunMode.Async)]
        [Alias("dbo")]
        [Summary("Muestra la cotización del dólar bolsa (MEP).")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarBolsaPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    DollarResponse result = await DolarService.GetDollarBolsa().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = DolarService.CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar bolsa (MEP)")} expresada en {Format.Bold("pesos argentinos")}.");
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

        [Command("contadoconliqui", RunMode = RunMode.Async)]
        [Alias("ccl")]
        [Summary("Muestra la cotización del dólar contado con liquidación.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarContadoConLiquiPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    DollarResponse result = await DolarService.GetDollarContadoConLiqui().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = DolarService.CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar contado con liquidación")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.");
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