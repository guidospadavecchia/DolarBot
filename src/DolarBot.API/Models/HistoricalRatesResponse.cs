using System;
using System.Collections.Generic;

namespace DolarBot.API.Models
{
    [Serializable]
    public class HistoricalRatesResponse
    {
        public DateTime Fecha { get; set; }
        public List<HistoricalMonthlyRate> Meses { get; set; }
    }
}