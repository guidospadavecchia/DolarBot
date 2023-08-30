using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.API.Services.DolarBotApi;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Modules.InteractiveCommands.Choices;
using DolarBot.Modules.InteractiveCommands.Components.Calculator;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Buttons;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Enums;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Modals;
using DolarBot.Services.Banking;
using DolarBot.Services.Currencies;
using DolarBot.Services.Dolar;
using DolarBot.Util.Extensions;
using Fergun.Interactive;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace DolarBot.Modules.InteractiveCommands
{
    /// <summary>
    /// Contains the dollar related commands.
    /// </summary>
    [HelpOrder(1)]
    [HelpTitle("Cotizaciones del Dólar")]
    public class DolarInteractiveModule : BaseFiatCurrencyInteractiveModule<DolarService, DollarResponse>
    {
        #region Constants
        private const string ALL_STANDARD_RATES = "all-standard";
        private const string ALL_BANK_RATES = "all-banks";
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        /// <param name="interactiveService">The interactive service.</param>
        public DolarInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api, InteractiveService interactiveService) : base(configuration, logger, api, interactiveService) { }
        #endregion

        #region Methods

        /// <inheritdoc />
        protected override DolarService CreateService(IConfiguration configuration, ApiCalls api) => new(configuration, api);

        /// <inheritdoc />
        protected override Currencies GetCurrentCurrency() => Currencies.Dolar;

        /// <summary>
        /// Returns the description for the specified <paramref name="choice"/>.
        /// </summary>
        /// <param name="choice">The bank choice.</param>
        /// <returns>The choice description.</returns>
        private static string GetBankChoiceDescription(DollarBankChoices choice) => $"Cotización del {Format.Bold("dólar oficial")} del {Format.Bold(choice.GetAttribute<ChoiceDisplayAttribute>().Name)} expresada en {Format.Bold("pesos argentinos")}.";

        /// <summary>
        /// Returns the description for all the bank rates.
        /// </summary>
        /// <returns>The bank rates description.</returns>
        private static string GetAllBanksDescription() => $"Cotizaciones de {Format.Bold("bancos")} del {Format.Bold("dólar oficial")} expresadas en {Format.Bold("pesos argentinos")}.";

        /// <summary>
        /// Retrieves the response and description from the specified <paramref name="dollarChoice"/>.
        /// </summary>
        /// <param name="dollarChoice">The user choice.</param>
        /// <returns>An asynchronous task containing the response.</returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<(DollarResponse, string)> GetStandardRate(DollarChoices dollarChoice)
        {
            string description;
            DollarResponse response;
            switch (dollarChoice)
            {
                case DollarChoices.Oficial:
                    description = $"Cotización del {Format.Bold("dólar oficial")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetDollarOficial();
                    break;
                case DollarChoices.Ahorro:
                    description = $"Cotización del {Format.Bold("dólar ahorro")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetDollarAhorro();
                    break;
                case DollarChoices.Tarjeta:
                    description = $"Cotización del {Format.Bold("dólar tarjeta")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetDollarTarjeta();
                    break;
                case DollarChoices.Qatar:
                    description = $"Cotización del {Format.Bold("dólar Qatar")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetDollarQatar();
                    break;
                case DollarChoices.Blue:
                    description = $"Cotización del {Format.Bold("dólar blue")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetDollarBlue();
                    break;
                case DollarChoices.Promedio:
                    description = $"Cotización {Format.Bold("promedio de los bancos del dólar oficial")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetDollarPromedio();
                    break;
                case DollarChoices.Bolsa:
                    description = $"Cotización del {Format.Bold("dólar bolsa (MEP)")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetDollarBolsa();
                    break;
                case DollarChoices.ContadoConLiquidacion:
                    description = $"Cotización del {Format.Bold("dólar contado con liquidación")}{Environment.NewLine} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetDollarContadoConLiqui();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return (response, description);
        }

        #endregion

        #region Components

        [ComponentInteraction($"{DolarCalculatorButtonBuilder.Id}:*", runMode: RunMode.Async)]
        public async Task HandleCalculatorButtonClick(string choice)
        {
            await RespondWithModalAsync<DolarCalculatorModal>($"{DolarCalculatorModal.Id}:{choice}");
        }

        [ModalInteraction($"{DolarCalculatorModal.Id}:*", runMode: RunMode.Async)]
        public async Task HandleCalculatorModalInput(string choice, DolarCalculatorModal calculatorModal)
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                bool isNumeric = decimal.TryParse(calculatorModal.Value.Replace(",", "."), NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal amount);
                if (!isNumeric || amount <= 0)
                {
                    amount = 1;
                }
                if (choice.StartsWith("bank:"))
                {
                    string bankChoice = choice.Split(":")[1];
                    if (bankChoice == ALL_BANK_RATES)
                    {
                        string description = GetAllBanksDescription();
                        await SendAllBankRates(description, amount);
                    }
                    else
                    {
                        DollarBankChoices dollarBankChoice = Enum.Parse<DollarBankChoices>(bankChoice);
                        Banks bank = Enum.Parse<Banks>(dollarBankChoice.ToString());
                        string description = GetBankChoiceDescription(dollarBankChoice);
                        await SendBankRate(bank, description, amount);
                    }
                }
                else
                {
                    if (choice == ALL_STANDARD_RATES)
                    {
                        await SendAllStandardRates(amount);
                    }
                    else
                    {
                        DollarChoices dollarChoice = Enum.Parse<DollarChoices>(choice);
                        var result = await GetStandardRate(dollarChoice);
                        await SendStandardRate(result.Item1, result.Item2, amount);
                    }
                }
            });
        }

        #endregion

        [SlashCommand("dolar", "Muestra las cotizaciones del dólar.", false, RunMode.Async)]
        public async Task GetDolarPriceAsync(
        [Summary("tipo", "Indica el tipo de cotización a mostrar.")]
            DollarChoices? dollarChoice = null)
        {
            try
            {
                await DeferAsync();
                if (dollarChoice == null)
                {
                    await SendAllStandardRates(components: new CalculatorComponentBuilder(ALL_STANDARD_RATES, CalculatorTypes.Dollar, Configuration).Build());
                }
                else
                {
                    var result = await GetStandardRate(dollarChoice.Value);
                    await SendStandardRate(result.Item1, result.Item2, components: new CalculatorComponentBuilder(dollarChoice.ToString(), CalculatorTypes.Dollar, Configuration).Build());
                }
            }
            catch (Exception ex)
            {
                await FollowUpWithErrorResponseAsync(ex);
            }
        }

        [SlashCommand("dolar-bancos", "Muestra las cotizaciones bancarias del dólar.", false, RunMode.Async)]
        public async Task GetBankDollarPriceAsync(
            [Summary("banco", "Indica el banco a mostrar. Si no se especifica, se mostrarán todas las cotizaciones bancarias.")]
            DollarBankChoices? bankChoice = null)
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    if (bankChoice != null)
                    {
                        string description = GetBankChoiceDescription(bankChoice.Value);
                        Banks bank = Enum.Parse<Banks>(bankChoice.ToString());
                        await SendBankRate(bank, description, components: new CalculatorComponentBuilder($"bank:{bankChoice}", CalculatorTypes.Dollar, Configuration).Build());
                    }
                    else
                    {
                        string description = GetAllBanksDescription();
                        await SendAllBankRates(description, components: new CalculatorComponentBuilder($"bank:{ALL_BANK_RATES}", CalculatorTypes.Dollar, Configuration).Build());
                    }
                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
                }
            });
        }
    }
}