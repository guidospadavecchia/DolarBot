using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class RiesgoPaisResponse
    {
        public DateTime Fecha { get; set; }
        public decimal Valor { get; set; }
    }
}
