using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class CryptoResponse
    {
        public DateTime Date { get; set; }
        public CryptoImageResponse Image { get; set; }
        public string ARS { get; set; }
        public string ARSTaxed { get; set; }
        public string USD { get; set; }
        public string Code { get; set; }
        public string Usd24hChange { get; set; }
        public string Usd24hVolume { get; set; }
        public string UsdMarketCap { get; set; }
    }
}
