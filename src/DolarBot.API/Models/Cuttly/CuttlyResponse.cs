using Newtonsoft.Json;

namespace DolarBot.API.Models.Cuttly
{
    public class CuttlyResponse
    {
        [JsonProperty("url")]
        public CuttlyUrlResponse Url { get; set; }
    }
}
