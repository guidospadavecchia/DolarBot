using Discord;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Buttons;
using DolarBot.Modules.InteractiveCommands.Components.Calculator.Enums;
using Microsoft.Extensions.Configuration;

namespace DolarBot.Modules.InteractiveCommands.Components.Calculator
{
    /// <summary>
    /// Calculator component.
    /// </summary>
    public class CalculatorComponentBuilder : ComponentBuilder
    {
        /// <summary>
        /// Constructs a new <see cref="CalculatorComponentBuilder"/>.
        /// </summary>
        /// <param name="tag">A custom tag to identify the component or pass data.</param>
        public CalculatorComponentBuilder(string tag, CalculatorTypes type, IConfiguration configuration) : base()
        {
            var emojis = configuration.GetSection("customEmojis");
            Emote emote = Emote.Parse(emojis["calculator"]);
            ButtonBuilder calculatorButton = type switch
            {
                CalculatorTypes.Crypto => new CryptoCalculatorButtonBuilder(tag, emote),
                CalculatorTypes.Dollar => new DolarCalculatorButtonBuilder(tag, emote),
                CalculatorTypes.Euro => new EuroCalculatorButtonBuilder(tag, emote),
                CalculatorTypes.Real => new RealCalculatorButtonBuilder(tag, emote),
                CalculatorTypes.FiatCurrency => new FiatCurrencyCalculatorButtonBuilder(tag, emote),
                CalculatorTypes.Venezuela => new VzlaCalculatorButtonBuilder(tag, emote),
                _ => null,
            };
            if (calculatorButton != null)
            {
                WithButton(calculatorButton);
            }
        }
    }
}
