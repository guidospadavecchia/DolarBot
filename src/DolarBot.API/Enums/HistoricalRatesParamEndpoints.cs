using System.ComponentModel;

namespace DolarBot.API.Enums
{
    /// <summary>
    /// Represents the different API endpoints for historical rates.
    /// </summary>
    public enum HistoricalRatesParamEndpoints
    {
        [Description("/api/evolucion/dolar/oficial")]
        Dolar,
        [Description("/api/evolucion/dolar/ahorro")]
        DolarBlue,
        [Description("/api/evolucion/dolar/blue")]
        DolarAhorro,
        [Description("/api/evolucion/euro/oficial")]
        Euro,
        [Description("/api/evolucion/euro/ahorro")]
        EuroAhorro,
        [Description("/api/evolucion/real/oficial")]
        Real,
        [Description("/api/evolucion/real/ahorro")]
        RealAhorro
    }
}
