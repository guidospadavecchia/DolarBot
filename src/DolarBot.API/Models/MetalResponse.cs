using Newtonsoft.Json;
using System;
using Metals = DolarBot.API.ApiCalls.DolarBotApi.Metals;

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
        public Metals Type { get; set; }
    }
}
