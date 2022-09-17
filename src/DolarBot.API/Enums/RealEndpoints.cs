using System.ComponentModel;

namespace DolarBot.API.Enums
{
    /// <summary>
    /// Represents the different API endpoints for Real rates.
    /// </summary>
    public enum RealEndpoints
    {
        [Description("/api/real/oficial")]
        Oficial,
        [Description("/api/real/ahorro")]
        Ahorro,
        [Description("/api/real/tarjeta")]
        Tarjeta,
        [Description("/api/real/blue")]
        Blue,
        [Description("/api/real/bancos/nacion")]
        Nacion,
        [Description("/api/real/bancos/bbva")]
        BBVA,
        [Description("/api/real/bancos/chaco")]
        Chaco,
        [Description("/api/real/bancos/piano")]
        Piano,
        [Description("/api/real/bancos/ciudad")]
        Ciudad,
        [Description("/api/real/bancos/supervielle")]
        Supervielle
    }
}
