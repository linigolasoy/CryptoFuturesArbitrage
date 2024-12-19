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

    internal class FundingParsed
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty; // symbol string trading pair, for example: BTC...
        [JsonProperty("lastFundingRate")]
        public string FundingRate { get; set; } = string.Empty; // lastFundingRate string Last updated funding rate
        [JsonProperty("markPrice")]
        public string MarkPrice { get; set; } = string.Empty; // markPrice string current mark price
        [JsonProperty("indexPrice")]
        public string IndexPrice { get; set; } = string.Empty; // indexPrice string index price
        [JsonProperty("nextFundingTime")]
        public long NextSettleTime { get; set; } = 0; // nextSettleTime	long	next charge time

        /*
            
            
            nextFundingTime int64 The remaining time for the nex...
        */
    }

}
