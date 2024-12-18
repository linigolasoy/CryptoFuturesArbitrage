using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Responses
{

    internal class FundingHistoryParsed
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("fundingRate")]
        public double FundingRate { get; set; } = 0;
        [JsonProperty("settleTime")]
        public long SettleTime { get; set; } = 0;   
        [JsonProperty("collectCycle")]
        public int Cycle { get; set; } = 0; 
    }

    internal class FundingHistoryPageParsed
    {
        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = 0;
        [JsonProperty("totalCount")]
        public int Count { get; set; } = 0;
        [JsonProperty("totalPage")]
        public int Pages { get; set; } = 0;

        [JsonProperty("resultList")]
        public List<FundingHistoryParsed>? History { get; set; } = null;

    }

    internal class FuturesFundingParsed
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty; // symbol	string	the name of the contract
        [JsonProperty("fundingRate")]
        public double FundingRate { get; set; } = 0; // fundingRate	decimal	funding rate
        [JsonProperty("maxFundingRate")]
        public double MaxFundingRate { get; set; } = 0; // maxFundingRate	decimal	max funding rate
        [JsonProperty("minFundingRate")]
        public double MinFundingRate { get; set; } = 0; // minFundingRate	decimal	min funding rate
        [JsonProperty("collectCycle")]
        public int CollectCycle { get; set; } = 0; // collectCycle	int	charge cycle
        [JsonProperty("nextSettleTime")]
        public long NextSettleTime { get; set; } = 0; // nextSettleTime	long	next charge time
        [JsonProperty("timestamp")]
        public long ActualTimestamp { get; set; } = 0; // timestamp	long	system timestamp
    }
}
