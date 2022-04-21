using Discord.Interactions;

namespace DolarBot.Modules.InteractiveCommands.Choices
{
    /// <summary>
    /// The available choices of banks for the Euro command.
    /// </summary>
    public enum RealBankChoices
    {
        [ChoiceDisplay("Banco Nación")]
        Nacion,
        [ChoiceDisplay("Banco BBVA")]
        BBVA,
        [ChoiceDisplay("Banco Piano")]
        Piano,
        [ChoiceDisplay("Banco Ciudad")]
        Ciudad,
        [ChoiceDisplay("Banco Supervielle")]
        Supervielle,
        [ChoiceDisplay("Nuevo Banco del Chaco")]
        Chaco,
    }
}
