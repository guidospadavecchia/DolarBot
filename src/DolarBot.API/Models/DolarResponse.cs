﻿using Newtonsoft.Json;
using System;
using DollarTypes = DolarBot.API.ApiCalls.DolarArgentinaApi.DollarTypes;

namespace DolarBot.API.Models
{
    [Serializable]
    public class DolarResponse
    {
        [JsonIgnore]
        public DollarTypes Type { get; set; }
        public DateTime Fecha { get; set; }
        public string Compra { get; set; }
        public string Venta { get; set; }
    }
}
