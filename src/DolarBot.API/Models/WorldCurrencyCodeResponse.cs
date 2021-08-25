using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class WorldCurrencyCodeResponse
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
