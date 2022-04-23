using System.ComponentModel;

namespace DolarBot.API.Enums
{
    /// <summary>
    /// Represents the different API endpoints for precious metals rates.
    /// </summary>
    public enum MetalEndpoints
    {
        [Description("/api/metales/oro")]
        Gold,
        [Description("/api/metales/plata")]
        Silver,
        [Description("/api/metales/cobre")]
        Copper
    }
}
