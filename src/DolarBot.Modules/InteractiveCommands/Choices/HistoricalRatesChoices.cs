using Discord.Interactions;

namespace DolarBot.Modules.InteractiveCommands.Choices
{
    /// <summary>
    /// The available choices for the historical rates command.
    /// </summary>
    public enum HistoricalRatesChoices
    {
        [ChoiceDisplay("Dólar oficial")]
        Dolar,
        [ChoiceDisplay("Dólar blue")]
        DolarBlue,
        [ChoiceDisplay("Dólar ahorro")]
        DolarAhorro,
        [ChoiceDisplay("Euro oficial")]
        Euro,
        [ChoiceDisplay("Euro ahorro")]
        EuroAhorro,
        [ChoiceDisplay("Real oficial")]
        Real,
        [ChoiceDisplay("Real ahorro")]
        RealAhorro
    }
}
