using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class HistoricalMonthlyRate
    {
        public string Anio { get; set; }
        public string Mes { get; set; }
        public string Valor { get; set; }
    }
}
