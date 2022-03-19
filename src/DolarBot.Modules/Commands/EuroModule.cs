using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Banking;
using DolarBot.Services.Currencies;
using DolarBot.Services.Euro;
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
    public class EuroModule : BaseFiatCurrencyModule<EuroService, EuroResponse>
    {
        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public EuroModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger, api) { }
        #endregion

        #region Methods

        /// <inheritdoc />
        protected override EuroService CreateService(IConfiguration configuration, ApiCalls api) => new EuroService(configuration, api);

        /// <inheritdoc />
        protected override Currencies GetCurrentCurrency() => Currencies.Euro;

        #endregion

        [Command("euro", RunMode = RunMode.Async)]
        [Alias("e")]
        [Summary("Muestra todas las cotizaciones del Euro oficial disponibles.")]
        [HelpUsageExample(false, "$euro", "$e", "$euro Nacion", "$e BBVA")]
        [RateLimit(1, 3, Measure.Seconds)]
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
                        if (!userInput.IsNumeric() && Enum.TryParse(userInput, true, out Banks bank))
                        {
                            if (bank == Banks.Bancos)
                            {
                                string description = $"Cotizaciones de {Format.Bold("bancos")} del {Format.Bold("Euro oficial")} expresadas en {Format.Bold("pesos argentinos")}.";
                                await SendAllBankRates(description);
                            }
                            else
                            {
                                if (Service.GetValidBanks().Contains(bank))
                                {
                                    string description = $"Cotización del {Format.Bold("Euro oficial")} del {Format.Bold(bank.GetDescription())} expresada en {Format.Bold("pesos argentinos")}.";
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

        [Command("eurooficial", RunMode = RunMode.Async)]
        [Alias("eo")]
        [Summary("Muestra la cotización del Euro oficial.")]
        [RateLimit(1, 3, Measure.Seconds)]
        [HelpUsageExample(false, "$eurooficial", "$eo")]
        public async Task GetEuroOficialPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    EuroResponse result = await Service.GetEuroOficial();
                    string description = $"Cotización del {Format.Bold("Euro oficial")} expresada en {Format.Bold("pesos argentinos")}.";
                    await SendStandardRate(result, description);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("euroahorro", RunMode = RunMode.Async)]
        [Alias("ea")]
        [Summary("Muestra la cotización del Euro oficial más impuesto P.A.I.S. y deducción de ganancias.")]
        [RateLimit(1, 3, Measure.Seconds)]
        [HelpUsageExample(false, "$euroahorro", "$ea")]
        public async Task GetEuroAhorroPriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    EuroResponse result = await Service.GetEuroAhorro();
                    string description = $"Cotización del {Format.Bold("Euro ahorro")} expresada en {Format.Bold("pesos argentinos")}.";
                    await SendStandardRate(result, description);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("euroblue", RunMode = RunMode.Async)]
        [Alias("eb")]
        [Summary("Muestra la cotización del Euro blue.")]
        [RateLimit(1, 3, Measure.Seconds)]
        [HelpUsageExample(false, "$euroblue", "$eb")]
        public async Task GetEuroBluePriceAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    EuroResponse result = await Service.GetEuroBlue();
                    string description = $"Cotización del {Format.Bold("Euro blue")} expresada en {Format.Bold("pesos argentinos")}.";
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