using Newtonsoft.Json;
using System;
using DollarType = DolarBot.API.ApiCalls.DolarArgentinaApi.DollarType;

namespace DolarBot.API.Models
{
    [Serializable]
    public class DolarResponse
    {
        [JsonIgnore]
        public DollarType Type { get; set; }
        public DateTime Fecha { get; set; }
        public string Compra { get; set; }
        public string Venta { get; set; }
    }
}
