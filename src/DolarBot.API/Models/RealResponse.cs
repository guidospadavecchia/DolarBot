﻿using DolarBot.API.Models.Base;
using Newtonsoft.Json;
using System;
using RealTypes = DolarBot.API.ApiCalls.DolarBotApi.RealTypes;

namespace DolarBot.API.Models
{
    [Serializable]
    public class RealResponse : CurrencyResponse
    {
        [JsonIgnore]
        public RealTypes Type { get; set; }
    }
}
