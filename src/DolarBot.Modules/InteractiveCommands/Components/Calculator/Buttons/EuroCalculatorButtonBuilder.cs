using Discord;

namespace DolarBot.Modules.InteractiveCommands.Components.Calculator.Buttons
{
    /// <summary>
    /// A button builder for a calculator component.
    /// </summary>
    public class EuroCalculatorButtonBuilder : ButtonBuilder
    {
        private const ButtonStyle STYLE = ButtonStyle.Secondary;
        private const string DEFAULT_EMOJI = ":1234:";
        public const string Id = "euro_calculator_button";

        /// <summary>
        /// Creates a new <see cref="EuroCalculatorButtonBuilder"/>.
        /// </summary>
        /// <param name="tag">A custom tag to identify the component or pass data.</param>
        public EuroCalculatorButtonBuilder(string tag, IEmote emoji = null) : base(null, $"{Id}:{tag}", STYLE, null, emoji ?? Emoji.Parse(DEFAULT_EMOJI), false) { }
    }
}
