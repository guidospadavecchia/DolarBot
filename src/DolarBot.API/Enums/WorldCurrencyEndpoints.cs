using System.ComponentModel;

namespace DolarBot.API.Enums
{
    public enum WorldCurrencyEndpoints
    {
        [Description("/api/monedas/valor")]
        Base,
        [Description("/api/monedas/lista")]
        List,
        [Description("/api/monedas/historico")]
        Historical
    }
}
