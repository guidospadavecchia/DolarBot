using Discord;
using Discord.Interactions;

namespace DolarBot.Modules.InteractiveCommands.Components.Calculator.Modals
{
    /// <summary>
    /// A calculator modal.
    /// </summary>
    public class DolarCalculatorModal : IModal
    {
        public const string Id = "dolar_calculator";
        public const string TITLE = "Modificar monto";
        public const string LABEL = "Monto (USD):";
        public const string PLACEHOLDER = "1.00";
        public const int MAX_LENGTH = 10;

        /// <summary>
        /// The user value.
        /// </summary>
        [InputLabel(LABEL)]
        [ModalTextInput(Id, TextInputStyle.Short, PLACEHOLDER, maxLength: MAX_LENGTH)]
        public string Value { get; set; }

        /// <summary>
        /// The modal title.
        /// </summary>
        public string Title => TITLE;
    }
}
