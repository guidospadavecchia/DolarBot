using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Modules.InteractiveCommands.Choices;
using DolarBot.Services.Banking;
using DolarBot.Services.Currencies;
using DolarBot.Services.Euro;
using DolarBot.Util.Extensions;
using Fergun.Interactive;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DolarBot.Modules.InteractiveCommands
{
    /// <summary>
    /// Contains the euro related commands.
    /// </summary>
    [HelpOrder(2)]
    [HelpTitle("Cotizaciones del Euro")]
    public class EuroInteractiveModule : BaseFiatCurrencyInteractiveModule<EuroService, EuroResponse>
    {
        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        /// <param name="interactiveService">The interactive service.</param>
        public EuroInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api, InteractiveService interactiveService) : base(configuration, logger, api, interactiveService) { }
        #endregion

        #region Methods

        /// <inheritdoc />
        protected override EuroService CreateService(IConfiguration configuration, ApiCalls api) => new(configuration, api);

        /// <inheritdoc />
        protected override Currencies GetCurrentCurrency() => Currencies.Euro;

        #endregion

        [SlashCommand("euro", "Muestra las cotizaciones del euro.", false, RunMode.Async)]
        public async Task GetEuroPriceAsync(
        [Summary("tipo", "Indica el tipo de cotización a mostrar.")]
            EuroChoices? euroChoice = null)
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    if (euroChoice == null)
                    {
                        await SendAllStandardRates();
                    }
                    else
                    {
                        var result = await GetStandardRate(euroChoice.Value);
                        await SendStandardRate(result.Item1, result.Item2);
                    }
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponseAsync(ex);
                }
            });
        }

        [SlashCommand("euro-bancos", "Muestra las cotizaciones bancarias del euro.", false, RunMode.Async)]
        public async Task GetBankEuroPriceAsync(
            [Summary("banco", "Indica el banco a mostrar. Si no se especifica, se mostrarán todas las cotizaciones bancarias.")]
            EuroBankChoices? bankChoice = null)
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    if (bankChoice != null)
                    {
                        string description = $"Cotización del {Format.Bold("Euro oficial")} del {Format.Bold(bankChoice.Value.GetDescription())} expresada en {Format.Bold("pesos argentinos")}.";
                        Banks bank = Enum.Parse<Banks>(bankChoice.ToString());
                        await SendBankRate(bank, description);
                    }
                    else
                    {
                        string description = $"Cotizaciones de {Format.Bold("bancos")} del {Format.Bold("Euro oficial")} expresadas en {Format.Bold("pesos argentinos")}.";
                        await SendAllBankRates(description);
                    }
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponseAsync(ex);
                }
            });
        }

        /// <summary>
        /// Retrieves the response and description from the specified <paramref name="euroChoice"/>.
        /// </summary>
        /// <param name="euroChoice">The user choice.</param>
        /// <returns>An asynchronous task containing the response.</returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<(EuroResponse, string)> GetStandardRate(EuroChoices euroChoice)
        {
            string description;
            EuroResponse response;
            switch (euroChoice)
            {
                case EuroChoices.Oficial:
                    description = $"Cotización del {Format.Bold("Euro oficial")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetEuroOficial();
                    break;
                case EuroChoices.Ahorro:
                    description = $"Cotización del {Format.Bold("Euro ahorro")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetEuroAhorro();
                    break;
                case EuroChoices.Blue:
                    description = $"Cotización del {Format.Bold("Euro blue")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetEuroBlue();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return (response, description);
        }
    }
}