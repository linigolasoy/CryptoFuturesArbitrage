using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Websocket
{
    /// <summary>
    /// Websocket channel message
    /// </summary>
    internal class ChannelMessage
    {
        [JsonProperty("channel")]
        public string? Channel { get; set; } = null;

        [JsonProperty("symbol")]
        public string? Symbol { get; set; } = null;

        [JsonProperty("data")]
        public JToken? Data { get; set; } = null;
        [JsonProperty("ts")]
        public long? UnixTime { get; set; } = null;
    }
}
