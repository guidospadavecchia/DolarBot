using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class DolarResponse
    {
        public DateTime Fecha { get; set; }
        public decimal Compra { get; set; }
        public decimal Venta { get; set; }
    }
}
