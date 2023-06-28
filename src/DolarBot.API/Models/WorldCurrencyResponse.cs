using Newtonsoft.Json;
using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class WorldCurrencyResponse
    {
        public DateTime Fecha { get; set; }
        public string? Valor { get; set; }
        [JsonIgnore]
        public string? Code { get; set; }
    }
}
