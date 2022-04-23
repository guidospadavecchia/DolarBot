using DolarBot.API.Enums;
using Newtonsoft.Json;
using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class MetalResponse
    {
        public DateTime Fecha { get; set; }
        public string Valor { get; set; }
        public string Unidad { get; set; }
        public string Moneda { get; set; }
        [JsonIgnore]
        public MetalEndpoints Type { get; set; }
    }
}
