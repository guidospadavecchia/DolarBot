using Newtonsoft.Json;
using System;
using EuroTypes = DolarBot.API.ApiCalls.DolarArgentinaApi.EuroTypes;

namespace DolarBot.API.Models
{
    [Serializable]
    public class EuroResponse
    {
        [JsonIgnore]
        public EuroTypes Type { get; set; }
        public DateTime Fecha { get; set; }
        public string Compra { get; set; }
        public string Venta { get; set; }
    }
}
