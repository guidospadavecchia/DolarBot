using System;

namespace DolarBot.API.Models
{
    [Serializable]
    public class CryptoImageResponse
    {
        public string Large { get; set; }
        public string Small { get; set; }
        public string Thumb { get; set; }
    }
}
