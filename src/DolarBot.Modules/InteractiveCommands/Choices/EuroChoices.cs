﻿using Discord.Interactions;

namespace DolarBot.Modules.InteractiveCommands.Choices
{
    /// <summary>
    /// The available choices for the Euro command.
    /// </summary>
    public enum EuroChoices
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
    }
}
