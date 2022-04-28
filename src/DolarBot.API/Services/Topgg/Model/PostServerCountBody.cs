using Newtonsoft.Json;

namespace DolarBot.API.Services.Topgg.Model
{
    public sealed class PostServerCountBody
    {
        [JsonProperty("server_count")]
        public int Count { get; set; }
    }
}
