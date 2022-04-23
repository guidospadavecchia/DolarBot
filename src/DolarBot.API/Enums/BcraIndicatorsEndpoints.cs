using System.ComponentModel;

namespace DolarBot.API.Enums
{
    /// <summary>
    /// Represents the different API endpoints for BCRA values.
    /// </summary>
    public enum BcraIndicatorsEndpoints
    {
        [Description("/api/bcra/reservas")]
        Reservas,
        [Description("/api/bcra/circulante")]
        Circulante,
        [Description("/api/bcra/riesgopais")]
        RiesgoPais
    }
}
