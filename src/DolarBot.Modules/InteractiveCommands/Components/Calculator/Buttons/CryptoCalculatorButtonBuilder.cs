using Discord;
using DolarBot.Util;

namespace DolarBot.Modules.InteractiveCommands.Components.Calculator.Buttons
{
    /// <summary>
    /// A button builder for a calculator component.
    /// </summary>
    public class CryptoCalculatorButtonBuilder : ButtonBuilder
    {
        private const string LABEL = GlobalConfiguration.Constants.BLANK_SPACE;
        private const ButtonStyle STYLE = ButtonStyle.Secondary;
        private const string DEFAULT_EMOJI = ":1234:";
        public const string Id = "crypto_calculator_button";

        /// <summary>
        /// Creates a new <see cref="CryptoCalculatorButtonBuilder"/>.
        /// </summary>
        /// <param name="tag">A custom tag to identify the component or pass data.</param>
        public CryptoCalculatorButtonBuilder(string tag, IEmote emoji = null) : base(LABEL, $"{Id}:{tag}", STYLE, null, emoji ?? Emoji.Parse(DEFAULT_EMOJI), false) { }
    }
}
