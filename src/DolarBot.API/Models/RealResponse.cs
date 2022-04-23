using DolarBot.API.Enums;
using DolarBot.API.Models.Base;
using Newtonsoft.Json;
using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class RealResponse : CurrencyResponse
    {
        [JsonIgnore]
        public RealEndpoints Type { get; set; }
    }
}
