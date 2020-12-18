using System;

namespace DolarBot.API.Models.Base
{
    [Serializable]
    public abstract class CurrencyResponse : ICloneable
    {
        public DateTime Fecha { get; set; }
        public string Compra { get; set; }
        public string Venta { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}