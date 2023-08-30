using Discord.Interactions;

namespace DolarBot.Modules.InteractiveCommands.Choices
{
    /// <summary>
    /// The available choices for the dollar command.
    /// </summary>
    public enum DollarChoices
    {
        [ChoiceDisplay("Oficial")]
        Oficial,
        [ChoiceDisplay("Ahorro")]
        Ahorro,
        [ChoiceDisplay("Tarjeta")]
        Tarjeta,
        [ChoiceDisplay("Qatar")]
        Qatar,
        [ChoiceDisplay("Blue")]
        Blue,
        [ChoiceDisplay("Promedio")]
        Promedio,
        [ChoiceDisplay("Bolsa")]
        Bolsa,
        [ChoiceDisplay("Contado con Liquidación")]
        ContadoConLiquidacion
    }
}
