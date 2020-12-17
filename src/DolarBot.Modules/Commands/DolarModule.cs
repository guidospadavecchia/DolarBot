using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Banking;
using DolarBot.Services.Dolar;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DollarTypes = DolarBot.API.ApiCalls.DolarArgentinaApi.DollarTypes;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains the dollar related commands.
    /// </summary>
    [HelpOrder(1)]
    [HelpTitle("Cotizaciones del Dólar")]
    public class DolarModule : BaseInteractiveModule
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
        public DolarModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration)
        {
            Logger = logger;
            Api = api;
        }
        #endregion

        [Command("dolar", RunMode = RunMode.Async)]
        [Alias("d")]
        [Summary("Muestra todas las cotizaciones del dólar disponibles o por banco.")]
        [HelpUsageExample(false, "$dolar", "$d", "$dolar bancos", "$dolar santander", "$d galicia")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarPriceAsync(
            [Summary("Indica la cotización del banco a mostrar. Los valores posibles son aquellos devueltos por el comando `$bancos`.")]
            string banco = null)
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    DolarService dolarService = new DolarService(Configuration, Api);

                    if (banco != null)
                    {
                        string userInput = Format.Sanitize(banco).RemoveFormat(true);
                        if (Enum.TryParse(userInput, true, out Banks bank))
                        {
                            if (bank == Banks.Bancos)
                            {
                                //Show all private banks prices
                                List<Banks> banks = Enum.GetValues(typeof(Banks)).Cast<Banks>().Where(b => b != Banks.Bancos).ToList();
                                Task<DolarResponse>[] tasks = new Task<DolarResponse>[banks.Count];
                                for (int i = 0; i < banks.Count; i++)
                                {
                                    DollarTypes dollarType = dolarService.GetBankInformation(banks[i], out string _);
                                    tasks[i] = Api.DolarArgentina.GetDollarPrice(dollarType);
                                }

                                DolarResponse[] responses = await Task.WhenAll(tasks).ConfigureAwait(false);
                                if (responses.Any(r => r != null))
                                {
                                    string thumbnailUrl = Configuration.GetSection("images").GetSection("bank")["64"];
                                    DolarResponse[] successfulResponses = responses.Where(r => r != null).ToArray();
                                    EmbedBuilder embed = dolarService.CreateDollarEmbed(successfulResponses, $"Cotizaciones de {Format.Bold("bancos")} expresados en {Format.Bold("pesos argentinos")}.", thumbnailUrl);
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
                            {   //Show individual bank price
                                DollarTypes dollarType = dolarService.GetBankInformation(bank, out string thumbnailUrl);
                                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(dollarType).ConfigureAwait(false);
                                if (result != null)
                                {
                                    EmbedBuilder embed = dolarService.CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar oficial")} del {Format.Bold(bank.GetDescription())} expresada en {Format.Bold("pesos argentinos")}.", null, thumbnailUrl);
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
                            await ReplyAsync($"Banco '{Format.Bold(userInput)}' inexistente. Verifique los bancos disponibles con {Format.Code($"{commandPrefix}{bankCommand}")}.").ConfigureAwait(false);
                        }
                    }
                    else
                    {   //Show all dollar types (not banks)
                        DolarResponse[] responses = await Task.WhenAll(Api.DolarArgentina.GetDollarPrice(DollarTypes.Oficial),
                                                                       Api.DolarArgentina.GetDollarPrice(DollarTypes.Ahorro),
                                                                       Api.DolarArgentina.GetDollarPrice(DollarTypes.Blue),
                                                                       Api.DolarArgentina.GetDollarPrice(DollarTypes.Bolsa),
                                                                       Api.DolarArgentina.GetDollarPrice(DollarTypes.Promedio),
                                                                       Api.DolarArgentina.GetDollarPrice(DollarTypes.ContadoConLiqui)).ConfigureAwait(false);
                        if (responses.Any(r => r != null))
                        {
                            DolarResponse[] successfulResponses = responses.Where(r => r != null).ToArray();
                            EmbedBuilder embed = dolarService.CreateDollarEmbed(successfulResponses);
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
                    DolarService dolarService = new DolarService(Configuration, Api);
                    DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarTypes.Oficial).ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = dolarService.CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar oficial")} expresada en {Format.Bold("pesos argentinos")}.");
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
        public async Task GetDolarAhorroPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    DolarService dolarService = new DolarService(Configuration, Api);
                    DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarTypes.Ahorro).ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = dolarService.CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar ahorro")} expresada en {Format.Bold("pesos argentinos")}.");
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
                    DolarService dolarService = new DolarService(Configuration, Api);
                    DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarTypes.Blue).ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = dolarService.CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar blue")} expresada en {Format.Bold("pesos argentinos")}.");
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
                    DolarService dolarService = new DolarService(Configuration, Api);
                    DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarTypes.Promedio).ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = dolarService.CreateDollarEmbed(result, $"Cotización {Format.Bold("promedio de los bancos del dólar oficial")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.");
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
                    DolarService dolarService = new DolarService(Configuration, Api);
                    DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarTypes.Bolsa).ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = dolarService.CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar bolsa (MEP)")} expresada en {Format.Bold("pesos argentinos")}.");
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
                    DolarService dolarService = new DolarService(Configuration, Api);
                    DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarTypes.ContadoConLiqui).ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = dolarService.CreateDollarEmbed(result, $"Cotización del {Format.Bold("dólar contado con liquidación")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.");
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