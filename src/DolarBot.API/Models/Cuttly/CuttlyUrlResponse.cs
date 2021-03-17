using Newtonsoft.Json;

namespace DolarBot.API.Models.Cuttly
{
    public class CuttlyUrlResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("fullLink")]
        public string FullLink { get; set; }
        [JsonProperty("shortLink")]
        public string ShortLink { get; set; }
    }
}
