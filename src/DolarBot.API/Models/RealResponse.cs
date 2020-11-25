using Newtonsoft.Json;
using System;
using RealTypes = DolarBot.API.ApiCalls.DolarArgentinaApi.RealTypes;

namespace DolarBot.API.Models
{
    [Serializable]
    public class RealResponse
    {
        [JsonIgnore]
        public RealTypes Type { get; set; }
        public DateTime Fecha { get; set; }
        public string Compra { get; set; }
        public string Venta { get; set; }
    }
}
