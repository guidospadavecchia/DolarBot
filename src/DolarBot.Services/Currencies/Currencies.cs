using System.ComponentModel;

namespace DolarBot.Services.Currencies
{
    /// <summary>
    /// Represents the available currencies.
    /// </summary>
    public enum Currencies
    {
        [Description("Dólar")]
        Dolar,
        [Description("Euro")]
        Euro,
        [Description("Real")]
        Real
    }
}
