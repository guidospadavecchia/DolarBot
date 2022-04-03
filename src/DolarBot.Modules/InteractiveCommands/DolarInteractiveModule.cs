using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Modules.InteractiveCommands.Choices;
using DolarBot.Services.Banking;
using DolarBot.Services.Currencies;
using DolarBot.Services.Dolar;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
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
        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public DolarInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger, api) { }
        #endregion

        #region Methods

        /// <inheritdoc />
        protected override DolarService CreateService(IConfiguration configuration, ApiCalls api) => new(configuration, api);

        /// <inheritdoc />
        protected override Currencies GetCurrentCurrency() => Currencies.Dolar;

        #endregion

        [SlashCommand("dolar", "Muestra las cotizaciones del dólar.", false, RunMode.Async)]
        public async Task GetDolarPriceAsync(
        [Summary("tipo", "Indica el tipo de cotización a mostrar.")]
            DollarChoices? dollarChoice = null)
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    if (dollarChoice == null)
                    {
                        await SendAllStandardRates();
                    }
                    else
                    {
                        var result = await GetStandardRate(dollarChoice.Value);
                        await SendStandardRate(result.Item1, result.Item2);
                    }
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponse(Context.Interaction, ex);
                }
            });
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
                        string description = $"Cotización del {Format.Bold("dólar oficial")} del {Format.Bold(bankChoice.Value.GetDescription())} expresada en {Format.Bold("pesos argentinos")}.";
                        Banks bank = Enum.Parse<Banks>(bankChoice.ToString());
                        await SendBankRate(bank, description);
                    }
                    else
                    {
                        string description = $"Cotizaciones de {Format.Bold("bancos")} del {Format.Bold("dólar oficial")} expresadas en {Format.Bold("pesos argentinos")}.";
                        await SendAllBankRates(description);
                    }
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponse(Context.Interaction, ex);
                }
            });
        }

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
    }
}