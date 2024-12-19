using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Responses
{
    internal class ResponseFutures
    {
        [JsonProperty("msg")]
        public string? Message { get; set; } = null;
        [JsonProperty("code")]
        public int Code { get; set; } = 0;
        [JsonProperty("data")]
        public JToken? Data { get; set; } = null;
    }
}
