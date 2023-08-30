using System.ComponentModel;

namespace DolarBot.API.Enums
{
    /// <summary>
    /// Represents the different API endpoints for dollar rates.
    /// </summary>
    public enum DollarEndpoints
    {
        [Description("/api/dolar/oficial")]
        Oficial,
        [Description("/api/dolar/ahorro")]
        Ahorro,
        [Description("/api/dolar/tarjeta")]
        Tarjeta,
        [Description("/api/dolar/qatar")]
        Qatar,
        [Description("/api/dolar/blue")]
        Blue,
        [Description("/api/dolar/contadoliqui")]
        ContadoConLiqui,
        [Description("/api/dolar/promedio")]
        Promedio,
        [Description("/api/dolar/bolsa")]
        Bolsa,
        [Description("/api/dolar/bancos/nacion")]
        Nacion,
        [Description("/api/dolar/bancos/bbva")]
        BBVA,
        [Description("/api/dolar/bancos/piano")]
        Piano,
        [Description("/api/dolar/bancos/hipotecario")]
        Hipotecario,
        [Description("/api/dolar/bancos/galicia")]
        Galicia,
        [Description("/api/dolar/bancos/santander")]
        Santander,
        [Description("/api/dolar/bancos/ciudad")]
        Ciudad,
        [Description("/api/dolar/bancos/supervielle")]
        Supervielle,
        [Description("/api/dolar/bancos/patagonia")]
        Patagonia,
        [Description("/api/dolar/bancos/comafi")]
        Comafi,
        [Description("/api/dolar/bancos/bancor")]
        Bancor,
        [Description("/api/dolar/bancos/chaco")]
        Chaco,
        [Description("/api/dolar/bancos/pampa")]
        Pampa,
        [Description("/api/dolar/bancos/provincia")]
        Provincia,
        [Description("/api/dolar/bancos/icbc")]
        ICBC,
        [Description("/api/dolar/bancos/reba")]
        Reba,
        [Description("/api/dolar/bancos/roela")]
        Roela,
    }
}
