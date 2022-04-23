using System.ComponentModel;

namespace DolarBot.API.Enums
{
    /// <summary>
    /// Represents the different API endpoints for Venezuela (Bolivar to Dollar) rates.
    /// </summary>
    public enum VenezuelaEndpoints
    {
        [Description("/api/vzla/dolar")]
        Dollar,
        [Description("/api/vzla/euro")]
        Euro,
    }
}
