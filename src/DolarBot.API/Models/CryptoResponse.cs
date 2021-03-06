﻿using Newtonsoft.Json;
using System;
using CryptoCurrencies = DolarBot.API.ApiCalls.DolarBotApi.CryptoCurrencies;

namespace DolarBot.API.Models
{
    [Serializable]
    public class CryptoResponse
    {
        public DateTime Fecha { get; set; }
        public string ARS { get; set; }
        public string ARSTaxed { get; set; }
        public string USD { get; set; }
        public string Code { get; set; }

        [JsonIgnore]
        public CryptoCurrencies Currency;
    }
}
