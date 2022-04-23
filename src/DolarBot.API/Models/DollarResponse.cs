using DolarBot.API.Enums;
using DolarBot.API.Models.Base;
using Newtonsoft.Json;
using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class DollarResponse : CurrencyResponse
    {
        [JsonIgnore]
        public DollarEndpoints Type { get; set; }
    }
}
