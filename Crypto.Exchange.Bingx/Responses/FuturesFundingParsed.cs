using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Responses
{
    internal class FundingHistoryParsed
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("fundingRate")]
        public double FundingRate { get; set; } = 0;
        [JsonProperty("fundingTime")]
        public long SettleTime { get; set; } = 0;
    }
}
