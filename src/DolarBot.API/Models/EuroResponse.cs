using DolarBot.API.Models.Base;
using Newtonsoft.Json;
using System;
using EuroTypes = DolarBot.API.ApiCalls.DolarArgentinaApi.EuroTypes;

namespace DolarBot.API.Models
{
    [Serializable]
    public class EuroResponse : CurrencyResponse
    {
        [JsonIgnore]
        public EuroTypes Type { get; set; }
    }
}
