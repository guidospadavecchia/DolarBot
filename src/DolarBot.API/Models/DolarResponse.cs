using DolarBot.API.Models.Base;
using Newtonsoft.Json;
using System;
using DollarTypes = DolarBot.API.ApiCalls.DolarArgentinaApi.DollarTypes;

namespace DolarBot.API.Models
{
    [Serializable]
    public class DolarResponse : CurrencyResponse
    {
        [JsonIgnore]
        public DollarTypes Type { get; set; }
    }
}
