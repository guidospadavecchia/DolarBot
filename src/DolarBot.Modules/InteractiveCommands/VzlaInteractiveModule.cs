using Discord;
using Discord.Interactions;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.API.Services.DolarBotApi;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Modules.InteractiveCommands.Components.Calculator;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Buttons;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Enums;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Modals;
using DolarBot.Services.Currencies;
using DolarBot.Services.Venezuela;
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
    [HelpOrder(10)]
    [HelpTitle("Cotizaciones del Venezuela")]
    public class VzlaInteractiveModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about Venezuela's currency.
        /// </summary>
        private readonly VzlaService VzlaService;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        /// <param name="interactiveService">The interactive service.</param>
        public VzlaInteractiveModule(IConfiguration configuration, ILog logger, ApiCalls api, InteractiveService interactiveService) : base(configuration, logger, interactiveService)
        {
            VzlaService = new VzlaService(configuration, api);
        }
        #endregion

        #region Components

        [ComponentInteraction($"{VzlaCalculatorButtonBuilder.Id}:*", runMode: RunMode.Async)]
        public async Task HandleCalculatorButtonClick(string currency)
        {
            await RespondWithModalAsync<VzlaCalculatorModal>($"{VzlaCalculatorModal.Id}:{currency}");
        }

        [ModalInteraction($"{VzlaCalculatorModal.Id}:*", runMode: RunMode.Async)]
        public async Task HandleCalculatorModalInput(string currencyCode, VzlaCalculatorModal calculatorModal)
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    bool isNumeric = decimal.TryParse(calculatorModal.Value.Replace(",", "."), NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal amount);
                    if (!isNumeric || amount <= 0)
                    {
                        amount = 1;
                    }

                    Currencies currency = Enum.Parse<Currencies>(currencyCode);
                    VzlaResponse result = currency switch
                    {
                        Currencies.Dolar => await VzlaService.GetDollarRates(),
                        Currencies.Euro => await VzlaService.GetEuroRates(),
                        _ => throw new NotImplementedException(),
                    };
                    if (result != null)
                    {
                        EmbedBuilder embed = await VzlaService.CreateVzlaEmbedAsync(result, amount);
                        await FollowupAsync(embed: embed.Build());
                    }
                    else
                    {
                        await FollowUpWithApiErrorResponseAsync();
                    }
                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
                }
            });
        }

        #endregion

        [SlashCommand("bolivar-dolar", "Muestra las distintas cotizaciones del dólar para Venezuela.", false, RunMode.Async)]
        public async Task GetDollarRatesAsync()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    VzlaResponse result = await VzlaService.GetDollarRates();
                    if (result != null)
                    {
                        EmbedBuilder embed = await VzlaService.CreateVzlaEmbedAsync(result);
                        await FollowupAsync(embed: embed.Build(), components: new CalculatorComponentBuilder(Currencies.Dolar.ToString(), CalculatorTypes.Venezuela, Configuration).Build());
                    }
                    else
                    {
                        await FollowUpWithApiErrorResponseAsync();
                    }
                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
                }
            });
        }

        //[SlashCommand("bolivar-euro", "Muestra las distintas cotizaciones del Euro para Venezuela.", false, RunMode.Async)]
        public async Task GetEuroRatesAsync()
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    VzlaResponse result = await VzlaService.GetEuroRates();
                    if (result != null)
                    {
                        EmbedBuilder embed = await VzlaService.CreateVzlaEmbedAsync(result);
                        await FollowupAsync(embed: embed.Build(), components: new CalculatorComponentBuilder(Currencies.Euro.ToString(), CalculatorTypes.Venezuela, Configuration).Build());
                    }
                    else
                    {
                        await FollowUpWithApiErrorResponseAsync();
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