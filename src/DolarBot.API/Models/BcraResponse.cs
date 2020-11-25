using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class BcraResponse
    {
        public DateTime Fecha { get; set; }
        public string Valor { get; set; }
        public string Moneda { get; set; }
    }
}
