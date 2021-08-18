using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Banking;
using DolarBot.Services.Currencies;
using DolarBot.Services.Real;
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
    public class RealModule : BaseFiatCurrencyModule<RealService, RealResponse>
    {
        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public RealModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger, api) { }
        #endregion

        #region Methods

        /// <inheritdoc />
        protected override RealService CreateService(IConfiguration configuration, ApiCalls api) => new RealService(configuration, api);

        /// <inheritdoc />
        protected override Currencies GetCurrentCurrency() => Currencies.Real;

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
                            if (bank == Banks.Bancos)
                            {
                                string description = $"Cotizaciones disponibles del {Format.Bold("Real oficial")} expresadas en {Format.Bold("pesos argentinos")}.";
                                await SendAllBankRates(description);
                            }
                            else
                            {
                                if (Service.GetValidBanks().Contains(bank))
                                {
                                    string description = $"Cotización del {Format.Bold("Real oficial")} del {Format.Bold(bank.GetDescription())} expresada en {Format.Bold("pesos argentinos")}.";
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

        [Command("realoficial", RunMode = RunMode.Async)]
        [Alias("ro")]
        [Summary("Muestra la cotización del Real oficial.")]
        [RateLimit(1, 3, Measure.Seconds)]
        [HelpUsageExample(false, "$realoficial", "$ro")]
        public async Task GetRealOficialPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    RealResponse result = await Service.GetRealOficial();
                    string description = $"Cotización del {Format.Bold("Real oficial")} expresada en {Format.Bold("pesos argentinos")}.";
                    await SendStandardRate(result, description);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("realahorro", RunMode = RunMode.Async)]
        [Alias("ra")]
        [Summary("Muestra la cotización del Real oficial más impuesto P.A.I.S. y deducción de ganancias.")]
        [RateLimit(1, 3, Measure.Seconds)]
        [HelpUsageExample(false, "$realahorro", "$ra")]
        public async Task GetRealAhorroPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    RealResponse result = await Service.GetRealAhorro();
                    string description = $"Cotización del {Format.Bold("Real ahorro")} expresada en {Format.Bold("pesos argentinos")}.";
                    await SendStandardRate(result, description);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("realblue", RunMode = RunMode.Async)]
        [Alias("rb")]
        [Summary("Muestra la cotización del Real blue.")]
        [RateLimit(1, 3, Measure.Seconds)]
        [HelpUsageExample(false, "$realblue", "$rb")]
        public async Task GetRealBluePriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    RealResponse result = await Service.GetRealBlue();
                    string description = $"Cotización del {Format.Bold("Real blue")} expresada en {Format.Bold("pesos argentinos")}.";
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