using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Banking;
using DolarBot.Services.Currencies;
using DolarBot.Services.Dolar;
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
    public class DolarModule : BaseFiatCurrencyModule<DolarService, DollarResponse>
    {
        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public DolarModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger, api) { }
        #endregion

        #region Methods

        /// <inheritdoc />
        protected override DolarService CreateService(IConfiguration configuration, ApiCalls api) => new(configuration, api);

        /// <inheritdoc />
        protected override Currencies GetCurrentCurrency() => Currencies.Dolar;

        #endregion

        [Command("dolar", RunMode = RunMode.Async)]
        [Alias("d")]
        [Summary("Muestra todas las cotizaciones del dólar disponibles o por banco.")]
        [HelpUsageExample(false, "$dolar", "$d", "$dolar bancos", "$dolar santander", "$d galicia")]
        [RateLimit(1, 3, Measure.Seconds)]
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
                        if (!userInput.IsNumeric() && Enum.TryParse(userInput, true, out Banks bank))
                        {
                            if (bank == Banks.Bancos)
                            {
                                string description = $"Cotizaciones de {Format.Bold("bancos")} del {Format.Bold("dólar oficial")} expresadas en {Format.Bold("pesos argentinos")}.";
                                await SendAllBankRates(description);
                            }
                            else
                            {
                                if (Service.GetValidBanks().Contains(bank))
                                {
                                    string description = $"Cotización del {Format.Bold("dólar oficial")} del {Format.Bold(bank.GetDescription())} expresada en {Format.Bold("pesos argentinos")}.";
                                    await SendBankRate(bank, description);
                                }
                                else
                                {
                                    await SendInvalidBankForCurrency(bank);
                                }
                            }
                        }
                        else
                        {
                            await SendUnknownBankParameter(userInput);
                        }
                    }
                    else
                    {
                        await SendAllStandardRates();
                    }
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("dolaroficial", RunMode = RunMode.Async)]
        [Alias("do")]
        [Summary("Muestra la cotización del dólar oficial.")]
        [RateLimit(1, 3, Measure.Seconds)]
        [HelpUsageExample(false, "$dolaroficial", "$do")]
        public async Task GetDolarOficialPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    DollarResponse result = await Service.GetDollarOficial();
                    string description = $"Cotización del {Format.Bold("dólar oficial")} expresada en {Format.Bold("pesos argentinos")}.";
                    await SendStandardRate(result, description);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("dolarahorro", RunMode = RunMode.Async)]
        [Alias("da")]
        [Summary("Muestra la cotización del dólar oficial más impuesto P.A.I.S. y deducción de ganancias.")]
        [RateLimit(1, 3, Measure.Seconds)]
        [HelpUsageExample(false, "$dolarahorro", "$da")]
        public async Task GetDolarAhorroPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    DollarResponse result = await Service.GetDollarAhorro();
                    string description = $"Cotización del {Format.Bold("dólar ahorro")} expresada en {Format.Bold("pesos argentinos")}.";
                    await SendStandardRate(result, description);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("dolarblue", RunMode = RunMode.Async)]
        [Alias("db")]
        [Summary("Muestra la cotización del dólar blue.")]
        [RateLimit(1, 3, Measure.Seconds)]
        [HelpUsageExample(false, "$dolarblue", "$db")]
        public async Task GetDolarBluePriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    DollarResponse result = await Service.GetDollarBlue();
                    string description = $"Cotización del {Format.Bold("dólar blue")} expresada en {Format.Bold("pesos argentinos")}.";
                    await SendStandardRate(result, description);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("dolarpromedio", RunMode = RunMode.Async)]
        [Alias("dp")]
        [Summary("Muestra el promedio de las cotizaciones bancarias del dólar oficial.")]
        [RateLimit(1, 3, Measure.Seconds)]
        [HelpUsageExample(false, "$dolarpromedio", "$dp")]
        public async Task GetDolarPromedioPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    DollarResponse result = await Service.GetDollarPromedio();
                    string description = $"Cotización {Format.Bold("promedio de los bancos del dólar oficial")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.";
                    await SendStandardRate(result, description);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("dolarbolsa", RunMode = RunMode.Async)]
        [Alias("dbo")]
        [Summary("Muestra la cotización del dólar bolsa (MEP).")]
        [RateLimit(1, 3, Measure.Seconds)]
        [HelpUsageExample(false, "$dolarbolsa", "$dbo")]
        public async Task GetDolarBolsaPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    DollarResponse result = await Service.GetDollarBolsa();
                    string description = $"Cotización del {Format.Bold("dólar bolsa (MEP)")} expresada en {Format.Bold("pesos argentinos")}.";
                    await SendStandardRate(result, description);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("contadoconliqui", RunMode = RunMode.Async)]
        [Alias("ccl")]
        [Summary("Muestra la cotización del dólar contado con liquidación.")]
        [RateLimit(1, 3, Measure.Seconds)]
        [HelpUsageExample(false, "$contadoconliqui", "$ccl")]
        public async Task GetDolarContadoConLiquiPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    DollarResponse result = await Service.GetDollarContadoConLiqui();
                    string description = $"Cotización del {Format.Bold("dólar contado con liquidación")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.";
                    await SendStandardRate(result, description);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }
    }
}