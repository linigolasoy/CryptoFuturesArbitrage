using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Responses
{
    /// <summary>
    /// Futures request response
    /// </summary>
    internal class ResponseFutures
    {
        [JsonProperty("success")]
        public bool Success { get; set; } = false;
        [JsonProperty("code")]
        public int Code { get; set; } = 0;
        [JsonProperty("data")]
        public JToken? Data { get; set; } = null;

    }
}
