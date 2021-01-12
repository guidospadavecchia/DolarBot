using Newtonsoft.Json;
using System;
using static DolarBot.API.ApiCalls.DolarBotApi;

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
        public VenezuelaTypes Type;
    }
}
