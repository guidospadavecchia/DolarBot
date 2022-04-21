using Discord.Interactions;

namespace DolarBot.Modules.InteractiveCommands.Choices
{
    /// <summary>
    /// The available choices of banks for the dollar command.
    /// </summary>
    public enum DollarBankChoices
    {
        [ChoiceDisplay("Banco Nación")]
        Nacion,
        [ChoiceDisplay("Banco BBVA")]
        BBVA,
        [ChoiceDisplay("Banco Piano")]
        Piano,
        [ChoiceDisplay("Banco Hipotecario")]
        Hipotecario,
        [ChoiceDisplay("Banco Galicia")]
        Galicia,
        [ChoiceDisplay("Banco Santander")]
        Santander,
        [ChoiceDisplay("Banco Ciudad")]
        Ciudad,
        [ChoiceDisplay("Banco Supervielle")]
        Supervielle,
        [ChoiceDisplay("Banco Patagonia")]
        Patagonia,
        [ChoiceDisplay("Banco Comafi")]
        Comafi,
        [ChoiceDisplay("Banco de Córdoba")]
        Bancor,
        [ChoiceDisplay("Nuevo Banco del Chaco")]
        Chaco,
        [ChoiceDisplay("Banco de La Pampa")]
        Pampa,
        [ChoiceDisplay("Banco ICBC")]
        ICBC,
        [ChoiceDisplay("Banco Provincia")]
        Provincia,
        [ChoiceDisplay("Rebanking")]
        Reba,
        [ChoiceDisplay("Banco Roela")]
        Roela
    }
}
