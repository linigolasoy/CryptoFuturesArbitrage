using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Responses
{
    internal class FuturesBarParsed
    {
        [JsonProperty("open")]
        public double Open { get; set; } = 0;
        [JsonProperty("high")]
        public double High { get; set; } = 0;
        [JsonProperty("low")]
        public double Low { get; set; } = 0;
        [JsonProperty("close")]
        public double Close { get; set; } = 0;
        [JsonProperty("volume")]
        public double Volume { get; set; } = 0;
        [JsonProperty("time")]
        public long UnixTime { get; set; } = 0;
    }
}
