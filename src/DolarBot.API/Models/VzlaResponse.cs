using DolarBot.API.Enums;
using Newtonsoft.Json;
using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class VzlaResponse
    {
        public DateTime Fecha { get; set; }
        public string Paralelo { get; set; }
        public string Bancos { get; set; }
        public string Cucuta { get; set; }

        [JsonIgnore]
        public VenezuelaEndpoints Type;
    }
}
