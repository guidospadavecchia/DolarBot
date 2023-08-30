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
using DolarBot.Services.Euro;
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
    /// Contains the euro related commands.
    /// </summary>
    [HelpOrder(2)]
    [HelpTitle("Cotizaciones del Euro")]
    public class EuroInteractiveModule : BaseFiatCurrencyInteractiveModule<EuroService, EuroResponse>
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
        public EuroInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api, InteractiveService interactiveService) : base(configuration, logger, api, interactiveService) { }
        #endregion

        #region Methods

        /// <inheritdoc />
        protected override EuroService CreateService(IConfiguration configuration, ApiCalls api) => new(configuration, api);

        /// <inheritdoc />
        protected override Currencies GetCurrentCurrency() => Currencies.Euro;

        /// <summary>
        /// Returns the description for the specified <paramref name="choice"/>.
        /// </summary>
        /// <param name="choice">The bank choice.</param>
        /// <returns>The choice description.</returns>
        private static string GetBankChoiceDescription(EuroBankChoices choice) => $"Cotización del {Format.Bold("Euro oficial")} del {Format.Bold(choice.GetAttribute<ChoiceDisplayAttribute>().Name)} expresada en {Format.Bold("pesos argentinos")}.";

        /// <summary>
        /// Returns the description for all the bank rates.
        /// </summary>
        /// <returns>The bank rates description.</returns>
        private static string GetAllBanksDescription() => $"Cotizaciones de {Format.Bold("bancos")} del {Format.Bold("Euro oficial")} expresadas en {Format.Bold("pesos argentinos")}.";

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
                case EuroChoices.Tarjeta:
                    description = $"Cotización del {Format.Bold("Euro tarjeta")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetEuroTarjeta();
                    break;
                case EuroChoices.Qatar:
                    description = $"Cotización del {Format.Bold("Euro Qatar")} expresada en {Format.Bold("pesos argentinos")}.";
                    response = await Service.GetEuroQatar();
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

        #endregion

        #region Components

        [ComponentInteraction($"{EuroCalculatorButtonBuilder.Id}:*", runMode: RunMode.Async)]
        public async Task HandleCalculatorButtonClick(string choice)
        {
            await RespondWithModalAsync<EuroCalculatorModal>($"{EuroCalculatorModal.Id}:{choice}");
        }

        [ModalInteraction($"{EuroCalculatorModal.Id}:*", runMode: RunMode.Async)]
        public async Task HandleCalculatorModalInput(string choice, EuroCalculatorModal calculatorModal)
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
                        EuroBankChoices euroBankChoice = Enum.Parse<EuroBankChoices>(bankChoice);
                        Banks bank = Enum.Parse<Banks>(euroBankChoice.ToString());
                        string description = GetBankChoiceDescription(euroBankChoice);
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
                        EuroChoices euroChoice = Enum.Parse<EuroChoices>(choice);
                        var result = await GetStandardRate(euroChoice);
                        await SendStandardRate(result.Item1, result.Item2, amount);
                    }
                }
            });
        }

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
                        await SendAllStandardRates(components: new CalculatorComponentBuilder(ALL_STANDARD_RATES, CalculatorTypes.Euro, Configuration).Build());
                    }
                    else
                    {
                        var result = await GetStandardRate(euroChoice.Value);
                        await SendStandardRate(result.Item1, result.Item2, components: new CalculatorComponentBuilder(euroChoice.ToString(), CalculatorTypes.Euro, Configuration).Build());
                    }
                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
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
                        string description = GetBankChoiceDescription(bankChoice.Value);
                        Banks bank = Enum.Parse<Banks>(bankChoice.ToString());
                        await SendBankRate(bank, description, components: new CalculatorComponentBuilder($"bank:{bankChoice}", CalculatorTypes.Euro, Configuration).Build());
                    }
                    else
                    {
                        string description = GetAllBanksDescription();
                        await SendAllBankRates(description, components: new CalculatorComponentBuilder($"bank:{ALL_BANK_RATES}", CalculatorTypes.Euro, Configuration).Build());
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