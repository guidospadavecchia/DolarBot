using System.ComponentModel;

namespace DolarBot.API.Enums
{
    /// <summary>
    /// Represents the different API endpoints for euro rates.
    /// </summary>
    public enum EuroEndpoints
    {
        [Description("/api/euro/oficial")]
        Oficial,
        [Description("/api/euro/ahorro")]
        Ahorro,
        [Description("/api/euro/tarjeta")]
        Tarjeta,
        [Description("/api/euro/blue")]
        Blue,
        [Description("/api/euro/bancos/nacion")]
        Nacion,
        [Description("/api/euro/bancos/galicia")]
        Galicia,
        [Description("/api/euro/bancos/bbva")]
        BBVA,
        [Description("/api/euro/bancos/hipotecario")]
        Hipotecario,
        [Description("/api/euro/bancos/pampa")]
        Pampa,
        [Description("/api/euro/bancos/chaco")]
        Chaco,
        [Description("/api/euro/bancos/piano")]
        Piano,
        [Description("/api/euro/bancos/santander")]
        Santander,
        [Description("/api/euro/bancos/ciudad")]
        Ciudad,
        [Description("/api/euro/bancos/supervielle")]
        Supervielle,
        [Description("/api/euro/bancos/patagonia")]
        Patagonia,
        [Description("/api/euro/bancos/comafi")]
        Comafi,
        [Description("/api/euro/bancos/reba")]
        Reba,
        [Description("/api/euro/bancos/roela")]
        Roela
    }
}
