using System.ComponentModel;

namespace DolarBot.Services.Currencies
{
    /// <summary>
    /// Represents the available currencies.
    /// </summary>
    public enum Currencies
    {
        [Description("Dolar")]
        Dolar,
        [Description("Euro")]
        Euro,
        [Description("Real")]
        Real
    }
}
