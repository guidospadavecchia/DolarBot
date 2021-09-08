using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class CryptoCodeResponse
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
    }
}
