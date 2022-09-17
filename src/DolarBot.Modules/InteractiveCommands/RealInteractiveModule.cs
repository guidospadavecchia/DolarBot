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
using DolarBot.Services.Real;
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
    /// Contains the Real related commands.
    /// </summary>
    [HelpOrder(3)]
    [HelpTitle("Cotizaciones del Real")]
    public class RealInteractiveModule : BaseFiatCurrencyInteractiveModule<RealService, RealResponse>
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
        public RealInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api, InteractiveService interactiveService) : base(configuration, logger, api, interactiveService) { }
        #endregion

        #region Methods

        /// <inheritdoc />
        protected override RealService CreateService(IConfiguration configuration, ApiCalls api) => new(configuration, api);

        /// <inheritdoc />
        protected override Currencies GetCurrentCurrency() => Currencies.Real;

        /// <summary>
        /// Returns the description for the specified <paramref name="choice"/>.
        /// </summary>
        /// <param name="choice">The bank choice.</param>
        /// <returns>The choice description.</returns>
        private static string GetBankChoiceDescription(RealBankChoices choice) => $"Cotización del {Format.Bold("Real oficial")} del {Format.Bold(choice.GetAttribute<ChoiceDisplayAttribute>().Name)} expresada en {Format.Bold("pesos argentinos")}.";

        /// <summary>
        /// Returns the description for all the bank rates.
        /// </summary>
        /// <returns>The bank rates description.</returns>
        private static string GetAllBanksDescription() => $"Cotizaciones de {Format.Bold("bancos")} del {Format.Bold("Real oficial")} expresadas en {Format.Bold("pesos argentinos")}.";

        /// <summary>
        /// Retrieves the response and description from the specified <paramref name="realChoice"/>.
        /// </summary>
        /// <param name="realChoice">The user choice.</param>
        /// <returns>An asynchronous task containing the response.</returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<(RealResponse, string)> GetStandardRate(RealChoices realChoice)
        {
            string description;
            RealResponse response;
            switch (realChoice)
            {
                case RealChoices.Oficial:
                    description = $"Cotización del {Format.Bold("Real oficial")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetRealOficial();
                    break;
                case RealChoices.Ahorro:
                    description = $"Cotización del {Format.Bold("Real ahorro")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetRealAhorro();
                    break;
                case RealChoices.Tarjeta:
                    description = $"Cotización del {Format.Bold("Real tarjeta")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetRealTarjeta();
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

        #endregion

        #region Components

        [ComponentInteraction($"{RealCalculatorButtonBuilder.Id}:*", runMode: RunMode.Async)]
        public async Task HandleCalculatorButtonClick(string choice)
        {
            await RespondWithModalAsync<RealCalculatorModal>($"{RealCalculatorModal.Id}:{choice}");
        }

        [ModalInteraction($"{RealCalculatorModal.Id}:*", runMode: RunMode.Async)]
        public async Task HandleCalculatorModalInput(string choice, RealCalculatorModal calculatorModal)
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
                        RealBankChoices realBankChoice = Enum.Parse<RealBankChoices>(bankChoice);
                        Banks bank = Enum.Parse<Banks>(realBankChoice.ToString());
                        string description = GetBankChoiceDescription(realBankChoice);
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
                        RealChoices realChoice = Enum.Parse<RealChoices>(choice);
                        var result = await GetStandardRate(realChoice);
                        await SendStandardRate(result.Item1, result.Item2, amount);
                    }
                }
            });
        }

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
                        await SendAllStandardRates(components: new CalculatorComponentBuilder(ALL_STANDARD_RATES, CalculatorTypes.Real, Configuration).Build());
                    }
                    else
                    {
                        var result = await GetStandardRate(realChoice.Value);
                        await SendStandardRate(result.Item1, result.Item2, components: new CalculatorComponentBuilder(realChoice.ToString(), CalculatorTypes.Real, Configuration).Build());
                    }
                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
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
                        string description = GetBankChoiceDescription(bankChoice.Value);
                        Banks bank = Enum.Parse<Banks>(bankChoice.ToString());
                        await SendBankRate(bank, description, components: new CalculatorComponentBuilder($"bank:{bankChoice}", CalculatorTypes.Real, Configuration).Build());
                    }
                    else
                    {
                        string description = GetAllBanksDescription();
                        await SendAllBankRates(description, components: new CalculatorComponentBuilder($"bank:{ALL_BANK_RATES}", CalculatorTypes.Real, Configuration).Build());
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