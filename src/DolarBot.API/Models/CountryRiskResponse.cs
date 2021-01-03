using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class CountryRiskResponse
    {
        public DateTime Fecha { get; set; }
        public string Valor { get; set; }
    }
}
