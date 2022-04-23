using System.ComponentModel;

namespace DolarBot.API.Enums
{
    /// <summary>
    /// Represents the different API endpoints for crypto.
    /// </summary>
    public enum CryptoEndpoints
    {
        [Description("/api/crypto")]
        Crypto,
        [Description("/api/crypto/list")]
        List
    }
}
