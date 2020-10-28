using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Modules.Services.Dolar;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DollarType = DolarBot.API.ApiCalls.DolarArgentinaApi.DollarType;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains the main theme related commands.
    /// </summary>
    [HelpOrder(1)]
    [HelpTitle("Cotizaciones")]
    public class MainModule : BaseInteractiveModule
    {
        #region Constants
        private const string REQUEST_ERROR_MESSAGE = "Error: No se pudo obtener la cotización. Intente nuevamente en más tarde.";
        #endregion

        #region Vars
        /// <summary>
        /// Color for the embed messages.
        /// </summary>
        private readonly Color mainEmbedColor = new Color(67, 181, 129);

        /// <summary>
        /// Provides access to the different APIs.
        /// </summary>
        protected readonly ApiCalls Api;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public MainModule(IConfiguration configuration, ApiCalls api) : base(configuration)
        {
            Api = api;
        }
        #endregion

        [Command("bancos")]
        [Alias("b")]
        [Summary("Muestra la lista de bancos disponibles para obtener las cotizaciones.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetBanks()
        {
            string commandPrefix = Configuration["commandPrefix"];
            string banks = string.Join(", ", Enum.GetNames(typeof(Banks)).Select(b => Format.Bold(b)));
            await ReplyAsync($"Parámetros disponibles del comando {Format.Code($"{commandPrefix}dolar <banco>")}: {banks}.").ConfigureAwait(false);
        }

        [Command("dolar", RunMode = RunMode.Async)]
        [Alias("d")]
        [Summary("Muestra todas las cotizaciones del dólar disponibles o por banco.")]
        [HelpUsageExample(false, "$dolar", "$d", "$dolar bancos", "$dolar santander", "$d galicia")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarPriceAsync(
            [Summary("Indica la cotización del banco a mostrar. Los valores posibles son aquellos devueltos por el comando `$bancos`.")]
            string banco = null)
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarService dolarService = new DolarService(Configuration, Api, mainEmbedColor);

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
                                DollarType dollarType = dolarService.GetBankInformation(banks[i], out string _);
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
                            DollarType dollarType = dolarService.GetBankInformation(bank, out string thumbnailUrl);
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
                        string bankCommand = GetType().GetMethod("GetBanks").GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;
                        await ReplyAsync($"Banco '{Format.Bold(userInput)}' inexistente. Verifique los bancos disponibles con {Format.Code($"{commandPrefix}{bankCommand}")}.").ConfigureAwait(false);
                    }
                }
                else
                {   //Show all dollar types (not banks)
                    DolarResponse[] responses = await Task.WhenAll(Api.DolarArgentina.GetDollarPrice(DollarType.Oficial),
                                                                   Api.DolarArgentina.GetDollarPrice(DollarType.Ahorro),
                                                                   Api.DolarArgentina.GetDollarPrice(DollarType.Blue),
                                                                   Api.DolarArgentina.GetDollarPrice(DollarType.Bolsa),
                                                                   Api.DolarArgentina.GetDollarPrice(DollarType.Promedio),
                                                                   Api.DolarArgentina.GetDollarPrice(DollarType.ContadoConLiqui)).ConfigureAwait(false);
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

        [Command("dolaroficial", RunMode = RunMode.Async)]
        [Alias("do")]
        [Summary("Muestra la cotización del dólar oficial.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarOficialPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarService dolarService = new DolarService(Configuration, Api, mainEmbedColor);
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Oficial).ConfigureAwait(false);
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

        [Command("dolarahorro", RunMode = RunMode.Async)]
        [Alias("da")]
        [Summary("Muestra la cotización del dólar oficial más impuesto P.A.I.S. y ganancias.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarAhorroPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarService dolarService = new DolarService(Configuration, Api, mainEmbedColor);
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Ahorro).ConfigureAwait(false);
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

        [Command("dolarblue", RunMode = RunMode.Async)]
        [Alias("db")]
        [Summary("Muestra la cotización del dólar blue.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarBluePriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarService dolarService = new DolarService(Configuration, Api, mainEmbedColor);
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Blue).ConfigureAwait(false);
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

        [Command("dolarpromedio", RunMode = RunMode.Async)]
        [Alias("dp")]
        [Summary("Muestra el promedio de las cotizaciones bancarias del dólar oficial.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarPromedioPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarService dolarService = new DolarService(Configuration, Api, mainEmbedColor);
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Promedio).ConfigureAwait(false);
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

        [Command("dolarbolsa", RunMode = RunMode.Async)]
        [Alias("dbo")]
        [Summary("Muestra la cotización del dólar bolsa (MEP).")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarBolsaPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarService dolarService = new DolarService(Configuration, Api, mainEmbedColor);
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.Bolsa).ConfigureAwait(false);
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

        [Command("contadoconliqui", RunMode = RunMode.Async)]
        [Alias("ccl")]
        [Summary("Muestra la cotización del dólar contado con liquidación.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetDolarContadoConLiquiPriceAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                DolarService dolarService = new DolarService(Configuration, Api, mainEmbedColor);
                DolarResponse result = await Api.DolarArgentina.GetDollarPrice(DollarType.ContadoConLiqui).ConfigureAwait(false);
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

        [Command("riesgopais", RunMode = RunMode.Async)]
        [Alias("rp")]
        [Summary("Muestra el valor del riesgo país.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetRiesgoPaisValueAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                RiesgoPaisResponse result = await Api.DolarArgentina.GetRiesgoPais().ConfigureAwait(false);
                if (result != null)
                {
                    RiesgoPaisService riesgoPaisService = new RiesgoPaisService(Configuration, Api, mainEmbedColor);
                    EmbedBuilder embed = riesgoPaisService.CreateRiesgoPaisEmbed(result);
                    await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                }
            }
        }
    }
}