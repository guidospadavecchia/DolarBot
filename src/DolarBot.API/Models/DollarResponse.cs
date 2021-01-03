using DolarBot.API.Models.Base;
using Newtonsoft.Json;
using System;
using DollarTypes = DolarBot.API.ApiCalls.DolarBotApi.DollarTypes;

namespace DolarBot.API.Models
{
    [Serializable]
    public class DollarResponse : CurrencyResponse
    {
        [JsonIgnore]
        public DollarTypes Type { get; set; }
    }
}
