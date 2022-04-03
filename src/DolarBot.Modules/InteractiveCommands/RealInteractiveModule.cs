using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Modules.InteractiveCommands.Choices;
using DolarBot.Services.Banking;
using DolarBot.Services.Currencies;
using DolarBot.Services.Real;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DolarBot.Modules.InteractiveCommands
{
    /// <summary>
    /// Contains the Real related commands.
    /// </summary>
    [HelpOrder(3)]
    [HelpTitle("Cotizaciones del Real")]
    public class RealInteractiveModule : BaseFiatCurrencyInteractiveModule<RealService, RealResponse>
    {
        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public RealInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger, api) { }
        #endregion

        #region Methods

        /// <inheritdoc />
        protected override RealService CreateService(IConfiguration configuration, ApiCalls api) => new(configuration, api);

        /// <inheritdoc />
        protected override Currencies GetCurrentCurrency() => Currencies.Real;

        #endregion

        [SlashCommand("real", "Muestra las cotizaciones del real.", false, RunMode.Async)]
        public async Task GetRealPriceAsync(
        [Summary("tipo", "Indica el tipo de cotización a mostrar.")]
            RealChoices? realChoice = null)
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    if (realChoice == null)
                    {
                        await SendAllStandardRates();
                    }
                    else
                    {
                        var result = await GetStandardRate(realChoice.Value);
                        await SendStandardRate(result.Item1, result.Item2);
                    }
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponse(ex);
                }
            });
        }

        [SlashCommand("real-bancos", "Muestra las cotizaciones bancarias del real.", false, RunMode.Async)]
        public async Task GetBankRealPriceAsync(
            [Summary("banco", "Indica el banco a mostrar. Si no se especifica, se mostrarán todas las cotizaciones bancarias.")]
            RealBankChoices? bankChoice = null)
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    if (bankChoice != null)
                    {
                        string description = $"Cotización del {Format.Bold("Real oficial")} del {Format.Bold(bankChoice.Value.GetDescription())} expresada en {Format.Bold("pesos argentinos")}.";
                        Banks bank = Enum.Parse<Banks>(bankChoice.ToString());
                        await SendBankRate(bank, description);
                    }
                    else
                    {
                        string description = $"Cotizaciones de {Format.Bold("bancos")} del {Format.Bold("Real oficial")} expresadas en {Format.Bold("pesos argentinos")}.";
                        await SendAllBankRates(description);
                    }
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponse(ex);
                }
            });
        }

        /// <summary>
        /// Retrieves the response and description from the specified <paramref name="euroChoice"/>.
        /// </summary>
        /// <param name="euroChoice">The user choice.</param>
        /// <returns>An asynchronous task containing the response.</returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<(RealResponse, string)> GetStandardRate(RealChoices euroChoice)
        {
            string description;
            RealResponse response;
            switch (euroChoice)
            {
                case RealChoices.Oficial:
                    description = $"Cotización del {Format.Bold("Real oficial")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetRealOficial();
                    break;
                case RealChoices.Ahorro:
                    description = $"Cotización del {Format.Bold("Real ahorro")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetRealAhorro();
                    break;
                case RealChoices.Blue:
                    description = $"Cotización del {Format.Bold("Real blue")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetRealBlue();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return (response, description);
        }
    }
}