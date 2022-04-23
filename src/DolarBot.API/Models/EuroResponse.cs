using DolarBot.API.Enums;
using DolarBot.API.Models.Base;
using Newtonsoft.Json;
using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class EuroResponse : CurrencyResponse
    {
        [JsonIgnore]
        public EuroEndpoints Type { get; set; }
    }
}
